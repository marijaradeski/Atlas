using AtlasApplication.Abstractions;
using AtlasDomain.Compliance;
using AtlasDomain.Compliance.Policies;

namespace AtlasApplication.Compliance.AssessCustomerCompliance;

public sealed class AssessCustomerComplianceHandler
{
    private readonly ICustomerApplicationRepository _customerRepository;
    private readonly IComplianceAssessmentRepository _assessmentRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly NationalIdentifierValidator _validator;

    public AssessCustomerComplianceHandler(
        ICustomerApplicationRepository customerRepository,
        IComplianceAssessmentRepository assessmentRepository,
        IUnitOfWork unitOfWork,
        NationalIdentifierValidator validator)
    {
        _customerRepository = customerRepository;
        _assessmentRepository = assessmentRepository;
        _unitOfWork = unitOfWork;
        _validator = validator;
    }

    public async Task<AssessCustomerComplianceResult> HandleAsync(
        AssessCustomerComplianceCommand command,
        CancellationToken cancellationToken = default)
    {
        if (command.CustomerApplicationId == Guid.Empty)
        {
            throw new ArgumentException(
                "Customer application ID is required.");
        }

        var customerApplication =
            await _customerRepository.GetByIdAsync(
                command.CustomerApplicationId,
                cancellationToken);

        if (customerApplication is null)
        {
            throw new KeyNotFoundException(
                "Customer application was not found.");
        }

        var existingAssessment =
            await _assessmentRepository.GetByCustomerApplicationIdAsync(
                customerApplication.Id,
                cancellationToken);

        if (existingAssessment is not null)
        {
            throw new InvalidOperationException(
                "A compliance assessment already exists for this application.");
        }

        var evaluatedAtUtc = DateTime.UtcNow;

        var assessment = MarketComplianceAssessmentFactory.Create(
            customerApplication.Id,
            command.Market,
            command.IsNonCitizenResident,
            evaluatedAtUtc);

        var identifierAssessmentService =
            new MarketIdentifierAssessmentService(_validator);

        identifierAssessmentService.Evaluate(
            assessment,
            command.Market,
            customerApplication.NationalId,
            evaluatedAtUtc);

        var identifierEvaluation = assessment.Evaluations
    .Single(x => x.RequirementCode == "NATIONAL_IDENTIFIER");

        if (identifierEvaluation.Outcome == EvaluationOutcome.NotSatisfied)
        {
            customerApplication.MarkIdentityVerificationFailed();
        }
        else if (assessment.Status == AssessmentStatus.RequiresManualReview)
        {
            customerApplication.MarkManualReviewRequired();
        }
        else if (assessment.Evaluations.Any(x =>
            x.RequirementCode == "BRANCH_VISIT" &&
            x.Outcome == EvaluationOutcome.Pending))
        {
            customerApplication.MarkAwaitingBranchVisit();
        }
        else if (assessment.Status == AssessmentStatus.Completed &&
                 assessment.Result == ComplianceResult.Compliant)
        {
            customerApplication.MarkScreeningPending();
        }

        await _assessmentRepository.AddAsync(
            assessment,
            cancellationToken);

        await _unitOfWork.SaveChangesAsync(
            cancellationToken);

        return new AssessCustomerComplianceResult(
            assessment.Id,
            assessment.Status,
            assessment.Result,
            assessment.Evaluations);
    }
}