using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LabelVerify.Web.Migrations
{
    /// <inheritdoc />
    public partial class AddAiMetadata : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "AiCompletionTokens",
                table: "ReviewSessions",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "AiGenerationTimeMs",
                table: "ReviewSessions",
                type: "float",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "AiPromptTokens",
                table: "ReviewSessions",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AiPromptVersion",
                table: "ReviewSessions",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "AiTotalTokens",
                table: "ReviewSessions",
                type: "int",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AiCompletionTokens",
                table: "ReviewSessions");

            migrationBuilder.DropColumn(
                name: "AiGenerationTimeMs",
                table: "ReviewSessions");

            migrationBuilder.DropColumn(
                name: "AiPromptTokens",
                table: "ReviewSessions");

            migrationBuilder.DropColumn(
                name: "AiPromptVersion",
                table: "ReviewSessions");

            migrationBuilder.DropColumn(
                name: "AiTotalTokens",
                table: "ReviewSessions");
        }
    }
}
