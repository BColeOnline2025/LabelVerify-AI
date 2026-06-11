using LabelVerify.Web.Models;
using System.Text.RegularExpressions;

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
                    WasSkipped = true,
                    Status = "Skipped",
                    ConfidenceScore = 100,
                    Notes = "Alcohol content was not provided, so this check was skipped."
                };
            }

            var expectedPercent = ExtractAlcoholPercent(application.AlcoholContent);
            var actualPercent = ExtractAlcoholPercent(extractedText);

            var found = !string.IsNullOrWhiteSpace(expectedPercent)
                        && !string.IsNullOrWhiteSpace(actualPercent)
                        && expectedPercent == actualPercent;

            return new FieldCheckResult
            {
                FieldName = "Alcohol Content",
                ExpectedValue = application.AlcoholContent,
                ActualValue = actualPercent ?? "Not Found",
                IsMatch = found,
                WasSkipped = false,
                Status = found ? "Pass" : "Fail",
                ConfidenceScore = found ? 100 : 0,
                Notes = found
                    ? "Alcohol percentage matches label text."
                    : "Alcohol percentage was not found or does not match."
            };
        }

        private static string? ExtractAlcoholPercent(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return null;
            }

            var match = Regex.Match(
                value,
                @"\b\d{1,2}(\.\d+)?\s*%",
                RegexOptions.IgnoreCase);

            return match.Success
                ? match.Value.Replace(" ", "")
                : null;
        }
    }
}