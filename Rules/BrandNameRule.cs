using FuzzySharp;
using LabelVerify.Web.Models;

namespace LabelVerify.Web.Rules
{
    public class BrandNameRule : ILabelRule
    {
        public FieldCheckResult Evaluate(LabelApplication application, string extractedText)
        {
            if (string.IsNullOrWhiteSpace(application.BrandName))
            {
                return new FieldCheckResult
                {
                    FieldName = "Brand Name",
                    ExpectedValue = "Not provided",
                    ActualValue = "Not evaluated",
                    IsMatch = false,
                    WasSkipped = false,
                    ConfidenceScore = 0,
                    Notes = "Brand name is required for label verification."
                };
            }

            var expected = Normalize(application.BrandName);
            var actualText = Normalize(extractedText);

            var score = Math.Max(Fuzz.PartialRatio(expected, actualText), Fuzz.TokenSetRatio(expected, actualText));

            var status = score switch
            {
                >= 95 => "Pass",
                >= 80 => "Review",
                _ => "Fail"
            };
            
            var isMatch = score >= 80;

            return new FieldCheckResult
            {
                FieldName = "Brand Name",
                ExpectedValue = application.BrandName,
                ActualValue = score >= 80
                    ? $"Match Detected ({score}% confidence)"
                    : $"No Reliable Match ({score}% confidence)",
                Status = status,
                IsMatch = isMatch,
                WasSkipped = false,
                ConfidenceScore = score,
                Notes = score switch
                {
                    >= 95 => "Strong brand match detected.",
                    >= 80 => "Likely brand match detected. Extra business terms or formatting differences may exist.",
                    >= 70 => "Possible brand match detected. Agent review recommended.",
                    _ => "Brand name was not confidently found on the label."
                }
            };
        }

        private static string Normalize(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return string.Empty;
            }

            return value
                .ToUpperInvariant()
                .Replace("'", "")
                .Replace("’", "")
                .Replace(".", "")
                .Replace(",", "")
                .Replace("-", " ")
                .Replace("&", "AND")
                .Trim();
        }
    }
}