namespace AtlasDomain.Compliance;

public class ComplianceAssessment
{
    private readonly List<RequirementEvaluation> _evaluations = [];

    public Guid Id { get; private set; }

    public Guid CustomerApplicationId { get; private set; }

    public AssessmentStatus Status { get; private set; }

    public DateTime CreatedAtUtc { get; private set; }

    public IReadOnlyCollection<RequirementEvaluation> Evaluations => _evaluations.AsReadOnly();

    public ComplianceResult Result
    {
        get
        {
            if (Status != AssessmentStatus.Completed)
                return ComplianceResult.Undetermined;

            if (_evaluations.Any(x => x.EffectiveOutcome == EvaluationOutcome.NotSatisfied))
            {
                return ComplianceResult.NonCompliant;
            }

            return ComplianceResult.Compliant;
        }
    }


    private ComplianceAssessment(Guid customerApplicationId, DateTime createdAtUtc)
    {
        Id = Guid.NewGuid();
        CustomerApplicationId = customerApplicationId;
        CreatedAtUtc = createdAtUtc;
        Status = AssessmentStatus.InProgress;
    }

    public static ComplianceAssessment Create(Guid customerApplicationId, DateTime createdAtUtc)
    {
        if (customerApplicationId == Guid.Empty)
            throw new ArgumentException("Customer application ID is required.");

        if (createdAtUtc.Kind != DateTimeKind.Utc)
            throw new ArgumentException("Creation timestamp must be UTC.");

        return new ComplianceAssessment(customerApplicationId, createdAtUtc);
    }

    public void AddRequirement(string requirementCode, string requirementName)
    {
        EnsureNotCompleted();

        if (string.IsNullOrWhiteSpace(requirementCode))
            throw new ArgumentException("Requirement code is required.");

        if (_evaluations.Any(x =>
            string.Equals(
                x.RequirementCode,
                requirementCode.Trim(),
                StringComparison.OrdinalIgnoreCase)))
        {
            throw new InvalidOperationException($"Requirement '{requirementCode}' already exists.");
        }

        _evaluations.Add(
            new RequirementEvaluation(
                requirementCode,
                requirementName));

        RecalculateStatus();
    }

    public void RecordEvaluation(
        string requirementCode,
        EvaluationOutcome outcome,
        string? reason,
        DateTime evaluatedAtUtc,
        Guid? evaluatedByStaffId = null)
    {
        EnsureNotCompleted();

        var evaluation = _evaluations.SingleOrDefault(x =>
            string.Equals(
                x.RequirementCode,
                requirementCode,
                StringComparison.OrdinalIgnoreCase));

        if (evaluation is null)
            throw new InvalidOperationException($"Requirement '{requirementCode}' was not found.");

        evaluation.RecordOutcome(
            outcome,
            reason,
            evaluatedAtUtc,
            evaluatedByStaffId);

        RecalculateStatus();
    }

    public void ResolveReview(
    string requirementCode,
    EvaluationOutcome outcome,
    string? reason,
    DateTime resolvedAtUtc,
    Guid resolvedByStaffId)
    {
        EnsureNotCompleted();

        if (string.IsNullOrWhiteSpace(requirementCode))
            throw new ArgumentException("Requirement code is required.");

        var evaluation = _evaluations.SingleOrDefault(x =>
            string.Equals(
                x.RequirementCode,
                requirementCode.Trim(),
                StringComparison.OrdinalIgnoreCase));

        if (evaluation is null)
            throw new InvalidOperationException($"Requirement '{requirementCode}' was not found.");

        evaluation.ResolveReview(
            outcome,
            reason,
            resolvedAtUtc,
            resolvedByStaffId);

        RecalculateStatus();
    }

    private void RecalculateStatus()
    {
        if (_evaluations.Any(
            x => x.Outcome == EvaluationOutcome.RequiresReview &&
                 !x.IsReviewResolved))
        {
            Status = AssessmentStatus.RequiresManualReview;
            return;
        }

        if (_evaluations.Count > 0 &&
            _evaluations.All(
                x => x.EffectiveOutcome != EvaluationOutcome.Pending &&
                    x.EffectiveOutcome != EvaluationOutcome.RequiresReview))
        {
            Status = AssessmentStatus.Completed;
            return;
        }

        Status = AssessmentStatus.InProgress;
    }

    private void EnsureNotCompleted()
    {
        if (Status == AssessmentStatus.Completed)
            throw new InvalidOperationException("A completed assessment cannot be modified.");
    }
}

public enum ComplianceResult
{
    Undetermined = 0,
    Compliant = 1,
    NonCompliant = 2
}
