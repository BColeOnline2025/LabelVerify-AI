namespace LabelVerify.Web.Models
{
    public class FieldCheckResult
    {
        public string FieldName { get; set; } = string.Empty;
        public string ExpectedValue { get; set; } = string.Empty;
        public string ActualValue { get; set; } = string.Empty;
        public bool IsMatch { get; set; }
        public double ConfidenceScore { get; set; }
        public string Notes { get; set; } = string.Empty;
        public bool WasSkipped { get; set; }
        public string Status { get; set; } = string.Empty;
    }
}