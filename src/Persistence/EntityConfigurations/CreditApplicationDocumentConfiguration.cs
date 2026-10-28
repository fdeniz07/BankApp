using Domain.Entities;
using Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Persistence.EntityConfigurations;

public class CreditApplicationDocumentConfiguration : IEntityTypeConfiguration<CreditApplicationDocument>
{
    public void Configure(EntityTypeBuilder<CreditApplicationDocument> builder)
    {
        builder.ToTable("CreditApplicationDocuments").HasKey(d => d.Id);

        builder.Property(d => d.Id).HasColumnName("Id").IsRequired();
        builder.Property(d => d.CreditApplicationId).HasColumnName("CreditApplicationId").IsRequired();
        builder.Property(d => d.DocumentType)
            .HasColumnName("DocumentType")
            .HasMaxLength(50)
            .HasConversion(
                type => type.Name,
                name => CreditDocumentType.FromName(name, ignoreCase: true))
            .IsRequired();
        builder.Property(d => d.OriginalFileName).HasColumnName("OriginalFileName").HasMaxLength(260).IsRequired();
        builder.Property(d => d.ContentType).HasColumnName("ContentType").HasMaxLength(100).IsRequired();
        builder.Property(d => d.SizeBytes).HasColumnName("SizeBytes").IsRequired();
        builder.Property(d => d.StoragePath).HasColumnName("StoragePath").HasMaxLength(500).IsRequired();
        builder.Property(d => d.UploadedByUserId).HasColumnName("UploadedByUserId").IsRequired();
        builder.Property(d => d.UploadedAt).HasColumnName("UploadedAt").IsRequired();
        builder.Property(d => d.IsDeleted).HasColumnName("IsDeleted").IsRequired();
        builder.Property(d => d.CreatedDate).HasColumnName("CreatedDate").IsRequired();
        builder.Property(d => d.UpdatedDate).HasColumnName("UpdatedDate");
        builder.Property(d => d.DeletedDate).HasColumnName("DeletedDate");

        builder.HasIndex(d => d.CreditApplicationId);
        builder.HasIndex(d => new { d.CreditApplicationId, d.DocumentType });

        builder.HasOne(d => d.CreditApplication)
            .WithMany()
            .HasForeignKey(d => d.CreditApplicationId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
