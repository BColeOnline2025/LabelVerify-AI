using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LabelVerify.Web.Migrations
{
    /// <inheritdoc />
    public partial class AddBlobStorageColumns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AuditReportBlobUrl",
                table: "ReviewSessions",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ColaPackageBlobUrl",
                table: "ReviewSessions",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ProductionLabelBlobUrlsJson",
                table: "ReviewSessions",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ZipPackageBlobUrl",
                table: "ReviewSessions",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AuditReportBlobUrl",
                table: "ReviewSessions");

            migrationBuilder.DropColumn(
                name: "ColaPackageBlobUrl",
                table: "ReviewSessions");

            migrationBuilder.DropColumn(
                name: "ProductionLabelBlobUrlsJson",
                table: "ReviewSessions");

            migrationBuilder.DropColumn(
                name: "ZipPackageBlobUrl",
                table: "ReviewSessions");
        }
    }
}
