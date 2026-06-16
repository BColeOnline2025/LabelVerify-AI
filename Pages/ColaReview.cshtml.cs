using LabelVerify.Web.Models;
using LabelVerify.Web.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Rendering;
using LabelVerify.Web.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Caching.Memory;
using System.IO.Compression;

namespace LabelVerify.Web.Pages
{
    public class ColaReviewModel(IMemoryCache memoryCache, PdfAuditReportGenerator pdfAuditReportGenerator,
        ILogger<ColaReviewModel> logger, ReviewAuditLogService auditLogService, UserManager<ApplicationUser> userManager,
        ReviewQueryService reviewQueryService, ColaReviewOrchestrator colaReviewOrchestrator) : PageModel
    {
        private readonly IMemoryCache _memoryCache = memoryCache;
        private readonly ILogger<ColaReviewModel> _logger = logger;
        private readonly ReviewAuditLogService _auditLogService = auditLogService;
        private readonly ReviewQueryService _reviewQueryService = reviewQueryService;
        private readonly ColaReviewOrchestrator _colaReviewOrchestrator = colaReviewOrchestrator;
        private readonly PdfAuditReportGenerator _pdfAuditReportGenerator = pdfAuditReportGenerator;
        private readonly UserManager<ApplicationUser> _userManager = userManager;

        [BindProperty]
        public ColaReviewUploadViewModel Input { get; set; } = new();
        public ApprovedProductProfile? ApprovedProfile { get; set; }
        public LabelFacts? ProductionFacts { get; set; }
        public VerificationResult? Result { get; set; }
        public string? PackageOcrText { get; set; }
        public string? ProductionLabelOcrText { get; set; }
        public Dictionary<string, string> ProductionFieldSources { get; set; } = [];
        public long ProcessingTimeMs { get; set; }
        public string? ReviewId { get; set; }
        public string? ExportCacheId { get; set; }
        [BindProperty]
        public string? ReviewerName { get; set; } = string.Empty;
        [BindProperty]
        public string? ReviewerNotes { get; set; } = string.Empty;
        [BindProperty]
        public string? FinalDisposition { get; set; } = string.Empty;
        public List<SelectListItem> ReviewerOptions { get; set; } = [];

        public void OnGet()
        {
            LoadReviewers();

            ReviewerName = User.Identity?.Name;
        }

        public async Task<IActionResult> OnPostAsync()
        {
            LoadReviewers();

            ReviewerName = User.Identity?.Name;
            
            _logger.LogInformation("COLA review started.");

            if (Input.ColaPackagePdf == null || Input.ColaPackagePdf.Length == 0)
            {
                ModelState.AddModelError(string.Empty, "Please upload an approved COLA package PDF.");

                return Page();
            }

            if (Input.ProductionLabelImages == null || Input.ProductionLabelImages.Count == 0)
            {
                ModelState.AddModelError(string.Empty, "Please upload at least one production label image.");

                return Page();
            }

            try
            {
                var submittedBy = User.Identity?.Name ?? "Unknown";

                var processed = await _colaReviewOrchestrator.ProcessAsync(Input.ColaPackagePdf, Input.ProductionLabelImages, submittedBy);

                ApprovedProfile = processed.ApprovedProfile;
                ProductionFacts = processed.ProductionFacts;
                Result = processed.VerificationResult;
                PackageOcrText = processed.PackageOcrText;
                ProductionLabelOcrText = processed.ProductionLabelOcrText;
                ProcessingTimeMs = processed.ProcessingTimeMs;
                ReviewId = processed.ReviewSessionId.ToString();
                FinalDisposition = Result.Recommendation;
                ExportCacheId = Guid.NewGuid().ToString("N");

                _memoryCache.Set(ExportCacheId, processed.AuditReport, TimeSpan.FromMinutes(30));

                _logger.LogInformation("COLA review completed. ReviewSessionId={ReviewSessionId}, ProcessingTimeMs={ProcessingTimeMs}",
                    processed.ReviewSessionId, processed.ProcessingTimeMs);

                return Page();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "COLA review failed.");

                ModelState.AddModelError(string.Empty, $"Unable to process review: {ex.Message}");

                return Page();
            }
        }

        public IActionResult OnGetExport(string reviewId)
        {
            if (string.IsNullOrWhiteSpace(reviewId) || !_memoryCache.TryGetValue(reviewId, out ComplianceAuditReport? report) ||
                report == null)
            {
                return RedirectToPage();
            }

            var pdfBytes = _pdfAuditReportGenerator.Generate(report);

            return File(pdfBytes, "application/pdf", $"LabelVerify_Report_{DateTime.Now:yyyyMMdd_HHmmss}.pdf");
        }

        public IActionResult OnGetExportCsv(string reviewId)
        {
            if (string.IsNullOrWhiteSpace(reviewId) || !_memoryCache.TryGetValue(reviewId, out ComplianceAuditReport? report) || report == null)
            {
                return RedirectToPage();
            }

            var csv = BuildCsv(report);

            return File(System.Text.Encoding.UTF8.GetBytes(csv), "text/csv", $"LabelVerify_Report_{DateTime.Now:yyyyMMdd_HHmmss}.csv");
        }

