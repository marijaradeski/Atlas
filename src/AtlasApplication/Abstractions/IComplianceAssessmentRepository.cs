using AtlasDomain.Compliance;

namespace AtlasApplication.Abstractions;

public interface IComplianceAssessmentRepository
{
    Task AddAsync(ComplianceAssessment assessment, CancellationToken cancellationToken = default);

    Task<ComplianceAssessment?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<ComplianceAssessment?> GetByCustomerApplicationIdAsync(Guid customerApplicationId, CancellationToken cancellationToken = default);
}
