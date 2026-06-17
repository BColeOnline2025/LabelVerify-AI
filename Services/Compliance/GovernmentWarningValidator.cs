using System.Text.RegularExpressions;
using LabelVerify.Web.Models;

namespace LabelVerify.Web.Services.Compliance
{
    public class GovernmentWarningValidator : ComplianceValidatorBase
    {
        private const string RequiredHeader = "GOVERNMENT WARNING:";
        private const string RequiredWarning = "GOVERNMENT WARNING: (1) According to the Surgeon General, women should not drink alcoholic beverages during pregnancy because of the risk of birth defects. (2) Consumption of alcoholic beverages impairs your ability to drive a car or operate machinery, and may cause health problems.";

        public List<FieldCheckResult> Validate(string? actualWarningText)
        {
            var actual = actualWarningText ?? string.Empty;

            return
            [
                ValidateHeader(actual),
                ValidateExactText(actual),
                ValidateCompleteness(actual)
            ];
        }

        private FieldCheckResult ValidateHeader(string actual)
        {
            var headerMatch = Regex.Match(actual, @"\bgovernment warning\s*:", RegexOptions.IgnoreCase);

            if (!headerMatch.Success)
            {
                return Fail("Government Warning Header Format", RequiredHeader, "Header not found",
                    "Required header GOVERNMENT WARNING: was not detected.");
            }

            var actualHeader = headerMatch.Value.Trim();

            if (actualHeader != RequiredHeader)
            {
                return Fail("Government Warning Header Format", RequiredHeader, actualHeader,
                    "Header must be exactly GOVERNMENT WARNING: in all capital letters.");
            }

            return Pass("Government Warning Header Format", RequiredHeader,actualHeader,
                "Required header appears exactly as required.");
        }

        private FieldCheckResult ValidateExactText(string actual)
        {
            var normalizedActual = Normalize(actual);
            var normalizedRequired = Normalize(RequiredWarning);

            if (normalizedActual == normalizedRequired)
            {
                return Pass("Government Warning Exact Text", RequiredWarning, actual, "Government warning text matches required statement word-for-word.");
            }

            return Fail("Government Warning Exact Text", RequiredWarning, actual, "Government warning must match the required statement word-for-word.");
        }

        private FieldCheckResult ValidateCompleteness(string actual)
        {
            var normalized = Normalize(actual);

            var requiredSegments = new[]
            {
                "According to the Surgeon General",
                "women should not drink alcoholic beverages during pregnancy",
                "because of the risk of birth defects",
                "Consumption of alcoholic beverages impairs your ability to drive a car or operate machinery",
                "and may cause health problems"
            };

            var missing = requiredSegments
                .Where(x => !normalized.Contains(Normalize(x)))
                .ToList();

            if (!missing.Any())
            {
                return Pass("Government Warning Completeness", RequiredWarning, actual,
                    "All required warning components were detected.");
            }

            return Fail("Government Warning Completeness", RequiredWarning, actual,
                $"Missing required warning component(s): {string.Join("; ", missing)}");
        }

        private static string Normalize(string value)
        {
            value = value ?? string.Empty;

            value = value.Replace("\r", " ");
            value = value.Replace("\n", " ");
            value = Regex.Replace(value, @"\s+", " ");

            value = value.Replace("WARNING :", "WARNING:");
            value = value.Replace("Warning :", "Warning:");

            return value.Trim();
        }
    }
}