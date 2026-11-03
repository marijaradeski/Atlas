
using AtlasApplication.Abstractions;
using AtlasApplication.Compliance.AssessCustomerCompliance;
using AtlasDomain.Compliance.Policies;
using AtlasInfrastructure.Persistence;
using AtlasInfrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using AtlasInfrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using AtlasApplication.Applications.CreateCustomerApplication;
using AtlasAPI.Endpoints;
using AtlasApplication.Applications.GetCustomerApplication;
using AtlasApplication.Applications.RecordRequirementEvaluation;
using System.Text.Json.Serialization;
using AtlasAPI.Endpoints.Authentication;
using System.Security.Claims;
using AtlasAPI.Identity;
using AtlasDomain.Compliance;
using AtlasAPI.Errors;
using Microsoft.AspNetCore.RateLimiting;
using System.Threading.RateLimiting;

var builder = WebApplication.CreateBuilder(args);

// Database configuration
var connectionString =
    builder.Configuration.GetConnectionString("AtlasDatabase")
    ?? throw new InvalidOperationException(
        "The AtlasDatabase connection string is not configured.");

builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.Converters.Add(
        new JsonStringEnumConverter());
});

builder.Services.AddDbContext<AtlasDbContext>(options =>
    options.UseSqlServer(connectionString));
// Staff identity
builder.Services
    .AddIdentityCore<AtlasStaffUser>(options =>
    {
        options.User.RequireUniqueEmail = true;
        options.Password.RequiredLength = 12;
        options.Password.RequireDigit = true;
        options.Password.RequireUppercase = true;
        options.Password.RequireLowercase = true;
        options.Password.RequireNonAlphanumeric = true;
        options.Lockout.MaxFailedAccessAttempts = 5;
        options.Lockout.DefaultLockoutTimeSpan =
            TimeSpan.FromMinutes(15);
    })
    .AddRoles<IdentityRole<Guid>>()
    .AddEntityFrameworkStores<AtlasDbContext>();

// JWT authentication
var jwtIssuer = builder.Configuration["Jwt:Issuer"]
    ?? throw new InvalidOperationException("JWT issuer is missing.");

var jwtAudience = builder.Configuration["Jwt:Audience"]
    ?? throw new InvalidOperationException("JWT audience is missing.");

var jwtSigningKey = builder.Configuration["Jwt:SigningKey"]
    ?? throw new InvalidOperationException("JWT signing key is missing.");

byte[] signingKeyBytes;

try
{
    signingKeyBytes = Convert.FromBase64String(jwtSigningKey);
}
catch (FormatException)
{
    throw new InvalidOperationException(
        "JWT signing key must be valid Base64.");
}

if (signingKeyBytes.Length < 32)
    throw new InvalidOperationException("JWT signing key must be at least 32 bytes.");

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = jwtIssuer,
            ValidateAudience = true,
            ValidAudience = jwtAudience,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(signingKeyBytes),
            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromSeconds(30),
            RoleClaimType = ClaimTypes.Role,
        };
    });

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("ComplianceStaff", policy =>
        policy.RequireAuthenticatedUser()
              .RequireRole("ComplianceStaff"));
});

// Infrastructure dependencies
builder.Services.AddScoped<ICustomerApplicationRepository,
    CustomerApplicationRepository>();

builder.Services.AddScoped<IComplianceAssessmentRepository,
    ComplianceAssessmentRepository>();

builder.Services.AddScoped<IUnitOfWork>(serviceProvider =>
    serviceProvider.GetRequiredService<AtlasDbContext>());

// Domain services
builder.Services.AddScoped<NationalIdentifierValidator>();

// Application handlers
builder.Services.AddScoped<AssessCustomerComplianceHandler>();

// OpenAPI
builder.Services.AddOpenApi();

// Standardized API error responses
builder.Services.AddProblemDetails();

builder.Services.AddScoped<CreateCustomerApplicationHandler>();

builder.Services.AddScoped<GetCustomerApplicationHandler>();
builder.Services.AddScoped<RecordRequirementEvaluationHandler>();

builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

    options.AddPolicy("Login", httpContext =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            factory: _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 5,
                Window = TimeSpan.FromMinutes(1),
                QueueLimit = 0,
                AutoReplenishment = true
            }));

    options.OnRejected = async (context, cancellationToken) =>
    {
        context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;

        await context.HttpContext.Response.WriteAsJsonAsync(
            new Microsoft.AspNetCore.Mvc.ProblemDetails
            {
                Status = StatusCodes.Status429TooManyRequests,
                Title = "Too many requests",
                Detail = "Too many login attempts. Please try again later.",
                Instance = context.HttpContext.Request.Path
            },
            cancellationToken: cancellationToken);
    };
});

