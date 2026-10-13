using Domain.Entities;
using Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Persistence.EntityConfigurations;

public class CreditTypeConfiguration : IEntityTypeConfiguration<CreditType>
{
    public void Configure(EntityTypeBuilder<CreditType> builder)
    {
        builder.ToTable("CreditTypes").HasKey(c => c.Id);

        builder.Property(c => c.Id).HasColumnName("Id").IsRequired();
        builder.Property(c => c.Name).HasColumnName("Name").HasMaxLength(100).IsRequired();
        builder.Property(c => c.Code).HasColumnName("Code").HasMaxLength(50).IsRequired();
        builder.Property(c => c.Category)
            .HasColumnName("Category")
            .HasMaxLength(50)
            .HasConversion(
                category => category.Name,
                name => CreditCategory.FromName(name, ignoreCase: true))
            .IsRequired();
        builder.Property(c => c.Description).HasColumnName("Description").HasMaxLength(500);
        builder.Property(c => c.CurrencyCode).HasColumnName("CurrencyCode").HasMaxLength(3).IsRequired();
        builder.Property(c => c.RiskLevel)
            .HasColumnName("RiskLevel")
            .HasMaxLength(50)
            .HasConversion(
                riskLevel => riskLevel.Name,
                name => CreditRiskLevel.FromName(name, ignoreCase: true))
            .IsRequired();
        builder.Property(c => c.RequiresCollateral).HasColumnName("RequiresCollateral").IsRequired();
        builder.Property(c => c.RequiresGuarantor).HasColumnName("RequiresGuarantor").IsRequired();
        builder.Property(c => c.IsActive).HasColumnName("IsActive").IsRequired();
        builder.Property(c => c.IsDeleted).HasColumnName("IsDeleted").IsRequired();
        builder.Property(c => c.CreatedDate).HasColumnName("CreatedDate").IsRequired();
        builder.Property(c => c.UpdatedDate).HasColumnName("UpdatedDate");
        builder.Property(c => c.DeletedDate).HasColumnName("DeletedDate");

        builder.HasIndex(c => c.Code).IsUnique();

        builder.HasMany(c => c.CreditSubTypes)
            .WithOne(c => c.CreditType)
            .HasForeignKey(c => c.CreditTypeId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
