using LabelVerify.Web.Models;

namespace LabelVerify.Web.Services.Compliance
{
    public class RecommendationEngine
    {
        public string Apply(string recommendation, List<FieldCheckResult> checks)
        {
            var criticalFailures = new[]
            {
                "Government Warning",
                "Alcohol Content",
                "Net Contents",
                "Sulfites"
            };

            if (checks.Any(c => criticalFailures.Any(cf => c.FieldName.StartsWith(cf)) && c.Status == "Fail"))
            {
                return "Fail";
            }

            return recommendation;
        }
    }
}