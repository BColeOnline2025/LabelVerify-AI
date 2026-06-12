using LabelVerify.Web.Models;
using LabelVerify.Web.Services;
using LabelVerify.Web.Services.Interfaces;
using LabelVerify.Web.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Diagnostics;

namespace LabelVerify.Web.Pages
{
    public class CompareLabelsModel : PageModel
    {
        private readonly IOcrService _ocrService;
        private readonly LabelFactExtractionService _factExtractionService;
        private readonly LabelComparisonService _comparisonService;

        public CompareLabelsModel(
            IOcrService ocrService,
            LabelFactExtractionService factExtractionService,
            LabelComparisonService comparisonService)
        {
            _ocrService = ocrService;
            _factExtractionService = factExtractionService;
            _comparisonService = comparisonService;
        }

        [BindProperty]
        public LabelComparisonUploadViewModel Input { get; set; } = new();

        public string? ApprovedLabelText { get; set; }

        public string? ProductionLabelText { get; set; }

        public LabelFacts? ApprovedFacts { get; set; }

        public LabelFacts? ProductionFacts { get; set; }

        public VerificationResult? Result { get; set; }

        public long ProcessingTimeMs { get; set; }

        public void OnGet()
        {
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (Input.ApprovedLabelFile == null || Input.ApprovedLabelFile.Length == 0)
            {
                ModelState.AddModelError("", "Please upload the approved label.");
                return Page();
            }

            if (Input.ProductionLabelFile == null || Input.ProductionLabelFile.Length == 0)
            {
                ModelState.AddModelError("", "Please upload the production label.");
                return Page();
            }

            var sw = Stopwatch.StartNew();

            await using var approvedStream = Input.ApprovedLabelFile.OpenReadStream();
            ApprovedLabelText = await _ocrService.ExtractTextAsync(approvedStream);
            ApprovedFacts = _factExtractionService.Extract(ApprovedLabelText);

            await using var productionStream = Input.ProductionLabelFile.OpenReadStream();
            ProductionLabelText = await _ocrService.ExtractTextAsync(productionStream);
            ProductionFacts = _factExtractionService.Extract(ProductionLabelText);

            Result = _comparisonService.Compare(ApprovedFacts, ProductionFacts);

            sw.Stop();
            ProcessingTimeMs = sw.ElapsedMilliseconds;

            return Page();
        }
    }
}