using AtlasDomain;
using AtlasDomain.Compliance;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AtlasInfrastructure.Persistence.Configurations;

public sealed class ComplianceAssessmentConfiguration: IEntityTypeConfiguration<ComplianceAssessment>
{
    public void Configure(EntityTypeBuilder<ComplianceAssessment> builder)
    {
        builder.ToTable("ComplianceAssessments");

        builder.HasKey(x => x.Id);

        builder.HasOne<CustomerApplication>()
            .WithMany()
            .HasForeignKey(x => x.CustomerApplicationId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Property(x => x.Id)
            .ValueGeneratedNever();

        builder.Property(x => x.CustomerApplicationId)
            .IsRequired();

        builder.Property(x => x.Status)
            .HasConversion<int>()
            .IsRequired();

        builder.Property(x => x.CreatedAtUtc)
            .IsRequired();

        builder.Ignore(x => x.Result);

        builder.OwnsMany(
            x => x.Evaluations,
            evaluation =>
            {
                evaluation.ToTable("RequirementEvaluations");

                evaluation.WithOwner()
                    .HasForeignKey("ComplianceAssessmentId");

                evaluation.HasKey(x => x.Id);

                evaluation.Property(x => x.Id)
                    .ValueGeneratedNever();

                evaluation.Property(x => x.RequirementCode)
                    .IsRequired()
                    .HasMaxLength(100);

                evaluation.Property(x => x.RequirementName)
                    .IsRequired()
                    .HasMaxLength(200);

                evaluation.Property(x => x.Outcome)
                    .HasConversion<int>()
                    .IsRequired();

                evaluation.Property(x => x.Reason)
                    .HasMaxLength(2000);

                evaluation.Property(x => x.EvaluatedAtUtc);

                evaluation.Property(x => x.EvaluatedByStaffId)
                    .HasColumnName("EvaluatedByStaffId");

                evaluation.Property(x => x.ResolutionOutcome)
                    .HasConversion<int?>();

                evaluation.Property(x => x.ResolutionReason)
                    .HasMaxLength(2000);

                evaluation.Property(x => x.ResolvedAtUtc);

                evaluation.Property(x => x.ResolvedByStaffId);
            });

        builder.Navigation(x => x.Evaluations)
            .UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
