using LabelVerify.Web.Models;

namespace LabelVerify.Web.Services.Compliance
{
    public class SulfitesValidator : ComplianceValidatorBase
    {
        private const string RequiredText = "CONTAINS SULFITES";

        public List<FieldCheckResult> Validate(
            ApprovedProductProfile approved,
            LabelFacts production)
        {
            var checks = new List<FieldCheckResult>();

            bool isWine =
                approved.ProductType.Contains("wine", StringComparison.OrdinalIgnoreCase) ||
                approved.ClassType.Contains("wine", StringComparison.OrdinalIgnoreCase);

            if (!isWine)
            {
                checks.Add(Skipped(
                    "Sulfites Statement",
                    "Sulfites declaration is only required for wine products."));

                return checks;
            }

            bool approvedPresent =
                !string.IsNullOrWhiteSpace(approved.SulfitesStatement);

            bool productionPresent =
                !string.IsNullOrWhiteSpace(production.SulfitesStatement);

            if (approvedPresent && productionPresent)
            {
                checks.Add(Pass(
                    "Sulfites Statement",
                    approved.SulfitesStatement,
                    production.SulfitesStatement,
                    "Required sulfites declaration detected."));
            }
            else if (approvedPresent && !productionPresent)
            {
                checks.Add(Fail(
                    "Sulfites Statement",
                    approved.SulfitesStatement,
                    production.SulfitesStatement,
                    "Required sulfites declaration missing from production label."));
            }
            else if (!approvedPresent && productionPresent)
            {
                checks.Add(Review(
                    "Sulfites Statement",
                    approved.SulfitesStatement,
                    production.SulfitesStatement,
                    "Sulfites declaration appears on production label but not on approved COLA."));
            }
            else
            {
                checks.Add(Skipped(
                    "Sulfites Statement",
                    "No sulfites declaration required."));
            }

            return checks;
        }

        private static bool IsWine(ApprovedProductProfile approved)
        {
            return approved.ProductType.Contains("wine", StringComparison.OrdinalIgnoreCase) ||
                   approved.ClassType.Contains("wine", StringComparison.OrdinalIgnoreCase);
        }

        private FieldCheckResult ValidatePresence(string actual)
        {
            if (!string.IsNullOrWhiteSpace(actual))
            {
                return Pass("Sulfites Presence", RequiredText, actual, "Sulfites statement found.");
            }

            return Fail("Sulfites Presence", RequiredText, "Not found", "Sulfites statement not found on production label.");
        }

        private FieldCheckResult ValidateExactText(string actual)
        {
            if (actual.Trim() == RequiredText)
            {
                return Pass("Sulfites Exact Text", RequiredText, actual, "Statement matches required text.");
            }

            return Fail("Sulfites Exact Text", RequiredText, actual, "Statement must be exactly CONTAINS SULFITES.");
        }

        protected FieldCheckResult Review(
            string fieldName,
            string approved,
            string actual,
            string note)
        {
            return new FieldCheckResult
            {
                FieldName = fieldName,
                ExpectedValue = approved ?? string.Empty,
                ActualValue = actual ?? string.Empty,
                IsMatch = false,
                WasSkipped = false,
                Status = "Review",
                ConfidenceScore = 50,
                Notes = note
            };
        }

        protected FieldCheckResult Skipped(
            string fieldName,
            string approved,
            string actual,
            string note)
        {
            return new FieldCheckResult
            {
                FieldName = fieldName,
                ExpectedValue = approved ?? string.Empty,
                ActualValue = actual ?? string.Empty,
                IsMatch = true,
                WasSkipped = true,
                Status = "Skipped",
                ConfidenceScore = 0,
                Notes = note
            };
        }
    }
}