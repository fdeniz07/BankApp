using Domain.Entities;
using Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Persistence.EntityConfigurations;

public class CreditApplicationConfiguration : IEntityTypeConfiguration<CreditApplication>
{
    public void Configure(EntityTypeBuilder<CreditApplication> builder)
    {
        builder.ToTable("CreditApplications").HasKey(c => c.Id);

        builder.Property(c => c.Id).HasColumnName("Id").IsRequired();
        builder.Property(c => c.ApplicationNumber).HasColumnName("ApplicationNumber").HasMaxLength(50).IsRequired();
        builder.Property(c => c.CustomerId).HasColumnName("CustomerId").IsRequired();
        builder.Property(c => c.CreditSubTypeId).HasColumnName("CreditSubTypeId").IsRequired();
        builder.Property(c => c.RequestedAmount).HasColumnName("RequestedAmount").HasPrecision(18, 2).IsRequired();
        builder.Property(c => c.RequestedTermMonths).HasColumnName("RequestedTermMonths").IsRequired();
        builder.Property(c => c.MonthlyIncome).HasColumnName("MonthlyIncome").HasPrecision(18, 2);
        builder.Property(c => c.ExistingDebtAmount).HasColumnName("ExistingDebtAmount").HasPrecision(18, 2);
        builder.Property(c => c.Purpose).HasColumnName("Purpose").HasMaxLength(500);
        builder.Property(c => c.Status)
            .HasColumnName("Status")
            .HasMaxLength(50)
            .HasConversion(
                status => status.Name,
                name => CreditApplicationStatus.FromName(name, ignoreCase: true))
            .IsRequired();
        builder.Property(c => c.ApplicationDate).HasColumnName("ApplicationDate").IsRequired();
        builder.Property(c => c.DecisionDate).HasColumnName("DecisionDate");
        builder.Property(c => c.ApprovedAmount).HasColumnName("ApprovedAmount").HasPrecision(18, 2);
        builder.Property(c => c.ApprovedTermMonths).HasColumnName("ApprovedTermMonths");
        builder.Property(c => c.ApprovedInterestRate).HasColumnName("ApprovedInterestRate").HasPrecision(9, 4);
        builder.Property(c => c.RejectionReason).HasColumnName("RejectionReason").HasMaxLength(500);
        builder.Property(c => c.Notes).HasColumnName("Notes").HasMaxLength(1000);
        builder.Property(c => c.DecisionBy).HasColumnName("DecisionBy").HasMaxLength(100);
        builder.Property(c => c.IsDeleted).HasColumnName("IsDeleted").IsRequired();
        builder.Property(c => c.CreatedDate).HasColumnName("CreatedDate").IsRequired();
        builder.Property(c => c.UpdatedDate).HasColumnName("UpdatedDate");
        builder.Property(c => c.DeletedDate).HasColumnName("DeletedDate");

        builder.HasIndex(c => c.ApplicationNumber).IsUnique();
        builder.HasIndex(c => c.CustomerId);
        builder.HasIndex(c => c.CreditSubTypeId);
        builder.HasIndex(c => c.Status);

        builder.HasOne(c => c.Customer)
            .WithMany(c => c.CreditApplications)
            .HasForeignKey(c => c.CustomerId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
