using LabelVerify.Web.Models;
using LabelVerify.Web.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace LabelVerify.Web.Pages.Reviews
{
    public class BatchDetailsModel(ReviewBatchService reviewBatchService, UserManager<ApplicationUser> userManager) : PageModel
    {
        private readonly ReviewBatchService _reviewBatchService = reviewBatchService;
        private readonly UserManager<ApplicationUser> _userManager = userManager;

        public ReviewBatch? Batch { get; set; }
        public List<SelectListItem> ReviewerOptions { get; set; } = [];

        public async Task<IActionResult> OnGetAsync(Guid id)
        {
            Batch = await _reviewBatchService.GetBatchAsync(id);
            ReviewerOptions = [.. _userManager.Users
                .OrderBy(x => x.DisplayName)
                .Select(x => new SelectListItem
                {
                    Text = x.DisplayName,
                    Value = x.DisplayName
                })];

            if (Batch == null)
            {
                return NotFound();
            }

            return Page();
        }

        public async Task<IActionResult> OnPostAssignAsync(Guid packageId, Guid id, string reviewer)
        {
            await _reviewBatchService.AssignReviewerAsync(packageId, reviewer);

            return RedirectToPage(new { id });
        }
    }
}