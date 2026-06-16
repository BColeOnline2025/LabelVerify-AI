using LabelVerify.Web.Models;
using LabelVerify.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace LabelVerify.Web.Pages.Reviews
{
    public class DashboardModel(DashboardAnalyticsService dashboardAnalyticsService, 
        ReviewQueryService reviewQueryService, MonthlyComplianceReportService monthlyComplianceReportService,
        MonthlyCompliancePdfGenerator monthlyCompliancePdfGenerator) : PageModel
    {
        private readonly DashboardAnalyticsService _dashboardAnalyticsService = dashboardAnalyticsService;
        private readonly ReviewQueryService _reviewQueryService = reviewQueryService;
        private readonly MonthlyComplianceReportService _monthlyComplianceReportService = monthlyComplianceReportService;
        private readonly MonthlyCompliancePdfGenerator _monthlyCompliancePdfGenerator = monthlyCompliancePdfGenerator;

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
            var currentUser = User.Identity?.Name ?? "Unknown";
            var workflowCounts = await _dashboardAnalyticsService.GetWorkflowStatusCountsAsync();

            WorkflowChart = new ChartData
            {
                Labels = [.. workflowCounts.Keys],
                Values = [.. workflowCounts.Values]
            };

            Metrics = await _reviewQueryService.GetMetricsAsync(currentUser);
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
            var currentUser = User.Identity?.Name ?? "Unknown";

            var report = await _monthlyComplianceReportService.GenerateAsync(currentUser);
            var pdf = _monthlyCompliancePdfGenerator.Generate(report);

            return File(pdf, "application/pdf", $"ComplianceReport_{DateTime.UtcNow:yyyyMMdd}.pdf");
        }
    }
}