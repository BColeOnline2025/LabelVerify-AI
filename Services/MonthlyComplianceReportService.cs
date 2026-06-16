using LabelVerify.Web.Models;

namespace LabelVerify.Web.Services
{
    public class MonthlyComplianceReportService(ReviewQueryService reviewQueryService, AzureOpenAiSummaryService ai)
    {
        private readonly ReviewQueryService _reviewQueryService = reviewQueryService;
        private readonly AzureOpenAiSummaryService _ai = ai;

        public async Task<MonthlyComplianceReport>
            GenerateAsync()
        {
            var metrics = await _reviewQueryService.GetMetricsAsync();
            var report = await _ai.GenerateMonthlyComplianceReportAsync(metrics);

            return new MonthlyComplianceReport
            {
                GeneratedUtc = DateTime.UtcNow,
                MonthName = DateTime.UtcNow.ToString("MMMM yyyy"),
                Metrics = metrics,
                AiReport = report
            };
        }
    }
}