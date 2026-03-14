using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LegalConnect.API.Migrations
{
    /// <inheritdoc />
    public partial class AddLinkedCaseIdToHireRequest : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "LinkedCaseId",
                table: "HireRequests",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_HireRequests_LinkedCaseId",
                table: "HireRequests",
                column: "LinkedCaseId");

            migrationBuilder.AddForeignKey(
                name: "FK_HireRequests_Cases_LinkedCaseId",
                table: "HireRequests",
                column: "LinkedCaseId",
                principalTable: "Cases",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_HireRequests_Cases_LinkedCaseId",
                table: "HireRequests");

            migrationBuilder.DropIndex(
                name: "IX_HireRequests_LinkedCaseId",
                table: "HireRequests");

            migrationBuilder.DropColumn(
                name: "LinkedCaseId",
                table: "HireRequests");
        }
    }
}
