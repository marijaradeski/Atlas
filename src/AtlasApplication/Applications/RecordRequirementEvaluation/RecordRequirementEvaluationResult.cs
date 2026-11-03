using AtlasDomain.Compliance;

namespace AtlasApplication.Applications.RecordRequirementEvaluation;

public sealed record RecordRequirementEvaluationResult(
    Guid AssessmentId,
    AssessmentStatus Status,
    ComplianceResult Result,
    IReadOnlyCollection<RequirementEvaluation> Evaluations);
