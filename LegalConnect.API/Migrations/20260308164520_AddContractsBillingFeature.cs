using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LegalConnect.API.Migrations
{
    /// <inheritdoc />
    public partial class AddContractsBillingFeature : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Note: CanAddActivity + CanUploadDocument were already applied manually to CaseStaffs
            migrationBuilder.CreateTable(
                name: "DuesEntries",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    LawyerProfileId = table.Column<int>(type: "int", nullable: false),
                    EntryType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedByUserId = table.Column<int>(type: "int", nullable: false),
                    InvoiceId = table.Column<int>(type: "int", nullable: true),
                    LitigationDisputeId = table.Column<int>(type: "int", nullable: true),
                    RefundInvoiceId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DuesEntries", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DuesEntries_LawyerProfiles_LawyerProfileId",
                        column: x => x.LawyerProfileId,
                        principalTable: "LawyerProfiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "LegalContracts",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ContractType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    LawyerProfileId = table.Column<int>(type: "int", nullable: true),
                    ProposalId = table.Column<int>(type: "int", nullable: true),
                    Title = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    FileName = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    FilePath = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    FileSize = table.Column<long>(type: "bigint", nullable: false),
                    IsAccepted = table.Column<bool>(type: "bit", nullable: false),
                    AcceptedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    GeneratedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LegalContracts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LegalContracts_LawyerProfiles_LawyerProfileId",
                        column: x => x.LawyerProfileId,
                        principalTable: "LawyerProfiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_LegalContracts_Proposals_ProposalId",
                        column: x => x.ProposalId,
                        principalTable: "Proposals",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "LitigationDisputes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    InvoiceId = table.Column<int>(type: "int", nullable: false),
                    ClientUserId = table.Column<int>(type: "int", nullable: false),
                    DisputeType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    DisputedAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Reason = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    Status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    AdminApproved = table.Column<bool>(type: "bit", nullable: false),
                    AdminApprovedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    AdminUserId = table.Column<int>(type: "int", nullable: true),
                    LawyerApproved = table.Column<bool>(type: "bit", nullable: false),
                    LawyerApprovedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LitigationDisputes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LitigationDisputes_Invoices_InvoiceId",
                        column: x => x.InvoiceId,
                        principalTable: "Invoices",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "SystemSettings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Key = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Value = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedByUserId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SystemSettings", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "RefundInvoices",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    LawyerProfileId = table.Column<int>(type: "int", nullable: false),
                    RefundInvoiceNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Reason = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    Status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    DuesEntryId = table.Column<int>(type: "int", nullable: true),
                    ContractId = table.Column<int>(type: "int", nullable: true),
                    GeneratedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    GeneratedByUserId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RefundInvoices", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RefundInvoices_DuesEntries_DuesEntryId",
                        column: x => x.DuesEntryId,
                        principalTable: "DuesEntries",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_RefundInvoices_LawyerProfiles_LawyerProfileId",
                        column: x => x.LawyerProfileId,
                        principalTable: "LawyerProfiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RefundInvoices_LegalContracts_ContractId",
                        column: x => x.ContractId,
                        principalTable: "LegalContracts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.InsertData(
                table: "SystemSettings",
                columns: new[] { "Id", "Description", "Key", "UpdatedAt", "UpdatedByUserId", "Value" },
                values: new object[] { 1, "Terms and Conditions shown to lawyers during registration", "LawyerRegistrationTnC", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 1, "TERMS AND CONDITIONS FOR LAWYER REGISTRATION\n\n1. By registering on LegalConnect, you agree to abide by all platform policies and applicable laws.\n2. You confirm that all information provided is accurate and complete.\n3. You agree to maintain confidentiality of client information.\n4. The platform reserves the right to suspend or terminate accounts for policy violations.\n5. Disputes are subject to the platform's arbitration process.\n6. These terms may be updated periodically; continued use constitutes acceptance." });

            migrationBuilder.CreateIndex(
                name: "IX_DuesEntries_LawyerProfileId",
                table: "DuesEntries",
                column: "LawyerProfileId");

            migrationBuilder.CreateIndex(
                name: "IX_LegalContracts_LawyerProfileId",
                table: "LegalContracts",
                column: "LawyerProfileId");

            migrationBuilder.CreateIndex(
                name: "IX_LegalContracts_ProposalId",
                table: "LegalContracts",
                column: "ProposalId");

            migrationBuilder.CreateIndex(
                name: "IX_LitigationDisputes_InvoiceId",
                table: "LitigationDisputes",
                column: "InvoiceId");

            migrationBuilder.CreateIndex(
                name: "IX_RefundInvoices_ContractId",
                table: "RefundInvoices",
                column: "ContractId");

            migrationBuilder.CreateIndex(
                name: "IX_RefundInvoices_DuesEntryId",
                table: "RefundInvoices",
                column: "DuesEntryId");

            migrationBuilder.CreateIndex(
                name: "IX_RefundInvoices_LawyerProfileId",
                table: "RefundInvoices",
                column: "LawyerProfileId");

            migrationBuilder.CreateIndex(
                name: "IX_RefundInvoices_RefundInvoiceNumber",
                table: "RefundInvoices",
                column: "RefundInvoiceNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SystemSettings_Key",
                table: "SystemSettings",
                column: "Key",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "LitigationDisputes");

            migrationBuilder.DropTable(
                name: "RefundInvoices");

            migrationBuilder.DropTable(
                name: "SystemSettings");

            migrationBuilder.DropTable(
                name: "DuesEntries");

            migrationBuilder.DropTable(
                name: "LegalContracts");
        }
    }
}
