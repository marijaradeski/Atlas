namespace AtlasAPI.Endpoints;

public sealed record CreateCustomerApplicationRequest(string FirstName, string LastName, DateOnly DateOfBirth,
    string Country,
    string NationalId,
    string Email,
    string Phone);
