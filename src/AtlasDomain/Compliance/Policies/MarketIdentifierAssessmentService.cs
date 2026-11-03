using AtlasDomain.Compliance;

namespace AtlasDomain.Compliance.Policies;

public sealed class MarketIdentifierAssessmentService
{
    private readonly NationalIdentifierValidator _validator;

    public MarketIdentifierAssessmentService(NationalIdentifierValidator validator)
    {
        _validator = validator ?? throw new ArgumentNullException(nameof(validator));
    }

    public void Evaluate(ComplianceAssessment assessment, MarketCode market, string? nationalIdentifier, DateTime evaluatedAtUtc)
    {
        ArgumentNullException.ThrowIfNull(assessment);

        var errors = _validator.Validate(market, nationalIdentifier);

        if (errors.Count == 0)
        {
            assessment.RecordEvaluation("NATIONAL_IDENTIFIER",
                EvaluationOutcome.Satisfied,
                null,
                evaluatedAtUtc);

            return;
        }

        var checkDigitPending =
            market == MarketCode.ME &&
            errors.Count == 1 &&
            errors.Single().Contains(
                "check-digit validation is not yet implemented",
                StringComparison.OrdinalIgnoreCase);

        if (checkDigitPending)
        {
            assessment.RecordEvaluation(
                "NATIONAL_IDENTIFIER",
                EvaluationOutcome.RequiresReview,
                "Identifier structure is valid, but the official ME check-digit algorithm has not yet been implemented.",
                evaluatedAtUtc);

            return;
        }

        assessment.RecordEvaluation(
            "NATIONAL_IDENTIFIER",
            EvaluationOutcome.NotSatisfied,
            string.Join(" ", errors),
            evaluatedAtUtc);
    }
}