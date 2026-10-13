using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Persistence.EntityConfigurations;

public class CorporateCustomerConfiguration : IEntityTypeConfiguration<CorporateCustomer>
{
    public void Configure(EntityTypeBuilder<CorporateCustomer> builder)
    {
        builder.ToTable("CorporateCustomers");

        builder.Property(c => c.CompanyName).HasColumnName("CompanyName").HasMaxLength(200).IsRequired();
        builder.Property(c => c.TaxNumber).HasColumnName("TaxNumber").HasMaxLength(20).IsRequired();
        builder.Property(c => c.TaxOffice).HasColumnName("TaxOffice").HasMaxLength(100).IsRequired();
        builder.Property(c => c.CompanyRegistrationNumber).HasColumnName("CompanyRegistrationNumber").HasMaxLength(50).IsRequired();
        builder.Property(c => c.CompanyEmail).HasColumnName("CompanyEmail").HasMaxLength(100);
        builder.Property(c => c.AuthorizedPersonName).HasColumnName("AuthorizedPersonName").HasMaxLength(100).IsRequired();
        builder.Property(c => c.CompanyAddress).HasColumnName("CompanyAddress").HasMaxLength(500);
        builder.Property(c => c.CompanyType).HasColumnName("CompanyType").HasMaxLength(50);
        builder.Property(c => c.CompanySector).HasColumnName("CompanySector").HasMaxLength(100);
        builder.Property(c => c.CompanyIndustry).HasColumnName("CompanyIndustry").HasMaxLength(100);
        builder.Property(c => c.CompanySubIndustry).HasColumnName("CompanySubIndustry").HasMaxLength(100);
        builder.Property(c => c.CompanyFoundationDate).HasColumnName("CompanyFoundationDate");

        builder.HasIndex(c => c.TaxNumber).IsUnique();
        builder.HasIndex(c => c.CompanyRegistrationNumber).IsUnique();
    }
}
