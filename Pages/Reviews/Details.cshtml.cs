using LabelVerify.Web.Models;
using LabelVerify.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.IO.Compression;
using System.Text.Json;

namespace LabelVerify.Web.Pages.Reviews
{
    public class DetailsModel(ReviewQueryService reviewQueryService, PdfAuditReportGenerator pdfAuditReportGenerator, 
        AzureBlobStorageService blobStorageService, ComplianceSummaryService complianceSummaryService,
        ReviewAuditLogService auditLogService) : PageModel
    {
        private readonly ReviewQueryService _reviewQueryService = reviewQueryService;
        private readonly PdfAuditReportGenerator _pdfAuditReportGenerator = pdfAuditReportGenerator;
        private readonly AzureBlobStorageService _blobStorageService = blobStorageService;
        private readonly ComplianceSummaryService _complianceSummaryService = complianceSummaryService;
        private readonly ReviewAuditLogService _auditLogService = auditLogService;

        public ReviewSession? Review { get; set; }
        public string? ColaPackageDownloadUrl { get; set; }
        public List<string> ProductionLabelDownloadUrls { get; set; } = [];
        public string ComplianceSummary { get; set; } = string.Empty;
        public List<ReviewAuditLog> AuditLogs { get; set; } = [];
        [BindProperty]
        public string ReviewerName { get; set; } = string.Empty;
        [BindProperty]
        public string ReviewerNotes { get; set; } = string.Empty;
        [BindProperty]
        public string FinalDisposition { get; set; } = string.Empty;
        [BindProperty]
        public string AssignedReviewer { get; set; } = "";

        public async Task<IActionResult> OnGetAsync(Guid id)
        {
            Review = await _reviewQueryService.GetByIdAsync(id);

            if (Review == null)
            {
                return NotFound();
            }

            if (!string.IsNullOrWhiteSpace(Review.ColaPackageBlobUrl))
            {
                ColaPackageDownloadUrl = _blobStorageService.GenerateReadSasUrl(Review.ColaPackageBlobUrl);
            }

            if (!string.IsNullOrWhiteSpace(Review.ProductionLabelBlobUrlsJson))
            {
                var urls = JsonSerializer.Deserialize<List<string>>(Review.ProductionLabelBlobUrlsJson) ?? [];

                ProductionLabelDownloadUrls = [.. urls.Select(url => _blobStorageService.GenerateReadSasUrl(url))];
            }

            if (Review.WorkflowStatus == "Assigned")
            {
                await _reviewQueryService.SaveWorkflowStatusAsync(Review.Id, "In Review");

                Review.WorkflowStatus = "In Review";

                await _auditLogService.LogAsync(Review.Id, "WorkflowStatusChanged", "Status changed from Assigned to In Review");
            }

            AuditLogs = await _reviewQueryService.GetAuditLogAsync(Review.Id);
            ReviewerName = Review.ReviewerName ?? "";
            ReviewerNotes = Review.ReviewerNotes ?? "";
            FinalDisposition = Review.FinalDisposition ?? Review.Recommendation;
            ComplianceSummary = _complianceSummaryService.Generate(BuildAuditReport(Review).VerificationResult);
            AssignedReviewer = Review.AssignedReviewer ?? "";

            return Page();
        }

        public async Task<IActionResult> OnGetExportPdfAsync(Guid id)
        {
            var review = await _reviewQueryService.GetByIdAsync(id);

            if (review == null)
            {
                return NotFound();
            }

            var report = BuildAuditReport(review);

            var pdfBytes = _pdfAuditReportGenerator.Generate(report);

            return File(pdfBytes, "application/pdf", $"Review_{id}.pdf");
        }

        public async Task<IActionResult> OnGetExportCsvAsync(Guid id)
        {
            var review = await _reviewQueryService.GetByIdAsync(id);

            if (review == null)
            {
                return NotFound();
            }

            var csv = BuildHistoricalCsv(review);

            return File(System.Text.Encoding.UTF8.GetBytes(csv), "text/csv", $"Review_{id}.csv");
        }

        private static ComplianceAuditReport BuildAuditReport(
            ReviewSession review)
        {
            var approved = JsonSerializer.Deserialize<ApprovedProductProfile>(review.ApprovedProfileJson)!;

            var production = JsonSerializer.Deserialize<LabelFacts>(review.ProductionFactsJson)!;

            return new ComplianceAuditReport
            {
                ReviewDate = review.ReviewDateUtc,
                Recommendation = review.Recommendation,
                OverallScore = review.OverallScore,
                ApprovedProfile = approved,
                ProductionFacts = production,
                VerificationResult = new VerificationResult
                {
                    Recommendation = review.Recommendation,
                    OverallScore = review.OverallScore,
                    Checks = [.. review.Results.Select(r => new FieldCheckResult
                    {
                        FieldName = r.FieldName,
                        ExpectedValue = r.ExpectedValue,
                        ActualValue = r.ActualValue,
                        SourceLabel = r.SourceLabel,
                        Status = r.Status,
                        ConfidenceScore = r.ConfidenceScore,
                        Notes = r.Notes
                    })]
                }
            };
        }

        private static string BuildHistoricalCsv(ReviewSession review)
        {
            var sb = new System.Text.StringBuilder();

            sb.AppendLine("Review Date,Recommendation,Overall Score,Field,Approved Value,Production Value,Found On,Status,Confidence,Notes");

            foreach (var item in review.Results)
            {
                sb.AppendLine(string.Join(",",
                    Escape(review.ReviewDateUtc.ToString("u")),
                    Escape(review.Recommendation),
                    Escape($"{review.OverallScore}%"),
                    Escape(item.FieldName),
                    Escape(item.ExpectedValue),
                    Escape(item.ActualValue),
                    Escape(item.SourceLabel),
                    Escape(item.Status),
                    Escape(item.Status == "Skipped" ? "N/A" : $"{item.ConfidenceScore}%"),
                    Escape(item.Notes)));
            }

            return sb.ToString();
        }

        private static string Escape(string? value)
        {
            value ??= string.Empty;

            return $"\"{value.Replace("\"", "\"\"")}\"";
        }

        public async Task<IActionResult> OnGetExportZipAsync(Guid id)
        {
            var review = await _reviewQueryService.GetByIdAsync(id);

            if (review == null)
            {
                return NotFound();
            }

            var report = BuildAuditReport(review);

            var pdfBytes = _pdfAuditReportGenerator.Generate(report);
            var csv = BuildHistoricalCsv(review);
            var metadataJson = BuildHistoricalMetadataJson(review);

            using var memoryStream = new MemoryStream();

            using (var archive = new ZipArchive(memoryStream, ZipArchiveMode.Create, true))
            {
                AddFileToZip(archive, "ComplianceReport.pdf", pdfBytes);

                AddFileToZip(archive, "ReviewResults.csv", System.Text.Encoding.UTF8.GetBytes(csv));

                AddFileToZip(archive, "review-metadata.json", System.Text.Encoding.UTF8.GetBytes(metadataJson));
            }

            return File(memoryStream.ToArray(), "application/zip", $"Review_{id}.zip");
        }

        private static void AddFileToZip(ZipArchive archive, string fileName, byte[] content)
        {
            var entry = archive.CreateEntry(fileName);

            using var entryStream = entry.Open();

            entryStream.Write(content, 0, content.Length);
        }

        private static string BuildHistoricalMetadataJson(ReviewSession review)
        {
            var metadata = new
            {
                review.Id,
                review.ReviewDateUtc,
                review.Recommendation,
                review.OverallScore,
                review.ProcessingTimeMs,
                review.ColaPackageFileName,
                review.UploadedLabelFiles,
                Results = review.Results.Select(x => new
                {
                    x.FieldName,
                    x.ExpectedValue,
                    x.ActualValue,
                    x.SourceLabel,
                    x.Status,
                    x.ConfidenceScore,
                    x.Notes
                })
            };

            return JsonSerializer.Serialize(metadata, new JsonSerializerOptions{WriteIndented = true});
        }

        public async Task<IActionResult> OnPostDispositionAsync(Guid id)
        {
            await _reviewQueryService.UpdateDispositionAsync(id, ReviewerName,
                FinalDisposition, ReviewerNotes);

            await _auditLogService.LogAsync(id, "DispositionUpdated",
                $"Disposition set to {FinalDisposition} by {ReviewerName}");

            return RedirectToPage(new { id });
        }

        public async Task<IActionResult> OnPostAssignAsync(Guid id)
        {
            await _reviewQueryService.AssignReviewerAsync(id, AssignedReviewer);

            await _auditLogService.LogAsync(id, "ReviewAssigned", $"Assigned to {AssignedReviewer}");

            return RedirectToPage(new { id });
        }
    }
}