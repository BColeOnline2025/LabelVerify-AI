using LabelVerify.Web.ViewModels;

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
        public List<ReviewerProductivityMetric> ReviewerLeaderboard { get; set; } = [];
        public List<FindingMetric> TopFindings { get; set; } = [];
        public int TotalOpenReviews { get; set; }
        public int ActiveReviewers { get; set; }
        public double ReviewsPerReviewer { get; set; }
        public List<PriorityReviewItem> PriorityQueue { get; set; } = [];
        public int HighRiskReviews { get; set; }
        public int MediumRiskReviews { get; set; }
        public int LowRiskReviews { get; set; }
        public string? QueueRecommendation { get; set; }
        public DateTime? QueueRecommendationGeneratedUtc { get; set; }
        public string? QueueRecommendationModel { get; set; }
    }
}