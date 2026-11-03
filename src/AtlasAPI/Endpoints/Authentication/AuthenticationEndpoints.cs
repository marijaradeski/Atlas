using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using AtlasInfrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.RateLimiting;

namespace AtlasAPI.Endpoints.Authentication;

public static class AuthenticationEndpoints
{
    public static IEndpointRouteBuilder MapAuthenticationEndpoints(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPost("/auth/login", LoginAsync)
            .AllowAnonymous()
            .RequireRateLimiting("Login")
            .WithName("StaffLogin")
            .WithTags("Authentication");

        return endpoints;
    }

    private static async Task<IResult> LoginAsync(LoginRequest request, UserManager<AtlasStaffUser> userManager, IConfiguration configuration)
    {
        if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
        {
            return Results.Problem(
                statusCode: StatusCodes.Status400BadRequest,
                title: "Invalid request",
                detail: "Email and password are required.");
        }

        var user = await userManager.FindByEmailAsync(
            request.Email.Trim());

        if (user is null || !user.EmailConfirmed || await userManager.IsLockedOutAsync(user))
        {
            return Results.Problem(
                statusCode: StatusCodes.Status401Unauthorized,
                title: "Authentication failed",
                detail: "Invalid email or password.");
        }

        var passwordValid = await userManager.CheckPasswordAsync(
            user,
            request.Password);

        if (!passwordValid)
        {
            await userManager.AccessFailedAsync(user);
            return Results.Unauthorized();
        }

        await userManager.ResetAccessFailedCountAsync(user);

        var roles = await userManager.GetRolesAsync(user);

        if (roles.Count == 0)
            return Results.Unauthorized();

        var issuer = configuration["Jwt:Issuer"]
            ?? throw new InvalidOperationException("JWT issuer is missing.");

        var audience = configuration["Jwt:Audience"]
            ?? throw new InvalidOperationException("JWT audience is missing.");

        var encodedKey = configuration["Jwt:SigningKey"]
            ?? throw new InvalidOperationException("JWT signing key is missing.");

        var keyBytes = Convert.FromBase64String(encodedKey);

        if (keyBytes.Length < 32)
            throw new InvalidOperationException("JWT signing key is too short.");

        var expiresAt = DateTime.UtcNow.AddMinutes(15);

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Name, user.UserName ?? user.Email ?? user.Id.ToString()),
            new(ClaimTypes.Email, user.Email ?? string.Empty)
        };

        claims.AddRange(roles.Select(
            role => new Claim(ClaimTypes.Role, role)));

        var credentials = new SigningCredentials(new SymmetricSecurityKey(keyBytes), SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: expiresAt,
            signingCredentials: credentials);

        var tokenString = new JwtSecurityTokenHandler()
            .WriteToken(token);

        return Results.Ok(new LoginResponse(tokenString, "Bearer", expiresAt));
    }
}
