using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LegalConnect.API.Migrations
{
    /// <inheritdoc />
    public partial class AddAppointmentSlotManagement : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "LawyerBlackoutBlocks",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    LawyerProfileId = table.Column<int>(type: "int", nullable: false),
                    DayOfWeek = table.Column<int>(type: "int", nullable: false),
                    StartTime = table.Column<TimeSpan>(type: "time", nullable: false),
                    EndTime = table.Column<TimeSpan>(type: "time", nullable: false),
                    Reason = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    RecurringPattern = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LawyerBlackoutBlocks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LawyerBlackoutBlocks_LawyerProfiles_LawyerProfileId",
                        column: x => x.LawyerProfileId,
                        principalTable: "LawyerProfiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "LawyerPersonalHolidays",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    LawyerProfileId = table.Column<int>(type: "int", nullable: false),
                    HolidayDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Reason = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    RecurringPattern = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LawyerPersonalHolidays", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LawyerPersonalHolidays_LawyerProfiles_LawyerProfileId",
                        column: x => x.LawyerProfileId,
                        principalTable: "LawyerProfiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "LawyerTimeSlotConfigurations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    LawyerProfileId = table.Column<int>(type: "int", nullable: false),
                    SessionDurationMinutes = table.Column<int>(type: "int", nullable: false),
                    BufferTimeMinutes = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LawyerTimeSlotConfigurations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LawyerTimeSlotConfigurations_LawyerProfiles_LawyerProfileId",
                        column: x => x.LawyerProfileId,
                        principalTable: "LawyerProfiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "LawyerWorkingHours",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    LawyerProfileId = table.Column<int>(type: "int", nullable: false),
                    DayOfWeek = table.Column<int>(type: "int", nullable: false),
                    StartTime = table.Column<TimeSpan>(type: "time", nullable: false),
                    EndTime = table.Column<TimeSpan>(type: "time", nullable: false),
                    IsWorking = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LawyerWorkingHours", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LawyerWorkingHours_LawyerProfiles_LawyerProfileId",
                        column: x => x.LawyerProfileId,
                        principalTable: "LawyerProfiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MasterHolidays",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    HolidayName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    HolidayDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    AppliesYearly = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MasterHolidays", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "LawyerHolidayPreferences",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    LawyerProfileId = table.Column<int>(type: "int", nullable: false),
                    MasterHolidayId = table.Column<int>(type: "int", nullable: false),
                    IsEnabled = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LawyerHolidayPreferences", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LawyerHolidayPreferences_LawyerProfiles_LawyerProfileId",
                        column: x => x.LawyerProfileId,
                        principalTable: "LawyerProfiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_LawyerHolidayPreferences_MasterHolidays_MasterHolidayId",
                        column: x => x.MasterHolidayId,
                        principalTable: "MasterHolidays",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_LawyerBlackoutBlocks_LawyerProfileId_DayOfWeek",
                table: "LawyerBlackoutBlocks",
                columns: new[] { "LawyerProfileId", "DayOfWeek" });

            migrationBuilder.CreateIndex(
                name: "IX_LawyerHolidayPreferences_LawyerProfileId_MasterHolidayId",
                table: "LawyerHolidayPreferences",
                columns: new[] { "LawyerProfileId", "MasterHolidayId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_LawyerHolidayPreferences_MasterHolidayId",
                table: "LawyerHolidayPreferences",
                column: "MasterHolidayId");

            migrationBuilder.CreateIndex(
                name: "IX_LawyerPersonalHolidays_LawyerProfileId",
                table: "LawyerPersonalHolidays",
                column: "LawyerProfileId");

            migrationBuilder.CreateIndex(
                name: "IX_LawyerTimeSlotConfigurations_LawyerProfileId",
                table: "LawyerTimeSlotConfigurations",
                column: "LawyerProfileId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_LawyerWorkingHours_LawyerProfileId_DayOfWeek",
                table: "LawyerWorkingHours",
                columns: new[] { "LawyerProfileId", "DayOfWeek" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "LawyerBlackoutBlocks");

            migrationBuilder.DropTable(
                name: "LawyerHolidayPreferences");

            migrationBuilder.DropTable(
                name: "LawyerPersonalHolidays");

            migrationBuilder.DropTable(
                name: "LawyerTimeSlotConfigurations");

            migrationBuilder.DropTable(
                name: "LawyerWorkingHours");

            migrationBuilder.DropTable(
                name: "MasterHolidays");
        }
    }
}
