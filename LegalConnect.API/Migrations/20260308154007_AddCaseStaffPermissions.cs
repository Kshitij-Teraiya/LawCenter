using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LegalConnect.API.Migrations
{
    /// <inheritdoc />
    public partial class AddCaseStaffPermissions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "CanAddActivity",
                table: "CaseStaffs",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "CanUploadDocument",
                table: "CaseStaffs",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CanAddActivity",
                table: "CaseStaffs");

            migrationBuilder.DropColumn(
                name: "CanUploadDocument",
                table: "CaseStaffs");
        }
    }
}
