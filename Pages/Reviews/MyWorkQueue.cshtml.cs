using LabelVerify.Web.Models;
using LabelVerify.Web.Services;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace LabelVerify.Web.Pages.Reviews
{
    public class MyWorkQueueModel(ReviewBatchService reviewBatchService) : PageModel
    {
        private readonly ReviewBatchService _reviewBatchService = reviewBatchService;

        public List<ReviewBatchPackage> Packages { get; set; } = [];

        public async Task OnGetAsync()
        {
            var currentUser = User.Identity?.Name ?? "";

            Packages = await _reviewBatchService.GetAssignedPackagesAsync(currentUser);
        }
    }
}
