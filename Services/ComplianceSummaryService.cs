using System.Text;
using LabelVerify.Web.Models;

namespace LabelVerify.Web.Services
{
    public class ComplianceSummaryService
    {
        public string Generate(VerificationResult result)
        {
            var sb = new StringBuilder();

            sb.AppendLine("Compliance Review Summary");
            sb.AppendLine();

            var failed = result.Checks
                .Where(x => x.Status == "Fail")
                .ToList();

            var review = result.Checks
                .Where(x => x.Status == "Review")
                .ToList();

            var passed = result.Checks
                .Where(x => x.Status == "Pass")
                .ToList();

            sb.AppendLine($"Overall Recommendation: {result.Recommendation}");
            sb.AppendLine($"Compliance Score: {result.OverallScore}%");
            sb.AppendLine();

            if (passed.Any())
            {
                sb.AppendLine($"Successfully verified {passed.Count} fields.");
            }

            if (review.Any())
            {
                sb.AppendLine();
                sb.AppendLine("Manual review is recommended for:");

                foreach (var item in review)
                {
                    sb.AppendLine($"• {item.FieldName}");
                }
            }

            if (failed.Any())
            {
                sb.AppendLine();
                sb.AppendLine("The following discrepancies were detected:");

                foreach (var item in failed)
                {
                    sb.AppendLine($"• {item.FieldName}");
                }
            }

            return sb.ToString();
        }
    }
}