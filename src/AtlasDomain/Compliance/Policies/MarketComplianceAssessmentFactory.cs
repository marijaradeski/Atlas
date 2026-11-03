using AtlasDomain.Compliance;

namespace AtlasDomain.Compliance.Policies;

public static class MarketComplianceAssessmentFactory
{
    public static ComplianceAssessment Create(Guid customerApplicationId, MarketCode market, bool isNonCitizenResident, DateTime createdAtUtc)
    {
        var marketRequirements =  MarketRequirementsCatalog.GetFor(market);

        var assessment = ComplianceAssessment.Create(customerApplicationId, createdAtUtc);

        assessment.AddRequirement("NATIONAL_IDENTIFIER", $"{marketRequirements.IdentifierType} validation");

        if (marketRequirements.RequiresBranchVisit)
        {
            assessment.AddRequirement("BRANCH_VISIT", "Customer must visit a branch");
        }

        if (marketRequirements.RequiresWetSignature)
        {
            assessment.AddRequirement("WET_SIGNATURE", "Customer must provide a wet signature");
        }

        if (marketRequirements.RequiresPassportForNonCitizenResidents && isNonCitizenResident)
        {
            assessment.AddRequirement("PASSPORT", "Non-citizen resident must provide a passport");
        }

        return assessment;
    }
}