using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LabelVerify.Web.Migrations
{
    /// <inheritdoc />
    public partial class AddReviewerDisposition : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "DispositionDateUtc",
                table: "ReviewSessions",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FinalDisposition",
                table: "ReviewSessions",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ReviewerName",
                table: "ReviewSessions",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ReviewerNotes",
                table: "ReviewSessions",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DispositionDateUtc",
                table: "ReviewSessions");

            migrationBuilder.DropColumn(
                name: "FinalDisposition",
                table: "ReviewSessions");

            migrationBuilder.DropColumn(
                name: "ReviewerName",
                table: "ReviewSessions");

            migrationBuilder.DropColumn(
                name: "ReviewerNotes",
                table: "ReviewSessions");
        }
    }
}
