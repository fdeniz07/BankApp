using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Persistence.EntityConfigurations;

public class DecisionLetterTemplateConfiguration : IEntityTypeConfiguration<DecisionLetterTemplate>
{
    public void Configure(EntityTypeBuilder<DecisionLetterTemplate> builder)
    {
        builder.ToTable("DecisionLetterTemplates").HasKey(t => t.Id);

        builder.Property(t => t.Id).HasColumnName("Id").IsRequired();
        builder.Property(t => t.UserId).HasColumnName("UserId").IsRequired();
        builder.Property(t => t.Kind).HasColumnName("Kind").HasMaxLength(20).IsRequired();
        builder.Property(t => t.Locale).HasColumnName("Locale").HasMaxLength(5).IsRequired();
        builder.Property(t => t.Name).HasColumnName("Name").HasMaxLength(120).IsRequired();
        builder.Property(t => t.Body).HasColumnName("Body").HasMaxLength(4000).IsRequired();
        builder.Property(t => t.IsDeleted).HasColumnName("IsDeleted").IsRequired();
        builder.Property(t => t.CreatedDate).HasColumnName("CreatedDate").IsRequired();
        builder.Property(t => t.UpdatedDate).HasColumnName("UpdatedDate");
        builder.Property(t => t.DeletedDate).HasColumnName("DeletedDate");

        builder.HasIndex(t => new { t.UserId, t.Kind, t.Locale });
    }
}
