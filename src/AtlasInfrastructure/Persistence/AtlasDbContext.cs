using AtlasApplication.Abstractions;
using AtlasDomain;
using AtlasDomain.Compliance;
using AtlasInfrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace AtlasInfrastructure.Persistence;

public sealed class AtlasDbContext(DbContextOptions<AtlasDbContext> options): IdentityDbContext<AtlasStaffUser, IdentityRole<Guid>, Guid>(options), IUnitOfWork
{
    public DbSet<CustomerApplication> CustomerApplications => Set<CustomerApplication>();

    public DbSet<ComplianceAssessment> ComplianceAssessments => Set<ComplianceAssessment>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AtlasDbContext).Assembly);
    }
}
