using LabelVerify.Web.Models;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace LabelVerify.Web.Services
{
    public class MonthlyCompliancePdfGenerator
    {
        public byte[] Generate(MonthlyComplianceReport report)
        {
            return Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Margin(30);
                    page.Header()
                        .Text("LabelVerify Monthly Compliance Report")
                        .FontSize(22)
                        .Bold();
                    page.Content().Column(col =>
                    {
                        col.Spacing(12);
                        col.Item().Text($"Report Month: {report.MonthName}");
                        col.Item().Text($"Generated: {report.GeneratedUtc:u}");
                        col.Item().LineHorizontal(1);
                        col.Item().Text("AI Executive Report").FontSize(15).Bold();
                        col.Item().Background(Colors.Blue.Lighten5).Padding(10).Text(report.AiReport ?? "No AI report generated.").FontSize(10);
                        col.Item().LineHorizontal(1);
                        col.Item().Text("Operational Metrics").FontSize(15).Bold();
                        col.Item().Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.RelativeColumn();
                                columns.RelativeColumn();
                            });

                            AddRow(table, "Total Reviews", report.Metrics.TotalReviews.ToString());
                            AddRow(table, "Open Reviews", report.Metrics.TotalOpenReviews.ToString());
                            AddRow(table, "Active Reviewers", report.Metrics.ActiveReviewers.ToString());
                            AddRow(table, "Reviews Per Reviewer", report.Metrics.ReviewsPerReviewer.ToString("0.0"));
                            AddRow(table, "Average Review Time", $"{report.Metrics.AverageReviewHours:0.0} Hours");
                            AddRow(table, "Oldest Open Review", $"{report.Metrics.OldestOpenReviewDays} Days");
                            AddRow(table, "Reviews Exceeding SLA", report.Metrics.ReviewsExceedingSla.ToString());
                        });
                        col.Item().Text("Disposition Rates").FontSize(15).Bold();
                        col.Item().Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.RelativeColumn();
                                columns.RelativeColumn();
                            });

                            AddRow(table, "Approval Rate", $"{report.Metrics.ApprovalRate:0.0}%");
                            AddRow(table, "Review Rate", $"{report.Metrics.ReviewRate:0.0}%");
                            AddRow(table, "Rejection Rate", $"{report.Metrics.RejectionRate:0.0}%");
                        });
                        col.Item().Text("Top Compliance Findings").FontSize(15).Bold();
                        col.Item().Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.RelativeColumn();
                                columns.RelativeColumn();
                            });

                            Header(table, "Field");
                            Header(table, "Findings");

                            foreach (var finding in report.Metrics.TopFindings)
                            {
                                Cell(table, finding.FieldName);
                                Cell(table, finding.FindingCount.ToString());
                            }
                        });
                        col.Item().Text("Reviewer Productivity").FontSize(15).Bold();
                        col.Item().Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.RelativeColumn(2);
                                columns.RelativeColumn();
                                columns.RelativeColumn();
                                columns.RelativeColumn();
                            });

                            Header(table, "Reviewer");
                            Header(table, "Completed");
                            Header(table, "Avg Hours");
                            Header(table, "Approval %");

                            foreach (var reviewer in report.Metrics.ReviewerLeaderboard)
                            {
                                Cell(table, reviewer.ReviewerName);
                                Cell(table, reviewer.ReviewsCompleted.ToString());
                                Cell(table, reviewer.AverageReviewHours.ToString("0.0"));
                                Cell(table, $"{reviewer.ApprovalRate:0.0}%");
                            }
                        });
                    });

                    page.Footer().AlignCenter().Text("Generated by LabelVerify AI");
                });
            }).GeneratePdf();
        }

        private static void AddRow(TableDescriptor table, string label, string value)
        {
            Cell(table, label);
            Cell(table, value);
        }

        private static void Header(TableDescriptor table, string text)
        {
            table.Cell()
                .Background(Colors.Grey.Lighten2)
                .Padding(5)
                .Text(text)
                .Bold();
        }

        private static void Cell(TableDescriptor table, string? text)
        {
            table.Cell()
                .BorderBottom(1)
                .BorderColor(Colors.Grey.Lighten2)
                .Padding(5)
                .Text(string.IsNullOrWhiteSpace(text) ? "N/A" : text);
        }
    }
}