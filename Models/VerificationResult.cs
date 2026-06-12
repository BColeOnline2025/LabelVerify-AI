namespace LabelVerify.Web.Models
{
    public class VerificationResult
    {
        public List<FieldCheckResult> Checks { get; set; } = [];
        public int OverallScore { get; set; }
        public string Recommendation { get; set; } = string.Empty;
        public bool Passed => Checks.All(x => x.IsMatch);
        public int ExecutedChecks => Checks.Count(x => !x.WasSkipped);
        public int SkippedChecks => Checks.Count(x => x.WasSkipped);
    }
}