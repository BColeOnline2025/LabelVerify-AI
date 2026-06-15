namespace LabelVerify.Web.Models
{
    public class ReviewSession
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public DateTime ReviewDateUtc { get; set; } = DateTime.UtcNow;
        public string Recommendation { get; set; } = string.Empty;
        public int OverallScore { get; set; }
        public long ProcessingTimeMs { get; set; }
        public string ColaPackageFileName { get; set; } = string.Empty;
        public string UploadedLabelFiles { get; set; } = string.Empty;
        public List<ReviewResultEntity> Results { get; set; } = [];
        public string ApprovedProfileJson { get; set; } = string.Empty;
        public string ProductionFactsJson { get; set; } = string.Empty;
        public string? ColaPackageBlobUrl { get; set; }
        public string? ProductionLabelBlobUrlsJson { get; set; }
        public string? AuditReportBlobUrl { get; set; }
        public string? ZipPackageBlobUrl { get; set; }
        public string? ReviewerName { get; set; }
        public string? ReviewerNotes { get; set; }
        public string? FinalDisposition { get; set; }
        public DateTime? DispositionDateUtc { get; set; }
        public string WorkflowStatus { get; set; } = "Submitted";
        public string? AssignedReviewer { get; set; }
        public DateTime? AssignedDateUtc { get; set; }
        public int AgeDays => (DateTime.UtcNow - ReviewDateUtc).Days;
        public string? AiComplianceSummary { get; set; }
        public DateTime? AiSummaryGeneratedUtc { get; set; }
        public string? AiModelUsed { get; set; }
        public string? AiPromptVersion { get; set; }
        public int? AiPromptTokens { get; set; }
        public int? AiCompletionTokens { get; set; }
        public int? AiTotalTokens { get; set; }
        public double? AiGenerationTimeMs { get; set; }
        public DateTime? AssignedUtc { get; set; }
        public DateTime? ReviewStartedUtc { get; set; }
        public DateTime? CompletedUtc { get; set; }
    }
}