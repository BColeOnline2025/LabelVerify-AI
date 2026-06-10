using LabelVerify.Web.Models;

namespace LabelVerify.Web.Rules
{
    public class AlcoholContentRule : ILabelRule
    {
        public FieldCheckResult Evaluate(
            LabelApplication application,
            string extractedText)
        {
            if (string.IsNullOrWhiteSpace(application.AlcoholContent))
            {
                return new FieldCheckResult
                {
                    FieldName = "Alcohol Content",
                    ExpectedValue = "Not provided",
                    ActualValue = "Not evaluated",
                    IsMatch = true,
                    Status = "Skipped",
                    WasSkipped = true,
                    ConfidenceScore = 100,
                    Notes = "Alcohol content was not provided, so this check was skipped."
                };
            }

            bool found =
                extractedText.Contains(
                    application.AlcoholContent,
                    StringComparison.OrdinalIgnoreCase);

            return new FieldCheckResult
            {
                FieldName = "Alcohol Content",
                ExpectedValue = application.AlcoholContent,
                ActualValue = found ? application.AlcoholContent : "Not Found",
                IsMatch = found,
                ConfidenceScore = found ? 100 : 0,
                Notes = found
                    ? "Alcohol content matches."
                    : "Alcohol content mismatch."
            };
        }
    }
}