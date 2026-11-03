namespace AtlasDomain.Compliance.Policies;

public sealed class NationalIdentifierValidator
{
    public IReadOnlyCollection<string> Validate(MarketCode market, string? nationalIdentifier)
    {
        var errors = new List<string>();

        var requirements = MarketRequirementsCatalog.GetFor(market);

        if (string.IsNullOrWhiteSpace(nationalIdentifier))
        {
            errors.Add("National identifier is required.");
            return errors;
        }

        var identifier = nationalIdentifier.Trim();

        if (requirements.IdentifierLength.HasValue && identifier.Length != requirements.IdentifierLength.Value)
        {
            errors.Add(
                $"Identifier must contain exactly {requirements.IdentifierLength.Value} characters.");
        }

        if (requirements.DigitsOnly && !identifier.All(char.IsDigit))
        {
            errors.Add("Identifier must contain digits only.");
        }

        if (requirements.RequiredLetterPosition.HasValue)
        {
            var position = requirements.RequiredLetterPosition.Value - 1;

            if (identifier.Length >= requirements.RequiredLetterPosition.Value && !char.IsLetter(identifier[position]))
            {
                errors.Add($"Character at position {requirements.RequiredLetterPosition.Value} must be a letter.");
            }
        }

        if (requirements.RequiresMarketSpecificCheckDigit)
        {
            errors.Add("Market-specific check-digit validation is not yet implemented.");
        }

        return errors.AsReadOnly();
    }
}