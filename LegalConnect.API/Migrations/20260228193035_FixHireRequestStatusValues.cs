using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LegalConnect.API.Migrations
{
    /// <inheritdoc />
    public partial class FixHireRequestStatusValues : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Map old HireRequestStatus enum string values to new enum values
            migrationBuilder.Sql("UPDATE [HireRequests] SET [Status] = 'Inquiry' WHERE [Status] = 'Open'");
            migrationBuilder.Sql("UPDATE [HireRequests] SET [Status] = 'Rejected' WHERE [Status] IN ('Cancelled', 'Declined')");
            migrationBuilder.Sql("UPDATE [HireRequests] SET [Status] = 'DealInProgress' WHERE [Status] = 'ProposalSent'");
            migrationBuilder.Sql("UPDATE [HireRequests] SET [Status] = 'ConvertedToCase' WHERE [Status] IN ('Converted', 'ProposalAccepted')");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Reverse the status mappings
            migrationBuilder.Sql("UPDATE [HireRequests] SET [Status] = 'Open' WHERE [Status] = 'Inquiry'");
            migrationBuilder.Sql("UPDATE [HireRequests] SET [Status] = 'Declined' WHERE [Status] = 'Rejected'");
            migrationBuilder.Sql("UPDATE [HireRequests] SET [Status] = 'ProposalSent' WHERE [Status] = 'DealInProgress'");
            migrationBuilder.Sql("UPDATE [HireRequests] SET [Status] = 'Converted' WHERE [Status] = 'ConvertedToCase'");
        }
    }
}
