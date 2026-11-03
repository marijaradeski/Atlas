using AtlasDomain;

namespace AtlasApplication.Abstractions;

public interface ICustomerApplicationRepository
{
    Task<CustomerApplication?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    public Task AddAsync(CustomerApplication application, CancellationToken cancellationToken = default);
}