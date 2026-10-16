using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddCreditTypeLocale : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Locale",
                table: "CreditTypes",
                type: "nvarchar(5)",
                maxLength: 5,
                nullable: false,
                defaultValue: "tr");

            migrationBuilder.CreateIndex(
                name: "IX_CreditTypes_Locale",
                table: "CreditTypes",
                column: "Locale");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_CreditTypes_Locale",
                table: "CreditTypes");

            migrationBuilder.DropColumn(
                name: "Locale",
                table: "CreditTypes");
        }
    }
}
