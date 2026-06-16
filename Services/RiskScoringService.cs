using LabelVerify.Web.Models;

namespace LabelVerify.Web.Services
{
    public class RiskScoringService
    {
        public RiskAssessment Calculate(VerificationResult result)
        {
            var score = 0;
            var factors = new List<string>();
            var failCount = result.Checks.Count(x => x.Status == "Fail");
            var reviewCount = result.Checks.Count(x => x.Status == "Review");

            score += failCount * 20;
            score += reviewCount * 10;

            if (failCount > 0)
                factors.Add($"{failCount} failed checks");

            if (reviewCount > 0)
                factors.Add($"{reviewCount} review checks");

            if (result.OverallScore < 90)
            {
                score += 20;
                factors.Add("Low overall confidence");
            }

            if (result.OverallScore < 80)
            {
                score += 20;
                factors.Add("Very low confidence");
            }

            var level = score >= 70 ? "High" : score >= 40 ? "Medium" : "Low";

            return new RiskAssessment
            {
                RiskScore = Math.Min(score, 100),
                RiskLevel = level,
                RiskFactors = factors.Any() ? string.Join("; ", factors) : "No material risk factors identified"
            };
        }
    }
}