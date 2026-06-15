namespace LabelVerify.Web.ViewModels
{
    public class ReviewerProductivityMetric
    {
        public string ReviewerName { get; set; } = string.Empty;
        public int ReviewsCompleted { get; set; }
        public double AverageReviewHours { get; set; }
        public double ApprovalRate { get; set; }
        public double ReviewRate { get; set; }
        public double RejectionRate { get; set; }
    }
}