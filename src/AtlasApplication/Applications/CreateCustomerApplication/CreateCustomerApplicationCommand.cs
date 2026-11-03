namespace AtlasApplication.Applications.CreateCustomerApplication;

public sealed record CreateCustomerApplicationCommand(
    string FirstName,
    string LastName,
    DateOnly DateOfBirth,
    string Country,
    string NationalId,
    string Email,
    string Phone);