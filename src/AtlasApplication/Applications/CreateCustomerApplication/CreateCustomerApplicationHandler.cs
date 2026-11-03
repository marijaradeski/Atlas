using System.Net.Mail;
using AtlasApplication.Abstractions;
using AtlasDomain;

namespace AtlasApplication.Applications.CreateCustomerApplication;

public sealed class CreateCustomerApplicationHandler(
    ICustomerApplicationRepository customerRepository,
    IUnitOfWork unitOfWork)
{
    public async Task<CreateCustomerApplicationResult> HandleAsync(CreateCustomerApplicationCommand command, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);

        Validate(command);

        var createdAtUtc = DateTime.UtcNow;

        var application = CustomerApplication.Create(
            command.FirstName,
            command.LastName,
            command.DateOfBirth,
            command.Country,
            command.NationalId,
            command.Email,
            command.Phone,
            createdAtUtc);

        await customerRepository.AddAsync(
            application,
            cancellationToken);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return new CreateCustomerApplicationResult(application.Id, application.Status, createdAtUtc);
    }

    private static void Validate(CreateCustomerApplicationCommand command)
    {
        if (string.IsNullOrWhiteSpace(command.FirstName) || command.FirstName.Trim().Length > 100)
            throw new ArgumentException("First name is required and must not exceed 100 characters.");

        if (string.IsNullOrWhiteSpace(command.LastName) || command.LastName.Trim().Length > 100)
            throw new ArgumentException("Last name is required and must not exceed 100 characters.");

        if (command.DateOfBirth == default || command.DateOfBirth > DateOnly.FromDateTime(DateTime.UtcNow))
            throw new ArgumentException("A valid date of birth is required.");

        if (string.IsNullOrWhiteSpace(command.Country) || command.Country.Trim().Length > 100)
            throw new ArgumentException("Country is required and must not exceed 100 characters.");

        if (string.IsNullOrWhiteSpace(command.NationalId) || command.NationalId.Trim().Length > 64)
            throw new ArgumentException("National ID is required and must not exceed 64 characters.");

        if (string.IsNullOrWhiteSpace(command.Email) || command.Email.Trim().Length > 320 ||
            !MailAddress.TryCreate(command.Email.Trim(), out _))
            throw new ArgumentException("A valid email address is required.");

        if (string.IsNullOrWhiteSpace(command.Phone))
            throw new ArgumentException("Phone number is required.");

        var phone = command.Phone.Trim();

        var allowedPhoneCharacters = phone.All(c =>
            char.IsDigit(c) ||
            c is '+' or ' ' or '-' or '(' or ')');

        var digitCount = phone.Count(char.IsDigit);

        if (!allowedPhoneCharacters || phone.Length > 32 || digitCount is < 7 or > 15 || (phone.Contains('+') && !phone.StartsWith('+')))
        {
            throw new ArgumentException("Phone number must contain 7 to 15 digits and use a valid international phone format.");
        }
    }
}