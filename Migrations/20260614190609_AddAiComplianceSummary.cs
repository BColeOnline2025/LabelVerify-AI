using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LabelVerify.Web.Migrations
{
    /// <inheritdoc />
    public partial class AddAiComplianceSummary : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AiComplianceSummary",
                table: "ReviewSessions",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AiComplianceSummary",
                table: "ReviewSessions");
        }
    }
}
