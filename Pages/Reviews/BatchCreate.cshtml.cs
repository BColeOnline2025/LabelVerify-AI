using LabelVerify.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace LabelVerify.Web.Pages.Reviews
{
    public class BatchCreateModel(ReviewBatchService reviewBatchService, AzureBlobStorageService azureblobStorageService) : PageModel
    {
        private readonly ReviewBatchService _reviewBatchService = reviewBatchService;
        private readonly AzureBlobStorageService _azureblobStorageService = azureblobStorageService;

        [BindProperty]
        public string BatchName { get; set; } = string.Empty;
        [BindProperty]
        public List<IFormFile> ColaPackages { get; set; } = [];

        public void OnGet()
        {
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (string.IsNullOrWhiteSpace(BatchName))
            {
                ModelState.AddModelError(nameof(BatchName), "Batch name is required.");
            }

            if (ColaPackages == null || ColaPackages.Count == 0)
            {
                ModelState.AddModelError(nameof(ColaPackages), "Upload at least one COLA package PDF.");
            }

            if (!ModelState.IsValid)
            {
                return Page();
            }

            var uploadedPackages = new List<(string FileName, string BlobUrl)>();

            foreach (var file in ColaPackages)
            {
                await using var stream = file.OpenReadStream();

                var blobName = $"{Guid.NewGuid()}_{file.FileName}";
                var blobUrl = await _azureblobStorageService.UploadAsync(stream, "review-packages", blobName, file.ContentType);

                uploadedPackages.Add((file.FileName, blobUrl.BlobUrl));
            }

            var createdBy = User.Identity?.Name ?? "Unknown";

            var batchId = await _reviewBatchService.CreateBatchAsync(BatchName, createdBy, uploadedPackages);

            return RedirectToPage("/Reviews/BatchDetails", new { id = batchId });
        }
    }
}