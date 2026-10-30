using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Persistence.EntityConfigurations;

public class CustomerConfiguration : IEntityTypeConfiguration<Customer>
{
    public void Configure(EntityTypeBuilder<Customer> builder)
    {
        builder.ToTable("Customers").HasKey(c => c.Id);
        builder.UseTptMappingStrategy();

        builder.Property(c => c.Id).HasColumnName("Id").IsRequired();
        builder.Property(c => c.CustomerNumber).HasColumnName("CustomerNumber").HasMaxLength(50).IsRequired();
        builder.Property(c => c.Email).HasColumnName("Email").HasMaxLength(100).IsRequired();
        builder.Property(c => c.PhoneNumber).HasColumnName("PhoneNumber").HasMaxLength(20).IsRequired();
        builder.Property(c => c.Address).HasColumnName("Address").HasMaxLength(500).IsRequired();
        builder.Property(c => c.IsActive).HasColumnName("IsActive").IsRequired();
        builder.Property(c => c.BranchId).HasColumnName("BranchId");
        builder.Property(c => c.IsDeleted).HasColumnName("IsDeleted").IsRequired();
        builder.Property(c => c.CreatedDate).HasColumnName("CreatedDate").IsRequired();
        builder.Property(c => c.UpdatedDate).HasColumnName("UpdatedDate");
        builder.Property(c => c.DeletedDate).HasColumnName("DeletedDate");

        builder.HasIndex(c => c.CustomerNumber).IsUnique();
        builder.HasIndex(c => c.Email).IsUnique();
        builder.HasIndex(c => c.BranchId);

        builder.HasOne(c => c.Branch)
            .WithMany()
            .HasForeignKey(c => c.BranchId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
