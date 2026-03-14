using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LegalConnect.API.Migrations
{
    /// <inheritdoc />
    public partial class AddActivityTitleCategoryEventDateDocuments : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Category",
                table: "CaseActivities",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "EventDate",
                table: "CaseActivities",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Title",
                table: "CaseActivities",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "CaseActivityDocuments",
                columns: table => new
                {
                    CaseActivityId = table.Column<int>(type: "int", nullable: false),
                    CaseDocumentId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CaseActivityDocuments", x => new { x.CaseActivityId, x.CaseDocumentId });
                    table.ForeignKey(
                        name: "FK_CaseActivityDocuments_CaseActivities_CaseActivityId",
                        column: x => x.CaseActivityId,
                        principalTable: "CaseActivities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CaseActivityDocuments_CaseDocuments_CaseDocumentId",
                        column: x => x.CaseDocumentId,
                        principalTable: "CaseDocuments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CaseActivityDocuments_CaseDocumentId",
                table: "CaseActivityDocuments",
                column: "CaseDocumentId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CaseActivityDocuments");

            migrationBuilder.DropColumn(
                name: "Category",
                table: "CaseActivities");

            migrationBuilder.DropColumn(
                name: "EventDate",
                table: "CaseActivities");

            migrationBuilder.DropColumn(
                name: "Title",
                table: "CaseActivities");
        }
    }
}
