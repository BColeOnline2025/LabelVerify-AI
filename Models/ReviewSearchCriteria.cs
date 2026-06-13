namespace LabelVerify.Web.Models
{
    public class ReviewSearchCriteria
    {
        public string? Recommendation { get; set; }
        public int? MinimumScore { get; set; }
        public string? ColaPackageFileName { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
    }
}