using Core.Security.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Persistence.EntityConfigurations;

public class UserOperationClaimConfiguration : IEntityTypeConfiguration<UserOperationClaim>
{
    public void Configure(EntityTypeBuilder<UserOperationClaim> builder)
    {
        builder.ToTable("UserOperationClaims").HasKey(u => u.Id);

        builder.Property(u => u.Id).HasColumnName("Id").IsRequired();
        builder.Property(u => u.UserId).HasColumnName("UserId").IsRequired();
        builder.Property(u => u.OperationClaimId).HasColumnName("OperationClaimId").IsRequired();
        builder.Property(u => u.IsDeleted).HasColumnName("IsDeleted").IsRequired();
        builder.Property(u => u.CreatedDate).HasColumnName("CreatedDate").IsRequired();
        builder.Property(u => u.UpdatedDate).HasColumnName("UpdatedDate");
        builder.Property(u => u.DeletedDate).HasColumnName("DeletedDate");

        builder.HasIndex(u => new { u.UserId, u.OperationClaimId }).IsUnique();

        builder.HasOne(u => u.User)
            .WithMany(u => u.UserOperationClaims)
            .HasForeignKey(u => u.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(u => u.OperationClaim)
            .WithMany(o => o.UserOperationClaims)
            .HasForeignKey(u => u.OperationClaimId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
