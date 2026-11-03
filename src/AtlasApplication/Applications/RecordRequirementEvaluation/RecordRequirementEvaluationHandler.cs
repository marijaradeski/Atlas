using AtlasApplication.Abstractions;
using AtlasDomain.Compliance;

namespace AtlasApplication.Applications.RecordRequirementEvaluation;

public sealed class RecordRequirementEvaluationHandler(
    IComplianceAssessmentRepository assessmentRepository,
    IUnitOfWork unitOfWork)
{
    public async Task<RecordRequirementEvaluationResult> HandleAsync(RecordRequirementEvaluationCommand command, CancellationToken cancellationToken = default)
    {
        if (command.StaffUserId == Guid.Empty)
            throw new ArgumentException("Staff user ID is required.");

        if (command.AssessmentId == Guid.Empty)
            throw new ArgumentException("Assessment ID is required.");

        if (string.IsNullOrWhiteSpace(command.RequirementCode))
            throw new ArgumentException("Requirement code is required.");

        if (!Enum.IsDefined(command.Outcome) ||
            command.Outcome == EvaluationOutcome.Pending)
        {
            throw new ArgumentException("A valid final evaluation outcome is required.");
        }

        var assessment = await assessmentRepository.GetByIdAsync(command.AssessmentId, cancellationToken);

        if (assessment is null)
            throw new KeyNotFoundException("Compliance assessment was not found.");

        var evaluation = assessment.Evaluations.SingleOrDefault(x => x.RequirementCode == command.RequirementCode);

        if (evaluation is null)
            throw new ArgumentException("The requirement does not exist in this assessment.");

        if (evaluation.Outcome != EvaluationOutcome.Pending)
            throw new InvalidOperationException("Only pending requirements can be evaluated.");

        assessment.RecordEvaluation(
            command.RequirementCode,
            command.Outcome,
            command.Reason,
            DateTime.UtcNow,
            command.StaffUserId);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return new RecordRequirementEvaluationResult(
            assessment.Id,
            assessment.Status,
            assessment.Result,
            assessment.Evaluations);
    }
}



