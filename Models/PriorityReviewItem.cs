namespace LabelVerify.Web.Models
{
    public class PriorityReviewItem
    {
        public Guid ReviewId { get; set; }
        public string BrandName { get; set; } = "";
        public int RiskScore { get; set; }
        public string RiskLevel { get; set; } = "";
        public string Recommendation { get; set; } = "";
        public string? AssignedReviewer { get; set; }
        public int DaysOpen { get; set; }
    }
}