var app = builder.Build();

app.UseExceptionHandler();

app.UseRouting();
app.UseRateLimiter();

app.UseAuthentication();
app.UseAuthorization();

if (args.Contains("--provision-staff", StringComparer.OrdinalIgnoreCase))
{
    await StaffProvisioning.RunAsync(app.Services);
    return;
}

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.MapPost("/applications",
    async (
        CreateCustomerApplicationRequest request,
        CreateCustomerApplicationHandler handler,
        CancellationToken cancellationToken) =>
    {
        var command = new CreateCustomerApplicationCommand(
            request.FirstName,
            request.LastName,
            request.DateOfBirth,
            request.Country,
            request.NationalId,
            request.Email,
            request.Phone);

        var result = await handler.HandleAsync(
            command,
            cancellationToken);

        return Results.Created($"/applications/{result.ApplicationId}", result);
    })
    .WithName("CreateCustomerApplication")
    .WithTags("Applications");

app.MapGet("/applications/{id:guid}",
    async (Guid id, GetCustomerApplicationHandler handler, CancellationToken cancellationToken) =>
    {
        var result = await handler.HandleAsync(id, cancellationToken);

        return result is null
            ? Results.Problem(
                statusCode: StatusCodes.Status404NotFound,
                title: "Customer application not found",
                detail: "No customer application exists with the supplied ID.")
            : Results.Ok(result);
    })
    .WithName("GetCustomerApplication")
    .WithTags("Applications");

app.MapPost("/applications/{id:guid}/compliance-assessments",
    async (
        Guid id,
        AssessCustomerComplianceRequest request,
        AssessCustomerComplianceHandler handler,
        CancellationToken cancellationToken) =>
    {
        if (!Enum.TryParse<MarketCode>( request.Market, true, out var market) || !Enum.IsDefined(market))
        {
            throw new ArgumentException("Invalid market. Use MA, MB, MC, MD, ME or MF.");
        }

        var command = new AssessCustomerComplianceCommand(
            id,
            market,
            request.IsNonCitizenResident);

        var result = await handler.HandleAsync(
            command,
            cancellationToken);

        return Results.Ok(result);
    })
    .WithName("AssessCustomerCompliance")
    .WithTags("Compliance");

app.MapAuthenticationEndpoints();

app.MapPost(
    "/compliance-assessments/{id:guid}/evaluations",
    async (
        Guid id,
        RecordRequirementEvaluationRequest request,
        ClaimsPrincipal user,
        RecordRequirementEvaluationHandler handler,
        CancellationToken cancellationToken) =>
    {
        var staffIdValue = user.FindFirstValue(ClaimTypes.NameIdentifier);

        if (!Guid.TryParse(staffIdValue, out var staffUserId)) return Results.Unauthorized();

        if (!Enum.IsDefined(request.Outcome))
        {
            throw new ArgumentException(
                "Invalid evaluation outcome.");
        }

        var command = new RecordRequirementEvaluationCommand(
            id,
            request.RequirementCode,
            request.Outcome,
            request.Reason,
            staffUserId);

        var result = await handler.HandleAsync(
            command,
            cancellationToken);

        return Results.Ok(result);
    })
    .RequireAuthorization("ComplianceStaff")
    .WithName("RecordRequirementEvaluation")
    .WithTags("Compliance");

app.MapGet("/health/live", () =>
    Results.Ok(new
    {
        status = "Healthy"
    }))
    .AllowAnonymous()
    .WithName("Liveness")
    .WithTags("Health");

app.MapGet("/health/ready", async (
    AtlasDbContext dbContext,
    ILogger<Program> logger,
    CancellationToken cancellationToken) =>
{
    try
    {
        var canConnect = await dbContext.Database.CanConnectAsync(cancellationToken);

        if (!canConnect)
        {
            return Results.Problem(
                statusCode: StatusCodes.Status503ServiceUnavailable,
                title: "Service unavailable",
                detail: "The database is currently unavailable.");
        }

        return Results.Ok(new
        {
            status = "Ready",
            database = "Available"
        });
    }
    catch (OperationCanceledException)
        when (cancellationToken.IsCancellationRequested)
    {
        throw;
    }
    catch (Exception exception)
    {
        logger.LogError(
            exception,
            "Database readiness check failed.");

        return Results.Problem(
            statusCode: StatusCodes.Status503ServiceUnavailable,
            title: "Service unavailable",
            detail: "The database is currently unavailable.");
    }
})
.AllowAnonymous()
.WithName("Readiness")
.WithTags("Health");

app.Run();



