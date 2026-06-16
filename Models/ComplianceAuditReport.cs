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
        public string? AiComplianceSummary { get; set; }
        public string? AiRiskAssessment { get; set; }
        public string? RiskLevel { get; set; }
        public int RiskScore { get; set; }
        public string? RiskFactors { get; set; }
        public string? ReviewerName { get; set; }
        public string? FinalDisposition { get; set; }
        public string? ReviewerNotes { get; set; }
        public DateTime? DispositionDateUtc { get; set; }
        public Guid ReviewId { get; set; }
        public string? BrandName { get; set; }
        public DateTime ReviewDateUtc { get; set; }
        public string? WorkflowStatus { get; set; }
    }
}