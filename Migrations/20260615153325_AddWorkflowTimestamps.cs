using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LabelVerify.Web.Migrations
{
    /// <inheritdoc />
    public partial class AddWorkflowTimestamps : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "AssignedUtc",
                table: "ReviewSessions",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CompletedUtc",
                table: "ReviewSessions",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ReviewStartedUtc",
                table: "ReviewSessions",
                type: "datetime2",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AssignedUtc",
                table: "ReviewSessions");

            migrationBuilder.DropColumn(
                name: "CompletedUtc",
                table: "ReviewSessions");

            migrationBuilder.DropColumn(
                name: "ReviewStartedUtc",
                table: "ReviewSessions");
        }
    }
}
