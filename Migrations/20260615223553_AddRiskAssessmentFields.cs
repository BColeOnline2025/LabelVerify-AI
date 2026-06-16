using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LabelVerify.Web.Migrations
{
    /// <inheritdoc />
    public partial class AddRiskAssessmentFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AiRiskAssessment",
                table: "ReviewSessions",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RiskFactors",
                table: "ReviewSessions",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RiskLevel",
                table: "ReviewSessions",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "RiskScore",
                table: "ReviewSessions",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AiRiskAssessment",
                table: "ReviewSessions");

            migrationBuilder.DropColumn(
                name: "RiskFactors",
                table: "ReviewSessions");

            migrationBuilder.DropColumn(
                name: "RiskLevel",
                table: "ReviewSessions");

            migrationBuilder.DropColumn(
                name: "RiskScore",
                table: "ReviewSessions");
        }
    }
}
