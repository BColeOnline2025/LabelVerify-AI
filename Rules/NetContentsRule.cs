using LabelVerify.Web.Models;
using System.Text.RegularExpressions;

namespace LabelVerify.Web.Rules
{
    public class NetContentsRule : ILabelRule
    {
        public FieldCheckResult Evaluate(LabelApplication application, string extractedText)
        {
            if (string.IsNullOrWhiteSpace(application.NetContents))
            {
                return new FieldCheckResult
                {
                    FieldName = "Net Contents",
                    ExpectedValue = "Not provided",
                    ActualValue = ExtractNetContents(extractedText) ?? "Not evaluated",
                    IsMatch = true,
                    WasSkipped = true,
                    Status = "Skipped",
                    ConfidenceScore = 100,
                    Notes = "Net contents was not provided in the application data, so this check was skipped."
                };
            }

            var expected = ExtractNetContents(application.NetContents);
            var actual = ExtractNetContents(extractedText);

            if (expected == null)
            {
                return new FieldCheckResult
                {
                    FieldName = "Net Contents",
                    ExpectedValue = application.NetContents,
                    ActualValue = actual ?? "Not detected",
                    IsMatch = false,
                    WasSkipped = false,
                    Status = "Fail",
                    ConfidenceScore = 0,
                    Notes = "Expected net contents could not be interpreted."
                };
            }

            if (actual == null)
            {
                return new FieldCheckResult
                {
                    FieldName = "Net Contents",
                    ExpectedValue = application.NetContents,
                    ActualValue = "Not detected",
                    IsMatch = false,
                    WasSkipped = false,
                    Status = "Fail",
                    ConfidenceScore = 0,
                    Notes = "Net contents was not detected on the label."
                };
            }

            var isMatch = Normalize(expected) == Normalize(actual);

            return new FieldCheckResult
            {
                FieldName = "Net Contents",
                ExpectedValue = expected,
                ActualValue = actual,
                IsMatch = isMatch,
                WasSkipped = false,
                Status = isMatch ? "Pass" : "Fail",
                ConfidenceScore = isMatch ? 100 : 0,
                Notes = isMatch
                    ? "Net contents matches the label, allowing for common formatting differences such as ML, mL, milliliters, or L."
                    : "Net contents does not match the expected application value."
            };
        }

        private static string? ExtractNetContents(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                return null;
            }

            var match = Regex.Match(
                text,
                @"(?i)\b([0-9]{1,4}(?:\.[0-9]+)?)\s*(ml|mL|milliliter|milliliters|l|liter|liters)\b");

            if (!match.Success)
            {
                return null;
            }

            var amount = match.Groups[1].Value;
            var unit = match.Groups[2].Value.ToLowerInvariant();

            return unit switch
            {
                "ml" or "milliliter" or "milliliters" => $"{amount} mL",
                "l" or "liter" or "liters" => $"{amount} L",
                _ => $"{amount} {unit}"
            };
        }

        private static string Normalize(string value)
        {
            return value
                .ToUpperInvariant()
                .Replace(" ", "")
                .Replace("ML", "ML")
                .Replace("MILLILITERS", "ML")
                .Replace("MILLILITER", "ML")
                .Replace("LITERS", "L")
                .Replace("LITER", "L");
        }
    }
}