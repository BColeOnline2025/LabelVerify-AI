using LabelVerify.Web.Models;

namespace LabelVerify.Web.Rules
{
    public class GovernmentWarningRule : ILabelRule
    {
        public FieldCheckResult Evaluate(
            LabelApplication application,
            string extractedText)
        {
            bool found =
                extractedText.Contains(
                    "GOVERNMENT WARNING:",
                    StringComparison.OrdinalIgnoreCase);

            return new FieldCheckResult
            {
                FieldName = "Government Warning",
                ExpectedValue = "Required",
                ActualValue = found ? "Present" : "Missing",
                IsMatch = found,
                WasSkipped = false,
                Status = found ? "Pass" : "Fail",
                ConfidenceScore = found ? 100 : 0,
                Notes = found
                    ? "Government warning detected."
                    : "Government warning missing."
            };
        }
    }
}