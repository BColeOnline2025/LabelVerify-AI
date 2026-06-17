using LabelVerify.Web.Models;

namespace LabelVerify.Web.Services.Compliance
{
    public class SulfitesValidator : ComplianceValidatorBase
    {
        private const string RequiredText = "CONTAINS SULFITES";

        public List<FieldCheckResult> Validate(string approved, string actual)
        {
            actual ??= "";
            approved ??= "";

            return
            [
                ValidatePresence(actual),
                ValidateExactText(actual)
            ];
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
    }
}