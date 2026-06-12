using FuzzySharp;
using LabelVerify.Web.Models;

namespace LabelVerify.Web.Services
{
    public class LabelComparisonService
    {
        public VerificationResult Compare(LabelFacts approved, LabelFacts production)
        {
            var checks = new List<FieldCheckResult>
        {
            CompareFuzzy("Brand Name", approved.BrandName, production.BrandName, 85),
            CompareFuzzy("Class / Type", approved.ClassType, production.ClassType, 85),
            CompareExact("Alcohol Content", approved.AlcoholContent, production.AlcoholContent),
            CompareExact("Net Contents", approved.NetContents, production.NetContents),
            ComparePresence("Government Warning", approved.GovernmentWarning, production.GovernmentWarning),
            CompareFuzzy("Appellation", approved.Appellation, production.Appellation, 85, allowSkip: true),
            CompareFuzzy("Varietal", approved.Varietal, production.Varietal, 85, allowSkip: true),
            CompareFuzzy("Producer Statement", approved.ProducerStatement, production.ProducerStatement, 80, allowSkip: true),
            CompareFuzzy("Country of Origin", approved.CountryOfOrigin, production.CountryOfOrigin, 85, allowSkip: true)
        };

            var scoredChecks = checks
                .Where(x => !x.WasSkipped)
                .ToList();

            var overallScore = scoredChecks.Any()
                ? (int)Math.Round(scoredChecks.Average(x => x.ConfidenceScore))
                : 0;

            var hasFail = scoredChecks.Any(x => x.Status == "Fail");
            var hasReview = scoredChecks.Any(x => x.Status == "Review");

            var recommendation = hasFail
                ? "Reject"
                : hasReview
                    ? "Review"
                    : "Approve";

            return new VerificationResult
            {
                Checks = checks,
                OverallScore = overallScore,
                Recommendation = recommendation
            };
        }

        private static FieldCheckResult CompareExact(
            string fieldName,
            string approved,
            string production)
        {
            if (string.IsNullOrWhiteSpace(approved) &&
                string.IsNullOrWhiteSpace(production))
            {
                return Skipped(fieldName, "Field not detected on either label.");
            }

            if (string.IsNullOrWhiteSpace(approved) ||
                string.IsNullOrWhiteSpace(production))
            {
                return new FieldCheckResult
                {
                    FieldName = fieldName,
                    ExpectedValue = string.IsNullOrWhiteSpace(approved) ? "Not detected" : approved,
                    ActualValue = string.IsNullOrWhiteSpace(production) ? "Not detected" : production,
                    IsMatch = false,
                    WasSkipped = false,
                    Status = "Review",
                    ConfidenceScore = 50,
                    Notes = $"{fieldName} was detected on only one label. Agent review recommended."
                };
            }

            var match = Normalize(approved) == Normalize(production);

            return new FieldCheckResult
            {
                FieldName = fieldName,
                ExpectedValue = approved,
                ActualValue = production,
                IsMatch = match,
                WasSkipped = false,
                Status = match ? "Pass" : "Fail",
                ConfidenceScore = match ? 100 : 0,
                Notes = match
                    ? $"{fieldName} matches between approved and production labels."
                    : $"{fieldName} does not match between approved and production labels."
            };
        }

        private static FieldCheckResult ComparePresence(
            string fieldName,
            string approved,
            string production)
        {
            var approvedPresent = !string.IsNullOrWhiteSpace(approved);
            var productionPresent = !string.IsNullOrWhiteSpace(production);

            if (!approvedPresent && !productionPresent)
            {
                return new FieldCheckResult
                {
                    FieldName = fieldName,
                    ExpectedValue = "Not detected",
                    ActualValue = "Not detected",
                    IsMatch = false,
                    WasSkipped = false,
                    Status = "Fail",
                    ConfidenceScore = 0,
                    Notes = $"{fieldName} was not detected on either label."
                };
            }

            if (approvedPresent && productionPresent)
            {
                return new FieldCheckResult
                {
                    FieldName = fieldName,
                    ExpectedValue = "Present",
                    ActualValue = "Present",
                    IsMatch = true,
                    WasSkipped = false,
                    Status = "Pass",
                    ConfidenceScore = 100,
                    Notes = $"{fieldName} is present on both labels."
                };
            }

            return new FieldCheckResult
            {
                FieldName = fieldName,
                ExpectedValue = approvedPresent ? "Present" : "Missing",
                ActualValue = productionPresent ? "Present" : "Missing",
                IsMatch = false,
                WasSkipped = false,
                Status = "Fail",
                ConfidenceScore = 0,
                Notes = $"{fieldName} is present on only one label."
            };
        }

        private static FieldCheckResult CompareFuzzy(
            string fieldName,
            string approved,
            string production,
            int passThreshold,
            bool allowSkip = false)
        {
            if (string.IsNullOrWhiteSpace(approved) &&
                string.IsNullOrWhiteSpace(production))
            {
                return allowSkip
                    ? Skipped(fieldName, "Field not detected on either label.")
                    : ReviewMissing(fieldName, approved, production);
            }

            if (string.IsNullOrWhiteSpace(approved) ||
                string.IsNullOrWhiteSpace(production))
            {
                return ReviewMissing(fieldName, approved, production);
            }

            var score = Math.Max(
                Fuzz.PartialRatio(Normalize(approved), Normalize(production)),
                Fuzz.TokenSetRatio(Normalize(approved), Normalize(production)));

            var status = score switch
            {
                >= 90 => "Pass",
                >= 75 => "Review",
                _ => "Fail"
            };

            return new FieldCheckResult
            {
                FieldName = fieldName,
                ExpectedValue = approved,
                ActualValue = production,
                IsMatch = score >= passThreshold,
                WasSkipped = false,
                Status = status,
                ConfidenceScore = score,
                Notes = status switch
                {
                    "Pass" => $"{fieldName} appears to match between approved and production labels.",
                    "Review" => $"{fieldName} may match, but agent review is recommended.",
                    _ => $"{fieldName} does not appear to match."
                }
            };
        }

        private static FieldCheckResult Skipped(string fieldName, string note)
        {
            return new FieldCheckResult
            {
                FieldName = fieldName,
                ExpectedValue = "Not detected",
                ActualValue = "Not detected",
                IsMatch = true,
                WasSkipped = true,
                Status = "Skipped",
                ConfidenceScore = 0,
                Notes = note
            };
        }

        private static FieldCheckResult ReviewMissing(
            string fieldName,
            string approved,
            string production)
        {
            return new FieldCheckResult
            {
                FieldName = fieldName,
                ExpectedValue = string.IsNullOrWhiteSpace(approved) ? "Not detected" : approved,
                ActualValue = string.IsNullOrWhiteSpace(production) ? "Not detected" : production,
                IsMatch = false,
                WasSkipped = false,
                Status = "Review",
                ConfidenceScore = 50,
                Notes = $"{fieldName} was detected on only one label. Agent review recommended."
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