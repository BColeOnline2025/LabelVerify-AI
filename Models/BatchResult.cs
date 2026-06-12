namespace LabelVerify.Web.Models
{
    public class BatchResult
    {
        public string FileName { get; set; } = string.Empty;
        public string Recommendation { get; set; } = string.Empty;
        public int Score { get; set; }
        public long ProcessingTimeMs { get; set; }
        public string Status { get; set; } = string.Empty;
        public bool MeetsPerformanceTarget => ProcessingTimeMs <= 5000;
    }
}