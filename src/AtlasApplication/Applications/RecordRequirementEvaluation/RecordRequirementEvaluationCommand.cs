using AtlasDomain.Compliance;

namespace AtlasApplication.Applications.RecordRequirementEvaluation;

public sealed record RecordRequirementEvaluationCommand(
    Guid AssessmentId,
    string RequirementCode,
    EvaluationOutcome Outcome,
    string? Reason,
    Guid StaffUserId);
