using LabelVerify.Web.Models;

namespace LabelVerify.Web.Rules
{
    public class NetContentsRule : ILabelRule
    {
        public FieldCheckResult Evaluate(
            LabelApplication application,
            string extractedText)
        {
            // Skip check if field not provided
            if (string.IsNullOrWhiteSpace(application.NetContents))
            {
                return new FieldCheckResult
                {
                    FieldName = "Net Contents",
                    ExpectedValue = "Not provided",
                    ActualValue = "Not evaluated",
                    IsMatch = true,
                    Status = "Skipped",
                    WasSkipped = true,
                    ConfidenceScore = 100,
                    Notes = "Net contents was not provided, so this check was skipped."
                };
            }

            bool found =
                extractedText.Contains(
                    application.NetContents,
                    StringComparison.OrdinalIgnoreCase);

            return new FieldCheckResult
            {
                FieldName = "Net Contents",
                ExpectedValue = application.NetContents,
                ActualValue = found
                    ? application.NetContents
                    : "Not Found",
                IsMatch = found,
                ConfidenceScore = found ? 100 : 0,
                Notes = found
                    ? "Net contents verified."
                    : "Net contents mismatch."
            };
        }
    }
}