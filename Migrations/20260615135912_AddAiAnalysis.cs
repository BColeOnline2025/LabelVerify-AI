using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LabelVerify.Web.Migrations
{
    /// <inheritdoc />
    public partial class AddAiAnalysis : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AiAnalysis",
                table: "ReviewResults",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AiAnalysis",
                table: "ReviewResults");
        }
    }
}
