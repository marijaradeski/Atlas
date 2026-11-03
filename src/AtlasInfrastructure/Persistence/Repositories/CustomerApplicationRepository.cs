using AtlasApplication.Abstractions;
using AtlasDomain;
using AtlasInfrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace AtlasInfrastructure.Persistence.Repositories;

public sealed class CustomerApplicationRepository(AtlasDbContext context): ICustomerApplicationRepository
{
    public async Task<CustomerApplication?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await context.CustomerApplications
            .SingleOrDefaultAsync(
                x => x.Id == id,
                cancellationToken);
    }

    public async Task AddAsync(CustomerApplication application, CancellationToken cancellationToken = default)
    {
        await context.CustomerApplications.AddAsync(application, cancellationToken);
    }
}