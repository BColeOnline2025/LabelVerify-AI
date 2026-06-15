using LabelVerify.Web.Models;
using LabelVerify.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace LabelVerify.Web.Pages.Reviews
{
    public class IndexModel(ReviewQueryService reviewQueryService, DashboardAnalyticsService dashboardAnalyticsService) : PageModel
    {
        private readonly ReviewQueryService _reviewQueryService = reviewQueryService;
        private readonly DashboardAnalyticsService _dashboardAnalyticsService = dashboardAnalyticsService;

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
        public ChartData WorkflowChart { get; set; } = new();
        public ChartData MonthlyReviewChart { get; set; } = new();
        public ChartData TopFailureChart { get; set; } = new();
        public ChartData ReviewerProductivityChart { get; set; } = new();
        public List<string> OperationalInsights { get; set; } = [];

        public async Task OnGetAsync()
        {
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

            Reviews = result.Items;
            TotalRecords = result.TotalRecords; 
            Metrics = await _reviewQueryService.GetMetricsAsync();
            TopFailures = await _reviewQueryService.GetTopFailureReasonsAsync();

            var workflowCounts = await _dashboardAnalyticsService.GetWorkflowStatusCountsAsync();

            WorkflowChart = new ChartData
            {
                Labels = [.. workflowCounts.Keys],
                Values = [.. workflowCounts.Values]
            };

            MonthlyReviewChart = await _dashboardAnalyticsService.GetMonthlyReviewCountsAsync();
            TopFailureChart = await _dashboardAnalyticsService.GetTopFailureReasonsChartAsync();
            ReviewerProductivityChart = await _dashboardAnalyticsService.GetReviewerProductivityChartAsync();
            OperationalInsights = await _dashboardAnalyticsService.GetOperationalInsightsAsync();
        }
    }
}