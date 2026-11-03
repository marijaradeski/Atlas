using AtlasDomain.Compliance.Policies;

namespace AtlasApplication.Compliance.AssessCustomerCompliance;

public sealed record AssessCustomerComplianceCommand(
    Guid CustomerApplicationId,
    MarketCode Market,
    bool IsNonCitizenResident);