namespace LabelVerify.Web.Models
{
    public class ReviewAuditLog
    {
        public Guid Id { get; set; }
        public Guid ReviewSessionId { get; set; }
        public DateTime TimestampUtc { get; set; }
        public string EventType { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public string Severity { get; set; } = "Information";
    }
}