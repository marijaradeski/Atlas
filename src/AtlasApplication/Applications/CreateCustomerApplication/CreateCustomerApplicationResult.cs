using AtlasDomain;

namespace AtlasApplication.Applications.CreateCustomerApplication;

public sealed record CreateCustomerApplicationResult(Guid ApplicationId, ApplicationStatus Status, DateTime CreatedAtUtc);