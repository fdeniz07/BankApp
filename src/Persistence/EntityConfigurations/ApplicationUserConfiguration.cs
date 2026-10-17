using Domain.Entities;
using Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Persistence.EntityConfigurations;

public class ApplicationUserConfiguration : IEntityTypeConfiguration<ApplicationUser>
{
    public void Configure(EntityTypeBuilder<ApplicationUser> builder)
    {
        builder.Property(u => u.CustomerId).HasColumnName("CustomerId");
        builder.HasIndex(u => u.CustomerId);

        builder.Property(u => u.FirstName).HasColumnName("FirstName").HasMaxLength(50);
        builder.Property(u => u.LastName).HasColumnName("LastName").HasMaxLength(50);
        builder.Property(u => u.Title)
            .HasColumnName("Title")
            .HasMaxLength(50)
            .HasConversion(
                title => title == null ? null : title.Name,
                name => string.IsNullOrWhiteSpace(name) || !PersonTitle.IsValidName(name)
                    ? null
                    : PersonTitle.FromName(name, ignoreCase: true));
        builder.Property(u => u.Gender)
            .HasColumnName("Gender")
            .HasMaxLength(20)
            .HasConversion(
                gender => gender == null ? null : gender.Name,
                name => string.IsNullOrWhiteSpace(name) || !Gender.IsValidName(name)
                    ? null
                    : Gender.FromName(name, ignoreCase: true));
        builder.Property(u => u.DateOfBirth).HasColumnName("DateOfBirth");
        builder.Property(u => u.Position)
            .HasColumnName("Position")
            .HasMaxLength(100)
            .HasConversion(
                position => position == null ? null : position.Name,
                name => string.IsNullOrWhiteSpace(name) || !StaffPosition.IsValidName(name)
                    ? null
                    : StaffPosition.FromName(name, ignoreCase: true));
        builder.Property(u => u.ProfileImagePath).HasColumnName("ProfileImagePath").HasMaxLength(500);

        builder.HasOne(u => u.Customer)
            .WithMany()
            .HasForeignKey(u => u.CustomerId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
