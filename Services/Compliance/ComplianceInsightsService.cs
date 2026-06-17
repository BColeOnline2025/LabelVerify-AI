using LabelVerify.Web.Models;

namespace LabelVerify.Web.Services.Compliance
{
    public class ComplianceInsightsService
    {
        public List<string> Generate(VerificationResult result)
        {
            var insights = new List<string>();

            if (result.Checks.Any(x => x.FieldName.StartsWith("Government Warning") && x.Status == "Fail"))
            {
                insights.Add("Government Warning violation is considered a material compliance defect and supports rejection.");
            }

            if (result.Checks.Any(x => x.FieldName.StartsWith("Sulfites") && x.Status == "Fail"))
            {
                insights.Add("Sulfites declaration required for wine products was not found.");
            }

            return insights;
        }
    }
}