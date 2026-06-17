using LabelVerify.Web.Models;
using LabelVerify.Web.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace LabelVerify.Web.Pages.Reviews
{
    public class MyWorkQueueModel(ReviewBatchService reviewBatchService, ReviewQueryService reviewQueryService,
        MyWorkQueueService myWorkQueueService, UserManager<ApplicationUser> userManager) : PageModel
    {
        private readonly ReviewBatchService _reviewBatchService = reviewBatchService;
        private readonly ReviewQueryService _reviewQueryService = reviewQueryService;
        private readonly MyWorkQueueService _myWorkQueueService = myWorkQueueService;
        private readonly UserManager<ApplicationUser> _userManager = userManager;

        public List<ReviewBatchPackage> Packages { get; set; } = [];
        public List<ReviewSession> ReviewSessions { get; set; } = [];
        public List<MyWorkQueueItem> Items { get; set; } = [];

        public async Task OnGetAsync()
        {
            var currentUser = await _userManager.GetUserAsync(User);

            Packages = await _reviewBatchService.GetAssignedPackagesAsync(currentUser.DisplayName);

            ReviewSessions = await _reviewQueryService.GetAssignedReviewsAsync(currentUser.DisplayName);

            Items = await _myWorkQueueService.GetMyWorkQueueAsync(currentUser.DisplayName);
        }
    }
}