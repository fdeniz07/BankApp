using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Persistence.EntityConfigurations;

public class BranchConfiguration : IEntityTypeConfiguration<Branch>
{
    public void Configure(EntityTypeBuilder<Branch> builder)
    {
        builder.ToTable("Branches").HasKey(b => b.Id);

        builder.Property(b => b.Id).HasColumnName("Id").IsRequired();
        builder.Property(b => b.Code).HasColumnName("Code").HasMaxLength(20).IsRequired();
        builder.Property(b => b.Name).HasColumnName("Name").HasMaxLength(150).IsRequired();
        builder.Property(b => b.City).HasColumnName("City").HasMaxLength(100);
        builder.Property(b => b.IsActive).HasColumnName("IsActive").IsRequired();
        builder.Property(b => b.IsDeleted).HasColumnName("IsDeleted").IsRequired();
        builder.Property(b => b.CreatedDate).HasColumnName("CreatedDate").IsRequired();
        builder.Property(b => b.UpdatedDate).HasColumnName("UpdatedDate");
        builder.Property(b => b.DeletedDate).HasColumnName("DeletedDate");

        builder.HasIndex(b => b.Code).IsUnique();
    }
}
