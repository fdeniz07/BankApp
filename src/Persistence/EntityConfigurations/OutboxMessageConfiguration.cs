using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Persistence.EntityConfigurations;

public class OutboxMessageConfiguration : IEntityTypeConfiguration<OutboxMessage>
{
    public void Configure(EntityTypeBuilder<OutboxMessage> builder)
    {
        builder.ToTable("OutboxMessages").HasKey(o => o.Id);

        builder.Property(o => o.Id).HasColumnName("Id").IsRequired();
        builder.Property(o => o.Type).HasColumnName("Type").HasMaxLength(256).IsRequired();
        builder.Property(o => o.Payload).HasColumnName("Payload").IsRequired();
        builder.Property(o => o.OccurredOnUtc).HasColumnName("OccurredOnUtc").IsRequired();
        builder.Property(o => o.ProcessedOnUtc).HasColumnName("ProcessedOnUtc");
        builder.Property(o => o.Error).HasColumnName("Error").HasMaxLength(2000);
        builder.Property(o => o.IsDeleted).HasColumnName("IsDeleted").IsRequired();
        builder.Property(o => o.CreatedDate).HasColumnName("CreatedDate").IsRequired();
        builder.Property(o => o.UpdatedDate).HasColumnName("UpdatedDate");
        builder.Property(o => o.DeletedDate).HasColumnName("DeletedDate");

        builder.HasIndex(o => o.ProcessedOnUtc);
        builder.HasIndex(o => o.OccurredOnUtc);
    }
}
