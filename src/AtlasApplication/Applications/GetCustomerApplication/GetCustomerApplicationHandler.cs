using AtlasApplication.Abstractions;
using AtlasDomain;

namespace AtlasApplication.Applications.GetCustomerApplication;

public sealed record GetCustomerApplicationResult(
    Guid ApplicationId,
    string FirstName,
    string LastName,
    DateOnly DateOfBirth,
    string Country,
    string Email,
    string Phone,
    ApplicationStatus Status,
    DateTime CreatedAtUtc);

public sealed class GetCustomerApplicationHandler(ICustomerApplicationRepository repository)
{
    public async Task<GetCustomerApplicationResult?> HandleAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var application = await repository.GetByIdAsync(id, cancellationToken);

        if (application is null)
            return null;

        return new GetCustomerApplicationResult(
            application.Id,
            application.FirstName,
            application.LastName,
            application.DateOfBirth,
            application.Country,
            application.Email,
            application.Phone,
            application.Status,
            application.CreatedAtUtc);
    }
}
