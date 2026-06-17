using LabelVerify.Web.Models;
using LabelVerify.Web.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace LabelVerify.Web.Pages.Reviews
{
    public class DashboardModel(DashboardAnalyticsService dashboardAnalyticsService, 
        ReviewQueryService reviewQueryService, MonthlyComplianceReportService monthlyComplianceReportService,
        MonthlyCompliancePdfGenerator monthlyCompliancePdfGenerator, UserManager<ApplicationUser> userManager) : PageModel
    {
        private readonly DashboardAnalyticsService _dashboardAnalyticsService = dashboardAnalyticsService;
        private readonly ReviewQueryService _reviewQueryService = reviewQueryService;
        private readonly MonthlyComplianceReportService _monthlyComplianceReportService = monthlyComplianceReportService;
        private readonly MonthlyCompliancePdfGenerator _monthlyCompliancePdfGenerator = monthlyCompliancePdfGenerator;
        private readonly UserManager<ApplicationUser> _userManager = userManager;

        public string CurrentUser{ get; set; } = string.Empty;
        [BindProperty(SupportsGet = true)]
        public ReviewDashboardMetrics Metrics { get; set; } = new();
        public ChartData WorkflowChart { get; set; } = new();
        public ChartData MonthlyReviewChart { get; set; } = new();
        public ChartData TopFailureChart { get; set; } = new();
        public ChartData ReviewerProductivityChart { get; set; } = new();
        public List<FailureMetric> TopFailures { get; set; } = [];
        public ChartData TopFindingsChart { get; set; } = new();
        public List<string> OperationalInsights { get; set; } = [];
        public ChartData FindingsByMonthChart { get; set; } = new();

        public async Task OnGetAsync()
        {
            var currentUser = await _userManager.GetUserAsync(User);

            CurrentUser = currentUser.DisplayName;
            var workflowCounts = await _dashboardAnalyticsService.GetWorkflowStatusCountsAsync();

            WorkflowChart = new ChartData
            {
                Labels = [.. workflowCounts.Keys],
                Values = [.. workflowCounts.Values]
            };

            Metrics = await _reviewQueryService.GetMetricsAsync(CurrentUser);
            MonthlyReviewChart = await _dashboardAnalyticsService.GetMonthlyReviewCountsAsync();
            TopFailureChart = await _dashboardAnalyticsService.GetTopFailureReasonsChartAsync();
            ReviewerProductivityChart = await _dashboardAnalyticsService.GetReviewerProductivityChartAsync();
            TopFindingsChart = new ChartData
            {
                Labels = [.. Metrics.TopFindings.Select(x => x.FieldName)],

                Values = [.. Metrics.TopFindings.Select(x => x.FindingCount)]
            };
            ReviewerProductivityChart = new ChartData
            {
                Labels = [.. Metrics.ReviewerLeaderboard.Select(x => x.ReviewerName)],
                Values = [.. Metrics.ReviewerLeaderboard.Select(x => x.ReviewsCompleted)]
            };
            OperationalInsights = await _dashboardAnalyticsService.GetOperationalInsightsAsync();
            TopFailures = await _reviewQueryService.GetTopFailureReasonsAsync();
            FindingsByMonthChart = await _reviewQueryService.GetFindingsByMonthChartAsync();
        }

        public async Task<IActionResult> OnGetMonthlyReportAsync()
        {
            var currentUser = await _userManager.GetUserAsync(User);

            CurrentUser = currentUser.DisplayName;

            var report = await _monthlyComplianceReportService.GenerateAsync(CurrentUser);
            var pdf = _monthlyCompliancePdfGenerator.Generate(report);

            return File(pdf, "application/pdf", $"ComplianceReport_{DateTime.UtcNow:yyyyMMdd}.pdf");
        }
    }
}