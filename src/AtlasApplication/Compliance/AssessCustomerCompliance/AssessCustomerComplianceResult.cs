using AtlasDomain.Compliance;

namespace AtlasApplication.Compliance.AssessCustomerCompliance;

public sealed record AssessCustomerComplianceResult(
    Guid AssessmentId,
    AssessmentStatus Status,
    ComplianceResult Result,
    IReadOnlyCollection<RequirementEvaluation> Evaluations);