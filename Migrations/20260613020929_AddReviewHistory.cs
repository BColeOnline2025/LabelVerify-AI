using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LabelVerify.Web.Migrations
{
    /// <inheritdoc />
    public partial class AddReviewHistory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ReviewSessions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ReviewDateUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Recommendation = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    OverallScore = table.Column<int>(type: "int", nullable: false),
                    ProcessingTimeMs = table.Column<long>(type: "bigint", nullable: false),
                    ColaPackageFileName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UploadedLabelFiles = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReviewSessions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ReviewResults",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ReviewSessionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FieldName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ExpectedValue = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ActualValue = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SourceLabel = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ConfidenceScore = table.Column<int>(type: "int", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReviewResults", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ReviewResults_ReviewSessions_ReviewSessionId",
                        column: x => x.ReviewSessionId,
                        principalTable: "ReviewSessions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ReviewResults_ReviewSessionId",
                table: "ReviewResults",
                column: "ReviewSessionId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ReviewResults");

            migrationBuilder.DropTable(
                name: "ReviewSessions");
        }
    }
}
