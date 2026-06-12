using LabelVerify.Web.Models;
using LabelVerify.Web.Services;
using LabelVerify.Web.Services.Interfaces;
using LabelVerify.Web.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Diagnostics;

namespace LabelVerify.Web.Pages
{
    public class ColaReviewModel(
        IColaPackageIngestionService colaPackageIngestionService,
        IOcrService ocrService) : PageModel
    {
        private readonly IColaPackageIngestionService _colaPackageIngestionService = colaPackageIngestionService;
        private readonly IOcrService _ocrService = ocrService;

        [BindProperty]
        public ColaReviewUploadViewModel Input { get; set; } = new();

        public ApprovedProductProfile? ApprovedProfile { get; set; }

        public string? PackageOcrText { get; set; }

        public string? ProductionLabelOcrText { get; set; }

        public long ProcessingTimeMs { get; set; }

        public void OnGet()
        {
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (Input.ColaPackagePdf == null || Input.ColaPackagePdf.Length == 0)
            {
                ModelState.AddModelError(string.Empty, "Please upload an approved COLA package PDF.");

                return Page();
            }

            var sw = Stopwatch.StartNew();

            try
            {
                await using var packageStream = Input.ColaPackagePdf.OpenReadStream();

                var result = await _colaPackageIngestionService.ExtractPackageAsync(packageStream);

                ApprovedProfile = result.Profile;

                PackageOcrText = result.RawOcrText;

                if (string.IsNullOrWhiteSpace(PackageOcrText))
                {
                    throw new Exception("Document Intelligence returned no text.");
                }

                if (Input.ProductionLabelImage != null && Input.ProductionLabelImage.Length > 0)
                {
                    await using var productionStream = Input.ProductionLabelImage.OpenReadStream();

                    ProductionLabelOcrText = await _ocrService.ExtractTextAsync(productionStream);
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, $"Unable to process the COLA package: {ex.Message}");

                return Page();
            }
            finally
            {
                sw.Stop();
                ProcessingTimeMs = sw.ElapsedMilliseconds;
            }

            return Page();
        }
    }
}