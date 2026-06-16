using LabelVerify.Web.Models;
using LabelVerify.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace LabelVerify.Web.Pages.Reviews
{
    public class IndexModel(ReviewQueryService reviewQueryService) : PageModel
    {
        private readonly ReviewQueryService _reviewQueryService = reviewQueryService;

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

        public async Task OnGetAsync()
        {
            var currentUser = User.Identity?.Name ?? "Unknown";
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

            Metrics = await _reviewQueryService.GetMetricsAsync(currentUser);
            Reviews = result.Items;
            TotalRecords = result.TotalRecords; 
        }
    }
}