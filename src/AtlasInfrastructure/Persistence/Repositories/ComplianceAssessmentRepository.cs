using AtlasApplication.Abstractions;
using AtlasDomain.Compliance;
using Microsoft.EntityFrameworkCore;

namespace AtlasInfrastructure.Persistence.Repositories;

public sealed class ComplianceAssessmentRepository(
    AtlasDbContext context)
    : IComplianceAssessmentRepository
{
    public async Task AddAsync(
        ComplianceAssessment assessment,
        CancellationToken cancellationToken = default)
    {
        await context.ComplianceAssessments.AddAsync(
            assessment,
            cancellationToken);
    }

    public async Task<ComplianceAssessment?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        return await context.ComplianceAssessments
            .Include(x => x.Evaluations)
            .SingleOrDefaultAsync(
                x => x.Id == id,
                cancellationToken);
    }

    public async Task<ComplianceAssessment?> GetByCustomerApplicationIdAsync(Guid customerApplicationId, CancellationToken cancellationToken = default)
    {
        return await context.ComplianceAssessments
            .Include(x => x.Evaluations)
            .Where(x => x.CustomerApplicationId == customerApplicationId)
            .OrderByDescending(x => x.CreatedAtUtc)
            .FirstOrDefaultAsync(cancellationToken);
    }
}
