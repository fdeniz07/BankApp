using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Persistence.Migrations
{
    /// <inheritdoc />
    public partial class SplitTitleGenderAndStaffPosition : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Gender",
                table: "Users",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);

            // Legacy PersonTitle values (Mr/Ms/Mrs/PreferNotToSay) → Gender; keep only academic titles.
            migrationBuilder.Sql("""
                UPDATE Users SET Gender = N'Male' WHERE Title = N'Mr' AND Gender IS NULL;
                UPDATE Users SET Gender = N'Female' WHERE Title IN (N'Ms', N'Mrs') AND Gender IS NULL;
                UPDATE Users SET Gender = N'PreferNotToSay' WHERE Title = N'PreferNotToSay' AND Gender IS NULL;
                UPDATE Users SET Title = NULL WHERE Title IS NOT NULL AND Title NOT IN (N'Dr', N'Prof');
                UPDATE Users SET Position = NULL WHERE Position IS NOT NULL
                  AND Position NOT IN (
                    N'CreditAnalyst', N'RelationshipManager', N'BranchOfficer',
                    N'RiskOfficer', N'OperationsSpecialist', N'BranchManager');
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Gender",
                table: "Users");
        }
    }
}
