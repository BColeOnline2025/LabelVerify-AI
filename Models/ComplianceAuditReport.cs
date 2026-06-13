namespace LabelVerify.Web.Models
{
    public class ComplianceAuditReport
    {
        public DateTime ReviewDate { get; set; }
        public string Recommendation { get; set; } = string.Empty;
        public int OverallScore { get; set; }
        public ApprovedProductProfile ApprovedProfile { get; set; } = new();
        public LabelFacts ProductionFacts { get; set; }= new();
        public VerificationResult VerificationResult { get; set; } = new();
        public List<string> UploadedLabels { get; set; } = [];
    }
}