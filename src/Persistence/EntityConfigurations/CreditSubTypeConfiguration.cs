using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Persistence.EntityConfigurations;

public class CreditSubTypeConfiguration : IEntityTypeConfiguration<CreditSubType>
{
    public void Configure(EntityTypeBuilder<CreditSubType> builder)
    {
        builder.ToTable("CreditSubTypes").HasKey(c => c.Id);

        builder.Property(c => c.Id).HasColumnName("Id").IsRequired();
        builder.Property(c => c.CreditTypeId).HasColumnName("CreditTypeId").IsRequired();
        builder.Property(c => c.Name).HasColumnName("Name").HasMaxLength(100).IsRequired();
        builder.Property(c => c.Code).HasColumnName("Code").HasMaxLength(50).IsRequired();
        builder.Property(c => c.Description).HasColumnName("Description").HasMaxLength(500);
        builder.Property(c => c.MinAmount).HasColumnName("MinAmount").HasPrecision(18, 2).IsRequired();
        builder.Property(c => c.MaxAmount).HasColumnName("MaxAmount").HasPrecision(18, 2).IsRequired();
        builder.Property(c => c.InterestRate).HasColumnName("InterestRate").HasPrecision(9, 4).IsRequired();
        builder.Property(c => c.MinTermMonths).HasColumnName("MinTermMonths").IsRequired();
        builder.Property(c => c.MaxTermMonths).HasColumnName("MaxTermMonths").IsRequired();
        builder.Property(c => c.LateFeeRate).HasColumnName("LateFeeRate").HasPrecision(9, 4);
        builder.Property(c => c.EarlyPaymentFeeRate).HasColumnName("EarlyPaymentFeeRate").HasPrecision(9, 4);
        builder.Property(c => c.ProcessingFeeAmount).HasColumnName("ProcessingFeeAmount").HasPrecision(18, 2);
        builder.Property(c => c.ProcessingFeeRate).HasColumnName("ProcessingFeeRate").HasPrecision(9, 4);
        builder.Property(c => c.GracePeriodDays).HasColumnName("GracePeriodDays").IsRequired();
        builder.Property(c => c.RequiresCollateral).HasColumnName("RequiresCollateral").IsRequired();
        builder.Property(c => c.RequiresIncomeDocument).HasColumnName("RequiresIncomeDocument").IsRequired();
        builder.Property(c => c.IsActive).HasColumnName("IsActive").IsRequired();
        builder.Property(c => c.IsDeleted).HasColumnName("IsDeleted").IsRequired();
        builder.Property(c => c.CreatedDate).HasColumnName("CreatedDate").IsRequired();
        builder.Property(c => c.UpdatedDate).HasColumnName("UpdatedDate");
        builder.Property(c => c.DeletedDate).HasColumnName("DeletedDate");

        builder.HasIndex(c => c.Code).IsUnique();
        builder.HasIndex(c => c.CreditTypeId);

        builder.HasMany(c => c.CreditApplications)
            .WithOne(c => c.CreditSubType)
            .HasForeignKey(c => c.CreditSubTypeId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
