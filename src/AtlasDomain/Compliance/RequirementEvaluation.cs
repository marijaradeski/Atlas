namespace AtlasDomain.Compliance;

public class RequirementEvaluation
{
    public Guid Id { get; private set; }

    public string RequirementCode { get; private set; } = string.Empty;

    public string RequirementName { get; private set; } = string.Empty;

    public EvaluationOutcome Outcome { get; private set; }

    public string? Reason { get; private set; }

    public DateTime? EvaluatedAtUtc { get; private set; }

    public Guid? EvaluatedByStaffId { get; private set; }

    public EvaluationOutcome? ResolutionOutcome { get; private set; }

    public string? ResolutionReason { get; private set; }

    public DateTime? ResolvedAtUtc { get; private set; }

    public Guid? ResolvedByStaffId { get; private set; }

    public EvaluationOutcome EffectiveOutcome => ResolutionOutcome ?? Outcome;

    public bool IsReviewResolved => ResolutionOutcome.HasValue;

    internal RequirementEvaluation(
        string requirementCode,
        string requirementName)
    {
        if (string.IsNullOrWhiteSpace(requirementCode))
            throw new ArgumentException("Requirement code is required.");

        if (string.IsNullOrWhiteSpace(requirementName))
            throw new ArgumentException("Requirement name is required.");

        Id = Guid.NewGuid();
        RequirementCode = requirementCode.Trim();
        RequirementName = requirementName.Trim();
        Outcome = EvaluationOutcome.Pending;
    }

    internal void RecordOutcome(EvaluationOutcome outcome, string? reason, DateTime evaluatedAtUtc, Guid? evaluatedByStaffId = null)
    {
        if (Outcome != EvaluationOutcome.Pending)
            throw new InvalidOperationException("This requirement has already been evaluated.");

        if (!Enum.IsDefined(outcome) ||
            outcome == EvaluationOutcome.Pending)
        {
            throw new ArgumentException("Invalid evaluation outcome.");
        }

        if (evaluatedAtUtc.Kind != DateTimeKind.Utc)
            throw new ArgumentException("Timestamp must be UTC.");

        if ((outcome == EvaluationOutcome.NotSatisfied ||
             outcome == EvaluationOutcome.RequiresReview) &&
            string.IsNullOrWhiteSpace(reason))
        {
            throw new ArgumentException("A reason is required for this outcome.");
        }

        var normalizedReason = reason?.Trim();

        if (normalizedReason?.Length > 2000)
            throw new ArgumentException("Reason cannot exceed 2000 characters.");

        Outcome = outcome;
        Reason = normalizedReason;
        EvaluatedAtUtc = evaluatedAtUtc;
        EvaluatedByStaffId = evaluatedByStaffId;
    }

    internal void ResolveReview(EvaluationOutcome outcome, string? reason, DateTime resolvedAtUtc, Guid resolvedByStaffId)
    {
        if (Outcome != EvaluationOutcome.RequiresReview)
            throw new InvalidOperationException("Only requirements requiring manual review can be resolved.");

        if (ResolutionOutcome.HasValue)
            throw new InvalidOperationException("This manual review has already been resolved.");

        if (!Enum.IsDefined(outcome) ||
            outcome is EvaluationOutcome.Pending
                or EvaluationOutcome.RequiresReview)
        {
            throw new ArgumentException("A valid final resolution outcome is required.");
        }

        if (resolvedAtUtc.Kind != DateTimeKind.Utc)
            throw new ArgumentException("Resolution timestamp must be UTC.");

        if (resolvedByStaffId == Guid.Empty)
            throw new ArgumentException("Resolving staff member ID is required.");

        var normalizedReason = reason?.Trim();

        if (outcome == EvaluationOutcome.NotSatisfied &&
            string.IsNullOrWhiteSpace(normalizedReason))
        {
            throw new ArgumentException("A reason is required when resolving as not satisfied.");
        }

        if (normalizedReason?.Length > 2000)
            throw new ArgumentException("Resolution reason cannot exceed 2000 characters.");

        ResolutionOutcome = outcome;
        ResolutionReason = normalizedReason;
        ResolvedAtUtc = resolvedAtUtc;
        ResolvedByStaffId = resolvedByStaffId;
    }
}



