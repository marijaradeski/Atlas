namespace AtlasDomain.Compliance.Policies;

public enum NationalIdentifierType
{
    PersonalNumber = 1,
    UnifiedCitizenId = 2,
    CitizenNumber = 3,
    CivilNumber = 4
}

public sealed record MarketRequirementDefinition(
    MarketCode Market,
    NationalIdentifierType IdentifierType,
    int? IdentifierLength,
    bool DigitsOnly,
    int? RequiredLetterPosition,
    bool RequiresBranchVisit,
    bool RequiresWetSignature,
    bool RequiresPassportForNonCitizenResidents,
    bool RequiresMarketSpecificCheckDigit);