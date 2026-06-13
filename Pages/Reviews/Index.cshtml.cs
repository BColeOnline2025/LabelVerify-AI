using LabelVerify.Web.Models;
using LabelVerify.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace LabelVerify.Web.Pages.Reviews
{
    public class IndexModel : PageModel
    {
        private readonly ReviewQueryService _reviewQueryService;

        [BindProperty(SupportsGet = true)]
        public string? Recommendation { get; set; }
        [BindProperty(SupportsGet = true)]
        public int? MinimumScore { get; set; }
        [BindProperty(SupportsGet = true)]
        public string? ColaPackageFileName { get; set; }
        [BindProperty(SupportsGet = true)]
        public DateTime? StartDate { get; set; }
        [BindProperty(SupportsGet = true)]
        public DateTime? EndDate { get; set; }
        public List<ReviewSession> Reviews { get; set; } = [];
        public ReviewDashboardMetrics Metrics { get; set; } = new();
        public List<FailureMetric> TopFailures { get; set; } = [];

        public IndexModel(ReviewQueryService reviewQueryService)
        {
            _reviewQueryService = reviewQueryService;
        }

        public async Task OnGetAsync()
        {
            var criteria = new ReviewSearchCriteria
            {
                Recommendation = Recommendation,
                MinimumScore = MinimumScore,
                ColaPackageFileName = ColaPackageFileName,
                StartDate = StartDate,
                EndDate = EndDate
            };

            Reviews = await _reviewQueryService.SearchAsync(criteria);
            Metrics = await _reviewQueryService.GetMetricsAsync();
            TopFailures = await _reviewQueryService.GetTopFailureReasonsAsync();
        }
    }
}