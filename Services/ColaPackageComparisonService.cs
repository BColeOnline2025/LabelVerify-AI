using FuzzySharp;
using LabelVerify.Web.Models;

namespace LabelVerify.Web.Services
{
    public class ColaPackageComparisonService
    {
        public VerificationResult Compare(ApprovedProductProfile approved, LabelFacts production)
        {
            var checks = new List<FieldCheckResult>
            {
                CompareFuzzy("Brand Name", approved.BrandName, production.BrandName),
                CompareFuzzy("Fanciful Name", approved.FancifulName, production.FancifulName, allowSkip: true),
                CompareFuzzy("Class / Type", approved.ClassType, production.ClassType),
                CompareExact("Alcohol Content", approved.AlcoholContent, production.AlcoholContent),
                CompareExact("Net Contents", approved.NetContents, production.NetContents),
                ComparePresence("Government Warning", approved.GovernmentWarning, production.GovernmentWarning),
                CompareFuzzy("Producer Statement", approved.ProducerStatement, production.ProducerStatement, allowSkip: true),
                CompareFuzzy("Country of Origin", approved.CountryOfOrigin, production.CountryOfOrigin, allowSkip: true),
                CompareFuzzy("Appellation", approved.Appellation, production.Appellation, allowSkip: true),
                CompareFuzzy("Varietal", approved.Varietal, production.Varietal, allowSkip: true)
            };

            var scoredChecks = checks
                .Where(x => !x.WasSkipped)
                .ToList();

            var overallScore = scoredChecks.Count != 0
                ? (int)Math.Round(scoredChecks.Average(x => x.ConfidenceScore))
                : 0;

            var hasFail = scoredChecks.Any(x => string.Equals(x.Status, "Fail", StringComparison.OrdinalIgnoreCase));

            var hasReview = scoredChecks.Any(x => string.Equals(x.Status, "Review", StringComparison.OrdinalIgnoreCase));

            var recommendation = hasFail ? "Reject" : hasReview ? "Review" : "Approve";

            return new VerificationResult
            {
                Checks = checks,
                OverallScore = overallScore,
                Recommendation = recommendation
            };
        }

        private static FieldCheckResult CompareExact(string fieldName, string approved, string production)
        {
            if (string.IsNullOrWhiteSpace(approved) && string.IsNullOrWhiteSpace(production))
            {
                return Skipped(fieldName, "Field was not detected on either source.");
            }

            if (string.IsNullOrWhiteSpace(approved) || string.IsNullOrWhiteSpace(production))
            {
                return ReviewMissing(fieldName, approved, production);
            }

            var isMatch = Normalize(approved) == Normalize(production);

            return new FieldCheckResult
            {
                FieldName = fieldName,
                ExpectedValue = approved,
                ActualValue = production,
                IsMatch = isMatch,
                WasSkipped = false,
                Status = isMatch ? "Pass" : "Fail",
                ConfidenceScore = isMatch ? 100 : 0,
                Notes = isMatch
                    ? $"{fieldName} matches the approved COLA package."
                    : $"{fieldName} does not match the approved COLA package."
            };
        }

        private static FieldCheckResult ComparePresence(string fieldName, string approved, string production)
        {
            var approvedPresent =
                !string.IsNullOrWhiteSpace(approved) &&
                !approved.Equals("Not Applicable", StringComparison.OrdinalIgnoreCase);

            var productionPresent =
                !string.IsNullOrWhiteSpace(production) &&
                !production.Equals("Not Applicable", StringComparison.OrdinalIgnoreCase);

            if (!approvedPresent && !productionPresent)
            {
                return Skipped(fieldName, "Field was not applicable or not detected on either source.");
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
                    Notes = $"{fieldName} is present on both the approved package and production label."
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
                Notes = $"{fieldName} is present on only one source."
            };
        }

        private static FieldCheckResult CompareFuzzy(string fieldName, string approved, string production, bool allowSkip = false)
        {
            if (string.IsNullOrWhiteSpace(approved) && string.IsNullOrWhiteSpace(production))
            {
                return allowSkip
                    ? Skipped(fieldName, "Optional field was not detected on either source.")
                    : ReviewMissing(fieldName, approved, production);
            }

            if (string.IsNullOrWhiteSpace(approved) || string.IsNullOrWhiteSpace(production))
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
                IsMatch = status == "Pass",
                WasSkipped = false,
                Status = status,
                ConfidenceScore = score,
                Notes = status switch
                {
                    "Pass" => $"{fieldName} appears to match the approved COLA package.",
                    "Review" => $"{fieldName} may match, but agent review is recommended.",
                    _ => $"{fieldName} does not appear to match the approved COLA package."
                }
            };
        }

        private static FieldCheckResult ReviewMissing(string fieldName, string approved, string production)
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
                Notes = $"{fieldName} was detected on only one source. Agent review recommended."
            };
        }

        private static FieldCheckResult Skipped(string fieldName, string note)
        {
            return new FieldCheckResult
            {
                FieldName = fieldName,
                ExpectedValue = "Not applicable",
                ActualValue = "Not applicable",
                IsMatch = true,
                WasSkipped = true,
                Status = "Skipped",
                ConfidenceScore = 0,
                Notes = note
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
                .Replace("/", " ")
                .Trim();
        }
    }
}