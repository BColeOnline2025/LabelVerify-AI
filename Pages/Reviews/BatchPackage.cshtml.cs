using LabelVerify.Web.Models;
using LabelVerify.Web.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace LabelVerify.Web.Pages.Reviews
{
    public class BatchPackageModel(ReviewBatchService reviewBatchService, ColaReviewOrchestrator colaReviewOrchestrator,
        AzureBlobStorageService blobStorageService, UserManager<ApplicationUser> userManager) : PageModel
    {
        private readonly ReviewBatchService _reviewBatchService = reviewBatchService;
        private readonly ColaReviewOrchestrator _colaReviewOrchestrator = colaReviewOrchestrator;
        private readonly AzureBlobStorageService _blobStorageService = blobStorageService;
        private readonly UserManager<ApplicationUser> _userManager = userManager;

        public ReviewBatchPackage? Package { get; set; }

        [BindProperty]
        public List<IFormFile> ProductionLabels { get; set; } = [];

        public async Task<IActionResult> OnGetAsync(Guid packageId)
        {
            await _reviewBatchService.MarkInReviewAsync(packageId);

            Package = await _reviewBatchService.GetPackageAsync(packageId);

            if (Package == null)
            {
                return NotFound();
            }

            return Page();
        }

        public async Task<IActionResult> OnPostAsync(Guid packageId)
        {
            await _reviewBatchService.MarkInReviewAsync(packageId);

            Package = await _reviewBatchService.GetPackageAsync(packageId);
           

            if (Package == null)
            {
                return NotFound();
            }

            if (ProductionLabels == null || ProductionLabels.Count == 0)
            {
                ModelState.AddModelError(nameof(ProductionLabels), "Upload at least one production label.");

                return Page();
            }

            try
            {
                await using var colaStream = await _blobStorageService.DownloadAsync(Package.ColaPackageBlobUrl);

                var currentUser = await _userManager.GetUserAsync(User);
                var submittedBy = currentUser.DisplayName;
                var processed = await _colaReviewOrchestrator.ProcessAsync(colaStream, Package.ColaPackageFileName, ProductionLabels, submittedBy);

                await _reviewBatchService.MarkPackageCompletedAsync(packageId, processed.ReviewSessionId);

                return RedirectToPage("/Reviews/BatchDetails", new { id = Package.ReviewBatchId });
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, ex.Message);

                return Page();
            }
        }
    }
}