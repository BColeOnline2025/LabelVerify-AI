using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LabelVerify.Web.Migrations
{
    /// <inheritdoc />
    public partial class AddReviewBatches : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ReviewBatches",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BatchName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReviewBatches", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ReviewBatchPackages",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ReviewBatchId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ColaPackageFileName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ColaPackageBlobUrl = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ReviewSessionId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    UploadedUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CompletedUtc = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReviewBatchPackages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ReviewBatchPackages_ReviewBatches_ReviewBatchId",
                        column: x => x.ReviewBatchId,
                        principalTable: "ReviewBatches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ReviewBatchPackages_ReviewBatchId",
                table: "ReviewBatchPackages",
                column: "ReviewBatchId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ReviewBatchPackages");

            migrationBuilder.DropTable(
                name: "ReviewBatches");
        }
    }
}
