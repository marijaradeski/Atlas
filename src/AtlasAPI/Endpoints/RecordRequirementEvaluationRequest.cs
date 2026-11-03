using AtlasDomain.Compliance;

namespace AtlasAPI.Endpoints;

public sealed record RecordRequirementEvaluationRequest(string RequirementCode, EvaluationOutcome Outcome, string? Reason);
