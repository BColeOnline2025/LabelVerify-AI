namespace LabelVerify.Web.Models
{
    public class MonthlyComplianceReport
    {
        public DateTime GeneratedUtc { get; set; }
        public string MonthName { get; set; } = string.Empty;
        public string ExecutiveSummary { get; set; } = string.Empty;
        public string AiReport { get; set; } = string.Empty;
        public ReviewDashboardMetrics Metrics { get; set; } = new();
    }
}