using Domain.Entities;
using Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Persistence.EntityConfigurations;

public class CreditApplicationSagaConfiguration : IEntityTypeConfiguration<CreditApplicationSaga>
{
    public void Configure(EntityTypeBuilder<CreditApplicationSaga> builder)
    {
        builder.ToTable("CreditApplicationSagas").HasKey(s => s.Id);

        builder.Property(s => s.Id).HasColumnName("Id").IsRequired();
        builder.Property(s => s.CreditApplicationId).HasColumnName("CreditApplicationId").IsRequired();
        builder.Property(s => s.ApplicationNumber).HasColumnName("ApplicationNumber").HasMaxLength(50).IsRequired();
        builder.Property(s => s.Status)
            .HasColumnName("Status")
            .HasMaxLength(50)
            .HasConversion(
                status => status.Name,
                name => CreditApplicationSagaStatus.FromName(name, ignoreCase: true))
            .IsRequired();
        builder.Property(s => s.CurrentStep)
            .HasColumnName("CurrentStep")
            .HasMaxLength(50)
            .HasConversion(
                step => step.Name,
                name => CreditApplicationSagaStep.FromName(name, ignoreCase: true))
            .IsRequired();
        builder.Property(s => s.ContractReference).HasColumnName("ContractReference").HasMaxLength(100);
        builder.Property(s => s.FailureReason).HasColumnName("FailureReason").HasMaxLength(1000);
        builder.Property(s => s.StartedOnUtc).HasColumnName("StartedOnUtc").IsRequired();
        builder.Property(s => s.CompletedOnUtc).HasColumnName("CompletedOnUtc");
        builder.Property(s => s.IsDeleted).HasColumnName("IsDeleted").IsRequired();
        builder.Property(s => s.CreatedDate).HasColumnName("CreatedDate").IsRequired();
        builder.Property(s => s.UpdatedDate).HasColumnName("UpdatedDate");
        builder.Property(s => s.DeletedDate).HasColumnName("DeletedDate");

        builder.HasIndex(s => s.CreditApplicationId).IsUnique();
        builder.HasIndex(s => s.Status);
    }
}