        private static string BuildCsv(ComplianceAuditReport report)
        {
            var sb = new System.Text.StringBuilder();

            sb.AppendLine("LabelVerify Compliance Report");
            sb.AppendLine();

            sb.AppendLine($"Review Date,{report.ReviewDate:u}");
            sb.AppendLine($"Recommendation,{report.Recommendation}");
            sb.AppendLine($"Overall Score,{report.OverallScore}%");
            sb.AppendLine();

            sb.AppendLine("Field,Approved Value,Production Value,Found On,Status,Confidence");

            foreach (var check in report.VerificationResult.Checks)
            {
                sb.AppendLine(string.Join(",",
                    Escape(check.FieldName),
                    Escape(check.ExpectedValue),
                    Escape(check.ActualValue),
                    Escape(check.SourceLabel),
                    Escape(check.Status),
                    check.WasSkipped ? "N/A" : $"{check.ConfidenceScore}%"));
            }

            return sb.ToString();
        }

        private static string Escape(string? value)
        {
            value ??= string.Empty;

            return $"\"{value.Replace("\"", "\"\"")}\"";
        }

        public IActionResult OnGetExportDetailedCsv(string reviewId)
        {
            if (string.IsNullOrWhiteSpace(reviewId) || !_memoryCache.TryGetValue(reviewId, out ComplianceAuditReport? report) ||
                report == null)
            {
                return RedirectToPage();
            }

            var csv = BuildDetailedCsv(report);

            return File(System.Text.Encoding.UTF8.GetBytes(csv), "text/csv", $"LabelVerify_Detailed_Report_{DateTime.Now:yyyyMMdd_HHmmss}.csv");
        }

        private static string BuildDetailedCsv(ComplianceAuditReport report)
        {
            var sb = new System.Text.StringBuilder();

            sb.AppendLine("Review Date,Recommendation,Overall Score,Field,Approved Value,Production Value,Found On,Status,Confidence,Notes");

            foreach (var check in report.VerificationResult.Checks)
            {
                sb.AppendLine(string.Join(",",
                    Escape(report.ReviewDate.ToString("u")),
                    Escape(report.Recommendation),
                    Escape($"{report.OverallScore}%"),
                    Escape(check.FieldName),
                    Escape(check.ExpectedValue),
                    Escape(check.ActualValue),
                    Escape(check.SourceLabel),
                    Escape(check.Status),
                    Escape(check.WasSkipped ? "N/A" : $"{check.ConfidenceScore}%"),
                    Escape(check.Notes)));
            }

            return sb.ToString();
        }

        public IActionResult OnGetExportZip(string reviewId)
        {
            if (string.IsNullOrWhiteSpace(reviewId) || !_memoryCache.TryGetValue(reviewId, out ComplianceAuditReport? report) ||
                report == null)
            {
                return RedirectToPage();
            }

            var pdfBytes = _pdfAuditReportGenerator.Generate(report);

            var summaryCsv = BuildCsv(report);
            var detailedCsv = BuildDetailedCsv(report);
            var metadataJson = BuildMetadataJson(report);

            using var memoryStream = new MemoryStream();

            using (var archive = new ZipArchive(memoryStream, ZipArchiveMode.Create, true))
            {
                AddFileToZip(archive, "ComplianceReport.pdf", pdfBytes);
                AddFileToZip(archive, "SummaryReport.csv", System.Text.Encoding.UTF8.GetBytes(summaryCsv));
                AddFileToZip(archive, "DetailedReport.csv", System.Text.Encoding.UTF8.GetBytes(detailedCsv));
                AddFileToZip(archive, "review-metadata.json", System.Text.Encoding.UTF8.GetBytes(metadataJson));
            }

            return File(memoryStream.ToArray(), "application/zip", $"LabelVerify_ReviewPackage_{DateTime.Now:yyyyMMdd_HHmmss}.zip");
        }

        private static void AddFileToZip(ZipArchive archive, string fileName, byte[] content)
        {
            var entry = archive.CreateEntry(fileName);

            using var entryStream = entry.Open();

            entryStream.Write(content, 0, content.Length);
        }

        private static string BuildMetadataJson(ComplianceAuditReport report)
        {
            var metadata = new
            {
                report.ReviewDate,
                report.Recommendation,
                report.OverallScore,
                report.UploadedLabels,
                ApprovedProduct = new
                {
                    report.ApprovedProfile.BrandName,
                    report.ApprovedProfile.FancifulName,
                    report.ApprovedProfile.ClassType,
                    report.ApprovedProfile.AlcoholContent,
                    report.ApprovedProfile.NetContents
                },
                Results = report.VerificationResult.Checks.Select(x => new
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

            return System.Text.Json.JsonSerializer.Serialize(metadata,
                new System.Text.Json.JsonSerializerOptions
                {
                    WriteIndented = true
                });
        }

        public async Task<IActionResult> OnPostDispositionAsync(Guid id)
        {
            await _reviewQueryService.UpdateDispositionAsync(id, ReviewerName, FinalDisposition, ReviewerNotes);
            await _auditLogService.LogAsync(id, "DispositionUpdated", $"Disposition set to {FinalDisposition} by {ReviewerName}");

            return RedirectToPage(new { id });
        }

        private void LoadReviewers()
        {
            ReviewerOptions = [.. _userManager.Users
                .OrderBy(x => x.UserName)
                .Select(x => new SelectListItem
                {
                    Value = x.UserName!,
                    Text = string.IsNullOrWhiteSpace(x.Email)
                        ? x.UserName!
                        : $"{x.UserName} ({x.Email})"
                })];
        }
    }
}