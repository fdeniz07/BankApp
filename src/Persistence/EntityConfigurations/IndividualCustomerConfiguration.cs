using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Persistence.EntityConfigurations;

public class IndividualCustomerConfiguration : IEntityTypeConfiguration<IndividualCustomer>
{
    public void Configure(EntityTypeBuilder<IndividualCustomer> builder)
    {
        builder.ToTable("IndividualCustomers");

        builder.Property(c => c.FirstName).HasColumnName("FirstName").HasMaxLength(50).IsRequired();
        builder.Property(c => c.LastName).HasColumnName("LastName").HasMaxLength(50).IsRequired();
        builder.Property(c => c.NationalId).HasColumnName("NationalId").HasMaxLength(11).IsRequired();
        builder.Property(c => c.DateOfBirth).HasColumnName("DateOfBirth");
        builder.Property(c => c.Gender).HasColumnName("Gender").HasMaxLength(20);
        builder.Property(c => c.MotherName).HasColumnName("MotherName").HasMaxLength(50);
        builder.Property(c => c.FatherName).HasColumnName("FatherName").HasMaxLength(50);

        builder.HasIndex(c => c.NationalId).IsUnique();
    }
}
