namespace LabelVerify.Web.Models
{
    public class ReviewDashboardMetrics
    {
        public int TotalReviews { get; set; }
        public int ApprovedCount { get; set; }
        public int ReviewCount { get; set; }
        public int RejectedCount { get; set; }
        public double AverageScore { get; set; }
    }
}