namespace LabelVerify.Web.Models
{
    public class ReviewResultEntity
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid ReviewSessionId { get; set; }
        public ReviewSession? ReviewSession { get; set; }
        public string FieldName { get; set; } = string.Empty;
        public string ExpectedValue { get; set; } = string.Empty;
        public string ActualValue { get; set; } = string.Empty;
        public string SourceLabel { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public double ConfidenceScore { get; set; }
        public string Notes { get; set; } = string.Empty;
        public string? AiAnalysis { get; set; }
    }
}