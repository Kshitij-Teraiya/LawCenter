using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LegalConnect.API.Migrations
{
    /// <inheritdoc />
    public partial class AddIsAvailableForDealToCaseDocument : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsAvailableForDeal",
                table: "CaseDocuments",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsAvailableForDeal",
                table: "CaseDocuments");
        }
    }
}
