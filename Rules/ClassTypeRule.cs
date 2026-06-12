using FuzzySharp;
using LabelVerify.Web.Models;

namespace LabelVerify.Web.Rules
{
    public class ClassTypeRule : ILabelRule
    {
        private static readonly HashSet<string> GenericClassTypes =
            new(StringComparer.OrdinalIgnoreCase)
                {
                    "Wine",
                    "Beer",
                    "Ale",
                    "Lager",
                    "Spirits",
                    "Distilled Spirits",
                    "Whiskey",
                    "Whisky",
                    "Liquor"
                };

        public FieldCheckResult Evaluate(LabelApplication application, string extractedText)
        {
            if (string.IsNullOrWhiteSpace(application.ClassType))
            {
                return new FieldCheckResult
                {
                    FieldName = "Class / Type",
                    ExpectedValue = "Not provided",
                    ActualValue = "Not evaluated",
                    IsMatch = true,
                    WasSkipped = true,
                    Status = "Skipped",
                    ConfidenceScore = 0,
                    Notes = "Class / Type was not provided, so this check was skipped."
                };
            }

            if (GenericClassTypes.Contains(application.ClassType.Trim()))
            {
                return new FieldCheckResult
                {
                    FieldName = "Class / Type",
                    ExpectedValue = application.ClassType,
                    ActualValue = "Generic class/type provided",
                    IsMatch = false,
                    WasSkipped = false,
                    Status = "Review",
                    ConfidenceScore = 60,
                    Notes = "Class / Type is too generic for automatic approval. Agent review recommended."
                };
            }

            var expected = Normalize(application.ClassType);
            var actualText = Normalize(extractedText);

            var score = Math.Max(
                Fuzz.PartialRatio(expected, actualText),
                Fuzz.TokenSetRatio(expected, actualText));

            var status = score switch
            {
                >= 90 => "Pass",
                >= 75 => "Review",
                _ => "Fail"
            };

            return new FieldCheckResult
            {
                FieldName = "Class / Type",
                ExpectedValue = application.ClassType,
                ActualValue = score >= 75
                    ? $"Likely match detected ({score}% confidence)"
                    : $"No reliable class/type match detected ({score}% confidence)",
                IsMatch = score >= 90,
                WasSkipped = false,
                Status = status,
                ConfidenceScore = score,
                Notes = status switch
                {
                    "Pass" => "Class / Type appears to match the label.",
                    "Review" => "Possible Class / Type match detected. Agent review recommended.",
                    _ => "Class / Type does not appear to match the label."
                }
            };
        }

        private static string Normalize(string value)
        {
            return value
                .ToUpperInvariant()
                .Replace("'", "")
                .Replace("’", "")
                .Replace(".", "")
                .Replace(",", "")
                .Replace("-", " ")
                .Trim();
        }
    }
}