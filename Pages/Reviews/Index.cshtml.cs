using LabelVerify.Web.Models;
using LabelVerify.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace LabelVerify.Web.Pages.Reviews
{
    public class IndexModel(ReviewQueryService reviewQueryService, UserManager<ApplicationUser> userManager) : PageModel
    {
        private readonly ReviewQueryService _reviewQueryService = reviewQueryService;
        private readonly UserManager<ApplicationUser> _userManager = userManager;

        public string? ColaPackageFileName { get; set; }
        [BindProperty(SupportsGet = true)]
        public string? Recommendation { get; set; }
        [BindProperty(SupportsGet = true)]
        public int? MinimumScore { get; set; }
        [BindProperty(SupportsGet = true)]
        public ReviewDashboardMetrics Metrics { get; set; } = new();
        [BindProperty(SupportsGet = true)]
        public DateTime? StartDate { get; set; }
        [BindProperty(SupportsGet = true)]
        public DateTime? EndDate { get; set; }
        public List<ReviewSession> Reviews { get; set; } = [];
        [BindProperty(SupportsGet = true)]
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public int TotalRecords { get; set; }
        public int TotalPages => (int)Math.Ceiling((double)TotalRecords / PageSize);
        [BindProperty(SupportsGet = true)]
        public string? WorkflowStatus { get; set; }
        [BindProperty(SupportsGet = true)]
        public string? AssignedReviewer { get; set; }
        [BindProperty(SupportsGet = true)]
        public string? FinalDisposition { get; set; }
        public List<SelectListItem> ReviewerOptions { get; set; } = [];

        public async Task OnGetAsync()
        {
            var currentUser = await _userManager.GetUserAsync(User);
            var criteria = new ReviewSearchCriteria
            {
                Recommendation = Recommendation,
                MinimumScore = MinimumScore,
                ColaPackageFileName = ColaPackageFileName,
                StartDate = StartDate,
                EndDate = EndDate,
                WorkflowStatus = WorkflowStatus,
                AssignedReviewer = AssignedReviewer,
                FinalDisposition = FinalDisposition
            };

            var result = await _reviewQueryService.SearchPagedAsync(criteria, PageNumber, PageSize);

            Metrics = await _reviewQueryService.GetMetricsAsync(currentUser.DisplayName);
            Reviews = result.Items;
            TotalRecords = result.TotalRecords;

            ReviewerOptions = [.. _userManager.Users
                .OrderBy(x => x.DisplayName)
                .ThenBy(x => x.Email)
                .Select(x => new SelectListItem
                {
                    Value = x.DisplayName,
                    Text = x.DisplayName
                })];
        }
    }
}