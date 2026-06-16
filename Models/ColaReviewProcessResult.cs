namespace LabelVerify.Web.Models
{
    public class ColaReviewProcessResult
    {
        public Guid ReviewSessionId { get; set; }
        public ApprovedProductProfile ApprovedProfile { get; set; } = new();
        public LabelFacts ProductionFacts { get; set; } = new();
        public VerificationResult VerificationResult { get; set; } = new();
        public string? PackageOcrText { get; set; }
        public string? ProductionLabelOcrText { get; set; }
        public long ProcessingTimeMs { get; set; }
        public ComplianceAuditReport AuditReport { get; set; } = new();
    }
}