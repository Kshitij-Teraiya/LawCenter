using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LegalConnect.API.Migrations
{
    /// <inheritdoc />
    public partial class RefactorDealWorkflow : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_HireRequests_Cases_CaseId",
                table: "HireRequests");

            migrationBuilder.DropForeignKey(
                name: "FK_Invoices_HireRequests_HireRequestId",
                table: "Invoices");

            migrationBuilder.DropForeignKey(
                name: "FK_Proposals_HireRequests_HireRequestId",
                table: "Proposals");

            migrationBuilder.DropIndex(
                name: "IX_HireRequests_CaseId",
                table: "HireRequests");

            migrationBuilder.DropColumn(
                name: "CaseId",
                table: "HireRequests");

            migrationBuilder.RenameColumn(
                name: "HireRequestId",
                table: "Proposals",
                newName: "DealId");

            migrationBuilder.RenameIndex(
                name: "IX_Proposals_HireRequestId",
                table: "Proposals",
                newName: "IX_Proposals_DealId");

            migrationBuilder.RenameColumn(
                name: "HireRequestId",
                table: "Invoices",
                newName: "DealId");

            migrationBuilder.RenameIndex(
                name: "IX_Invoices_HireRequestId",
                table: "Invoices",
                newName: "IX_Invoices_DealId");

            migrationBuilder.AddColumn<string>(
                name: "CaseType",
                table: "HireRequests",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Court",
                table: "HireRequests",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "HireRequests",
                type: "nvarchar(3000)",
                maxLength: 3000,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "DealId",
                table: "Cases",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Deals",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    HireRequestId = table.Column<int>(type: "int", nullable: false),
                    ClientProfileId = table.Column<int>(type: "int", nullable: false),
                    LawyerProfileId = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Deals", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Deals_ClientProfiles_ClientProfileId",
                        column: x => x.ClientProfileId,
                        principalTable: "ClientProfiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Deals_HireRequests_HireRequestId",
                        column: x => x.HireRequestId,
                        principalTable: "HireRequests",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Deals_LawyerProfiles_LawyerProfileId",
                        column: x => x.LawyerProfileId,
                        principalTable: "LawyerProfiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Cases_DealId",
                table: "Cases",
                column: "DealId");

            migrationBuilder.CreateIndex(
                name: "IX_Deals_ClientProfileId",
                table: "Deals",
                column: "ClientProfileId");

            migrationBuilder.CreateIndex(
                name: "IX_Deals_HireRequestId",
                table: "Deals",
                column: "HireRequestId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Deals_LawyerProfileId",
                table: "Deals",
                column: "LawyerProfileId");

            // Clean up orphaned data from old HireRequestId values (now renamed to DealId)
            // These old values reference HireRequests, not Deals, so they'd violate the new FK
            migrationBuilder.Sql("DELETE FROM [Invoices] WHERE [DealId] NOT IN (SELECT [Id] FROM [Deals])");
            migrationBuilder.Sql("DELETE FROM [Proposals] WHERE [DealId] NOT IN (SELECT [Id] FROM [Deals])");

            migrationBuilder.AddForeignKey(
                name: "FK_Cases_Deals_DealId",
                table: "Cases",
                column: "DealId",
                principalTable: "Deals",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Invoices_Deals_DealId",
                table: "Invoices",
                column: "DealId",
                principalTable: "Deals",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Proposals_Deals_DealId",
                table: "Proposals",
                column: "DealId",
                principalTable: "Deals",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Cases_Deals_DealId",
                table: "Cases");

            migrationBuilder.DropForeignKey(
                name: "FK_Invoices_Deals_DealId",
                table: "Invoices");

            migrationBuilder.DropForeignKey(
                name: "FK_Proposals_Deals_DealId",
                table: "Proposals");

            migrationBuilder.DropTable(
                name: "Deals");

            migrationBuilder.DropIndex(
                name: "IX_Cases_DealId",
                table: "Cases");

            migrationBuilder.DropColumn(
                name: "CaseType",
                table: "HireRequests");

            migrationBuilder.DropColumn(
                name: "Court",
                table: "HireRequests");

            migrationBuilder.DropColumn(
                name: "Description",
                table: "HireRequests");

            migrationBuilder.DropColumn(
                name: "DealId",
                table: "Cases");

            migrationBuilder.RenameColumn(
                name: "DealId",
                table: "Proposals",
                newName: "HireRequestId");

            migrationBuilder.RenameIndex(
                name: "IX_Proposals_DealId",
                table: "Proposals",
                newName: "IX_Proposals_HireRequestId");

            migrationBuilder.RenameColumn(
                name: "DealId",
                table: "Invoices",
                newName: "HireRequestId");

            migrationBuilder.RenameIndex(
                name: "IX_Invoices_DealId",
                table: "Invoices",
                newName: "IX_Invoices_HireRequestId");

            migrationBuilder.AddColumn<int>(
                name: "CaseId",
                table: "HireRequests",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_HireRequests_CaseId",
                table: "HireRequests",
                column: "CaseId");

            migrationBuilder.AddForeignKey(
                name: "FK_HireRequests_Cases_CaseId",
                table: "HireRequests",
                column: "CaseId",
                principalTable: "Cases",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Invoices_HireRequests_HireRequestId",
                table: "Invoices",
                column: "HireRequestId",
                principalTable: "HireRequests",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Proposals_HireRequests_HireRequestId",
                table: "Proposals",
                column: "HireRequestId",
                principalTable: "HireRequests",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
