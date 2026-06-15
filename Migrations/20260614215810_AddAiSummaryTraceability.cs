using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LabelVerify.Web.Migrations
{
    /// <inheritdoc />
    public partial class AddAiSummaryTraceability : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AiModelUsed",
                table: "ReviewSessions",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "AiSummaryGeneratedUtc",
                table: "ReviewSessions",
                type: "datetime2",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AiModelUsed",
                table: "ReviewSessions");

            migrationBuilder.DropColumn(
                name: "AiSummaryGeneratedUtc",
                table: "ReviewSessions");
        }
    }
}
