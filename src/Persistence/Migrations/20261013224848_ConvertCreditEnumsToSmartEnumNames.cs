using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Persistence.Migrations
{
    /// <inheritdoc />
    public partial class ConvertCreditEnumsToSmartEnumNames : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_CreditApplications_Status",
                table: "CreditApplications");

            migrationBuilder.AddColumn<string>(
                name: "CategoryName",
                table: "CreditTypes",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RiskLevelName",
                table: "CreditTypes",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "StatusName",
                table: "CreditApplications",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            // Eski C# enum değerleri 0 tabanlıydı (Individual=0, Corporate=1, ...)
            migrationBuilder.Sql("""
                UPDATE CreditTypes SET CategoryName = CASE Category
                    WHEN 0 THEN N'Individual'
                    WHEN 1 THEN N'Corporate'
                    ELSE N'Individual'
                END;

                UPDATE CreditTypes SET RiskLevelName = CASE RiskLevel
                    WHEN 0 THEN N'Low'
                    WHEN 1 THEN N'Medium'
                    WHEN 2 THEN N'High'
                    ELSE N'Medium'
                END;

                UPDATE CreditApplications SET StatusName = CASE Status
                    WHEN 0 THEN N'Pending'
                    WHEN 1 THEN N'Approved'
                    WHEN 2 THEN N'Rejected'
                    WHEN 3 THEN N'Cancelled'
                    ELSE N'Pending'
                END;
                """);

            migrationBuilder.DropColumn(name: "Category", table: "CreditTypes");
            migrationBuilder.DropColumn(name: "RiskLevel", table: "CreditTypes");
            migrationBuilder.DropColumn(name: "Status", table: "CreditApplications");

            migrationBuilder.RenameColumn(
                name: "CategoryName",
                table: "CreditTypes",
                newName: "Category");

            migrationBuilder.RenameColumn(
                name: "RiskLevelName",
                table: "CreditTypes",
                newName: "RiskLevel");

            migrationBuilder.RenameColumn(
                name: "StatusName",
                table: "CreditApplications",
                newName: "Status");

            migrationBuilder.AlterColumn<string>(
                name: "Category",
                table: "CreditTypes",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "RiskLevel",
                table: "CreditTypes",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Status",
                table: "CreditApplications",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50,
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_CreditApplications_Status",
                table: "CreditApplications",
                column: "Status");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_CreditApplications_Status",
                table: "CreditApplications");

            migrationBuilder.AddColumn<int>(
                name: "CategoryInt",
                table: "CreditTypes",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "RiskLevelInt",
                table: "CreditTypes",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "StatusInt",
                table: "CreditApplications",
                type: "int",
                nullable: true);

            migrationBuilder.Sql("""
                UPDATE CreditTypes SET CategoryInt = CASE Category
                    WHEN N'Individual' THEN 0
                    WHEN N'Corporate' THEN 1
                    ELSE 0
                END;

                UPDATE CreditTypes SET RiskLevelInt = CASE RiskLevel
                    WHEN N'Low' THEN 0
                    WHEN N'Medium' THEN 1
                    WHEN N'High' THEN 2
                    ELSE 1
                END;

                UPDATE CreditApplications SET StatusInt = CASE Status
                    WHEN N'Pending' THEN 0
                    WHEN N'Approved' THEN 1
                    WHEN N'Rejected' THEN 2
                    WHEN N'Cancelled' THEN 3
                    ELSE 0
                END;
                """);

            migrationBuilder.DropColumn(name: "Category", table: "CreditTypes");
            migrationBuilder.DropColumn(name: "RiskLevel", table: "CreditTypes");
            migrationBuilder.DropColumn(name: "Status", table: "CreditApplications");

            migrationBuilder.RenameColumn(
                name: "CategoryInt",
                table: "CreditTypes",
                newName: "Category");

            migrationBuilder.RenameColumn(
                name: "RiskLevelInt",
                table: "CreditTypes",
                newName: "RiskLevel");

            migrationBuilder.RenameColumn(
                name: "StatusInt",
                table: "CreditApplications",
                newName: "Status");

            migrationBuilder.AlterColumn<int>(
                name: "Category",
                table: "CreditTypes",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "RiskLevel",
                table: "CreditTypes",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "Status",
                table: "CreditApplications",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_CreditApplications_Status",
                table: "CreditApplications",
                column: "Status");
        }
    }
}
