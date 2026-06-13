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
    }
}