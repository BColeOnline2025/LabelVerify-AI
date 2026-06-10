using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using LabelVerify.Web.Models;
using LabelVerify.Web.Services;
using LabelVerify.Web.Services.Interfaces;
using LabelVerify.Web.ViewModels;
using System.Diagnostics;

namespace LabelVerify.Web.Pages
{
    public class BatchModel : PageModel
    {
        private readonly IOcrService _ocrService;
        private readonly LabelVerificationService _verificationService;

        public BatchModel(
            IOcrService ocrService,
            LabelVerificationService verificationService)
        {
            _ocrService = ocrService;
            _verificationService = verificationService;
        }

        [BindProperty]
        public BatchUploadViewModel Input { get; set; } = new();

        public List<BatchResult> Results { get; set; } = [];

        public int TotalFiles => Results.Count;

        public int ApprovedCount =>
            Results.Count(x =>
                x.Recommendation.Contains("Approve",
                    StringComparison.OrdinalIgnoreCase));

        public int ReviewCount =>
            Results.Count(x =>
                x.Recommendation.Contains("Review",
                    StringComparison.OrdinalIgnoreCase));

        public int RejectCount =>
            Results.Count(x =>
                x.Recommendation.Contains("Reject",
                    StringComparison.OrdinalIgnoreCase));

        public double AverageProcessingTime =>
            Results.Any()
                ? Results.Average(x => x.ProcessingTimeMs)
                : 0;
        
        public void OnGet()
        {
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (Input.LabelFiles == null || !Input.LabelFiles.Any())
            {
                ModelState.AddModelError(
                    string.Empty,
                    "Please select one or more label files.");

                return Page();
            }

            foreach (var file in Input.LabelFiles)
            {
                try
                {
                    var sw = Stopwatch.StartNew();

                    await using var stream =
                        file.OpenReadStream();

                    var extractedText =
                        await _ocrService.ExtractTextAsync(stream);

                    sw.Stop();

                    var application =
                        new LabelApplication
                        {
                            BrandName = string.Empty,
                            ClassType = string.Empty,
                            AlcoholContent = string.Empty,
                            NetContents = string.Empty,
                            GovernmentWarning = string.Empty
                        };

                    var verificationResult =
                        _verificationService.Verify(
                            application,
                            extractedText);

                    Results.Add(
                        new BatchResult
                        {
                            FileName = file.FileName,
                            Recommendation =
                                verificationResult.Recommendation,
                            Status =
                                verificationResult.Passed
                                    ? "Pass"
                                    : "Review",
                            Score =
                                verificationResult.OverallScore,
                            ProcessingTimeMs =
                                sw.ElapsedMilliseconds
                        });
                }
                catch (Exception ex)
                {
                    Results.Add(
                        new BatchResult
                        {
                            FileName = file.FileName,
                            Recommendation = "Processing Error",
                            Score = 0,
                            ProcessingTimeMs = 0
                        });

                    ModelState.AddModelError(
                        string.Empty,
                        $"Error processing {file.FileName}: {ex.Message}");
                }
            }

            return Page();
        }
    }
}