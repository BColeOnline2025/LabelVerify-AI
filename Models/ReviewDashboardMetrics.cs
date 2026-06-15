namespace LabelVerify.Web.Models
{
    public class ReviewDashboardMetrics
    {
        public int TotalReviews { get; set; }
        public int ApprovedCount { get; set; }
        public int ReviewCount { get; set; }
        public int RejectedCount { get; set; }
        public double AverageScore { get; set; }
        public int Submitted { get; set; }
        public int Assigned { get; set; }
        public int InReview { get; set; }
        public int Approved { get; set; }
        public int Rejected { get; set; }
        public int AgingOver7Days { get; set; }
        public int AgingOver14Days { get; set; }
        public int MyAssigned { get; set; }
        public int MyInReview { get; set; }
        public int MyCompleted { get; set; }
        public int Unassigned { get; set; }
        public double AverageReviewHours { get; set; }
        public string FastestReviewer { get; set; } = "N/A";
        public double FastestReviewerAverageHours { get; set; }
        public int OldestOpenReviewDays { get; set; }
        public int ReviewsExceedingSla { get; set; }
        public double ApprovalRate { get; set; }
        public double ReviewRate { get; set; }
        public double RejectionRate { get; set; }
        public string? OperationalInsights { get; set; }
        public DateTime? OperationalInsightsGeneratedUtc { get; set; }
        public string? OperationalInsightsModel { get; set; }
    }
}