using LabelVerify.Web.Models;
using LabelVerify.Web.Services;
using LabelVerify.Web.Services.Interfaces;
using LabelVerify.Web.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Caching.Memory;
using System.Diagnostics;
using System.IO.Compression;

namespace LabelVerify.Web.Pages
{
    public class ColaReviewModel(IColaPackageIngestionService colaPackageIngestionService,
        IOcrService ocrService, LabelFactExtractionService labelFactExtractionService,
        ColaPackageComparisonService comparisonService, ComplianceReportService complianceReportService,
        PdfAuditReportGenerator pdfAuditReportGenerator, IMemoryCache memoryCache) : PageModel
    {
        private readonly IColaPackageIngestionService _colaPackageIngestionService = colaPackageIngestionService;
        private readonly IOcrService _ocrService = ocrService;
        private readonly LabelFactExtractionService _labelFactExtractionService = labelFactExtractionService;
        private readonly ColaPackageComparisonService _comparisonService = comparisonService;
        private readonly ComplianceReportService _complianceReportService = complianceReportService;
        private readonly PdfAuditReportGenerator _pdfAuditReportGenerator = pdfAuditReportGenerator;
        private readonly IMemoryCache _memoryCache = memoryCache;

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

        public void OnGet()
        {
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (Input.ColaPackagePdf == null || Input.ColaPackagePdf.Length == 0)
            {
                ModelState.AddModelError("", "Please upload an approved COLA package PDF.");
                return Page();
            }

            if (Input.ProductionLabelImages == null || Input.ProductionLabelImages.Count == 0)
            {
                ModelState.AddModelError("", "Please upload at least one production label image.");
                return Page();
            }

            var sw = Stopwatch.StartNew();

            try
            {
                await using var packageStream = Input.ColaPackagePdf.OpenReadStream();

                var packageResult = await _colaPackageIngestionService.ExtractPackageAsync(packageStream);

                ApprovedProfile = packageResult.Profile;
                PackageOcrText = packageResult.RawOcrText;

                var productionOcrTexts = new List<string>();

                var combinedFacts = new LabelFacts();

                foreach (var file in Input.ProductionLabelImages)
                {
                    if (file.Length == 0)
                    {
                        continue;
                    }

                    await using var productionStream = file.OpenReadStream();

                    var ocrText = await _ocrService.ExtractTextAsync(productionStream);

                    productionOcrTexts.Add($"--- {file.FileName} ---");
                    productionOcrTexts.Add(ocrText);

                    var facts = _labelFactExtractionService.Extract(ocrText);

                    MergeProductionFacts(combinedFacts, facts, file.FileName);
                }

                ProductionLabelOcrText = string.Join(Environment.NewLine + Environment.NewLine, productionOcrTexts);

                ProductionFacts = combinedFacts;

                Result = _comparisonService.Compare(ApprovedProfile, ProductionFacts);

                ApplyFieldSourcesToResult(Result);

                var uploadedFiles = Input.ProductionLabelImages
                    .Select(x => x.FileName)
                    .ToList();

                var report = _complianceReportService.Create(ApprovedProfile, ProductionFacts, Result, uploadedFiles);

                ReviewId = Guid.NewGuid().ToString("N");

                _memoryCache.Set(ReviewId, report, TimeSpan.FromMinutes(30));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Unable to process review: {ex.Message}");
                return Page();
            }
            finally
            {
                sw.Stop();
                ProcessingTimeMs = sw.ElapsedMilliseconds;
            }

            return Page();
        }

        private void MergeProductionFacts(
            LabelFacts combined,
            LabelFacts current,
            string fileName)
        {
            SetIfPresent(
                current.BrandName,
                value => combined.BrandName = value,
                "Brand Name",
                fileName);

            SetIfPresent(
                current.FancifulName,
                value => combined.FancifulName = value,
                "Fanciful Name",
                fileName);

            SetIfPresent(
                current.ClassType,
                value => combined.ClassType = value,
                "Class / Type",
                fileName);

            SetIfPresent(
                current.AlcoholContent,
                value => combined.AlcoholContent = value,
                "Alcohol Content",
                fileName);

            SetIfPresent(
                current.NetContents,
                value => combined.NetContents = value,
                "Net Contents",
                fileName);

            SetIfPresent(
                current.GovernmentWarning,
                value => combined.GovernmentWarning = value,
                "Government Warning",
                fileName);

            SetIfPresent(
                current.ProducerStatement,
                value => combined.ProducerStatement = value,
                "Producer Statement",
                fileName);

            SetIfPresent(
                current.CountryOfOrigin,
                value => combined.CountryOfOrigin = value,
                "Country of Origin",
                fileName);

            SetIfPresent(
                current.Appellation,
                value => combined.Appellation = value,
                "Appellation",
                fileName);

            SetIfPresent(
                current.Varietal,
                value => combined.Varietal = value,
                "Varietal",
                fileName);
        }

        private void SetIfPresent(string value, Action<string> setValue, string fieldName, string fileName)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return;
            }

            setValue(value);

            if (!ProductionFieldSources.ContainsKey(fieldName))
            {
                ProductionFieldSources[fieldName] = fileName;
            }
        }

        private void ApplyFieldSourcesToResult(VerificationResult result)
        {
            foreach (var check in result.Checks)
            {
                if (ProductionFieldSources.TryGetValue(check.FieldName, out var sourceLabel))
                {
                    check.SourceLabel = sourceLabel;
                }
                else
                {
                    check.SourceLabel = "Not found";
                }
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

        private static string BuildCsv(
            ComplianceAuditReport report)
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
    }
}