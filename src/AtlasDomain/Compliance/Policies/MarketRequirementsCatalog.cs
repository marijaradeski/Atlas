namespace AtlasDomain.Compliance.Policies;

public static class MarketRequirementsCatalog
{
    private static readonly IReadOnlyDictionary<MarketCode,MarketRequirementDefinition> Requirements =
        new Dictionary<MarketCode, MarketRequirementDefinition>
        {
            [MarketCode.MA] = new(
                MarketCode.MA,
                NationalIdentifierType.PersonalNumber,
                13,
                true,
                null,
                false,
                false,
                false,
                false),

            [MarketCode.MB] = new(
                MarketCode.MB,
                NationalIdentifierType.PersonalNumber,
                13,
                true,
                null,
                false,
                false,
                false,
                false),

            [MarketCode.MC] = new(
                MarketCode.MC,
                NationalIdentifierType.UnifiedCitizenId,
                9,
                false,
                9,
                false,
                false,
                false,
                false),

            [MarketCode.MD] = new(
                MarketCode.MD,
                NationalIdentifierType.CitizenNumber,
                10,
                true,
                null,
                true,
                true,
                false,
                false),

            [MarketCode.ME] = new(
                MarketCode.ME,
                NationalIdentifierType.CivilNumber,
                10,
                true,
                null,
                false,
                false,
                false,
                true),

            [MarketCode.MF] = new(
                MarketCode.MF,
                NationalIdentifierType.PersonalNumber,
                null,
                true,
                null,
                false,
                false,
                true,
                false)
        };

    public static MarketRequirementDefinition GetFor(MarketCode market)
    {
        if (!Requirements.TryGetValue(market, out var requirements))
        {
            throw new ArgumentOutOfRangeException(
                nameof(market),
                market,
                "Unsupported market.");
        }

        return requirements;
    }
}