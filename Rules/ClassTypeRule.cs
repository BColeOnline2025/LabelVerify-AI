using LabelVerify.Web.Models;

namespace LabelVerify.Web.Rules
{
    public class ClassTypeRule : ILabelRule
    {
        public FieldCheckResult Evaluate(
            LabelApplication application,
            string extractedText)
        {
            // Skip check if field not provided
            if (string.IsNullOrWhiteSpace(application.ClassType))
            {
                return new FieldCheckResult
                {
                    FieldName = "Class / Type",
                    ExpectedValue = "Not provided",
                    ActualValue = "Not evaluated",
                    IsMatch = true,
                    Status = "Skipped",
                    WasSkipped = true,
                    ConfidenceScore = 100,
                    Notes = "Class / Type was not provided, so this check was skipped."
                };
            }

            bool found =
                extractedText.Contains(
                    application.ClassType,
                    StringComparison.OrdinalIgnoreCase);

            return new FieldCheckResult
            {
                FieldName = "Class / Type",
                ExpectedValue = application.ClassType,
                ActualValue = found
                    ? application.ClassType
                    : "Not Found",
                IsMatch = found,
                ConfidenceScore = found ? 100 : 0,
                Notes = found
                    ? "Class / Type verified."
                    : "Class / Type mismatch."
            };
        }
    }
}