using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LegalConnect.API.Migrations
{
    /// <inheritdoc />
    public partial class AddMultiLawyerAndRequestSystem : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "LawyerProfileId",
                table: "Cases",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<bool>(
                name: "SharedWithAllLawyers",
                table: "CaseDocuments",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "CaseDocumentLawyerShares",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CaseDocumentId = table.Column<int>(type: "int", nullable: false),
                    LawyerProfileId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CaseDocumentLawyerShares", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CaseDocumentLawyerShares_CaseDocuments_CaseDocumentId",
                        column: x => x.CaseDocumentId,
                        principalTable: "CaseDocuments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CaseDocumentLawyerShares_LawyerProfiles_LawyerProfileId",
                        column: x => x.LawyerProfileId,
                        principalTable: "LawyerProfiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CaseLawyers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CaseId = table.Column<int>(type: "int", nullable: false),
                    LawyerProfileId = table.Column<int>(type: "int", nullable: false),
                    AddedByRole = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    AddedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CaseLawyers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CaseLawyers_Cases_CaseId",
                        column: x => x.CaseId,
                        principalTable: "Cases",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CaseLawyers_LawyerProfiles_LawyerProfileId",
                        column: x => x.LawyerProfileId,
                        principalTable: "LawyerProfiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ClientLawyerRequests",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ClientProfileId = table.Column<int>(type: "int", nullable: false),
                    LawyerProfileId = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Message = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    LawyerNote = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClientLawyerRequests", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ClientLawyerRequests_ClientProfiles_ClientProfileId",
                        column: x => x.ClientProfileId,
                        principalTable: "ClientProfiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ClientLawyerRequests_LawyerProfiles_LawyerProfileId",
                        column: x => x.LawyerProfileId,
                        principalTable: "LawyerProfiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CaseDocumentLawyerShares_CaseDocumentId_LawyerProfileId",
                table: "CaseDocumentLawyerShares",
                columns: new[] { "CaseDocumentId", "LawyerProfileId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CaseDocumentLawyerShares_LawyerProfileId",
                table: "CaseDocumentLawyerShares",
                column: "LawyerProfileId");

            migrationBuilder.CreateIndex(
                name: "IX_CaseLawyers_CaseId_LawyerProfileId",
                table: "CaseLawyers",
                columns: new[] { "CaseId", "LawyerProfileId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CaseLawyers_LawyerProfileId",
                table: "CaseLawyers",
                column: "LawyerProfileId");

            migrationBuilder.CreateIndex(
                name: "IX_ClientLawyerRequests_ClientProfileId_LawyerProfileId",
                table: "ClientLawyerRequests",
                columns: new[] { "ClientProfileId", "LawyerProfileId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ClientLawyerRequests_LawyerProfileId",
                table: "ClientLawyerRequests",
                column: "LawyerProfileId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CaseDocumentLawyerShares");

            migrationBuilder.DropTable(
                name: "CaseLawyers");

            migrationBuilder.DropTable(
                name: "ClientLawyerRequests");

            migrationBuilder.DropColumn(
                name: "SharedWithAllLawyers",
                table: "CaseDocuments");

            migrationBuilder.AlterColumn<int>(
                name: "LawyerProfileId",
                table: "Cases",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);
        }
    }
}
