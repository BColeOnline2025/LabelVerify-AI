using LabelVerify.Web.Models;
using LabelVerify.Web.Services;
using LabelVerify.Web.Services.Interfaces;
using LabelVerify.Web.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace LabelVerify.Web.Pages
{
    public class SingleModel(IOcrService ocrService, LabelVerificationService verificationService) : PageModel
    {
        private readonly IOcrService _ocrService = ocrService;
        private readonly LabelVerificationService _verificationService = verificationService;

        [BindProperty]
        public UploadLabelViewModel Input { get; set; } = new();
        public VerificationResult? Result { get; set; }
        public string? ExtractedText { get; set; }

        public void OnGet()
        {
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            if (Input.LabelFile == null || Input.LabelFile.Length == 0)
            {
                ModelState.AddModelError("", "Please upload a label image.");
                return Page();
            }

            await using var stream = Input.LabelFile.OpenReadStream();

            ExtractedText = await _ocrService.ExtractTextAsync(stream);

            var application = new LabelApplication
            {
                BrandName = Input.BrandName ?? string.Empty,
                ClassType = Input.ClassType ?? string.Empty,
                AlcoholContent = Input.AlcoholContent ?? string.Empty,
                NetContents = Input.NetContents ?? string.Empty,
                GovernmentWarning = Input.GovernmentWarning ?? string.Empty
            };

            Result = _verificationService.Verify(application, ExtractedText);

            return Page();
        }
    }
}