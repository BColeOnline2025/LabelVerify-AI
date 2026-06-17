using LabelVerify.Web.Models;
using System.Text.RegularExpressions;

namespace LabelVerify.Web.Services.Compliance
{
    public class AlcoholContentValidator : ComplianceValidatorBase
    {
        public List<FieldCheckResult> Validate(string approved, string actual)
        {
            var checks = new List<FieldCheckResult>();

            checks.Add(ValidateExactPercentage(approved, actual));
            checks.Add(ValidateFormat(approved, actual));

            return checks;
        }

        private FieldCheckResult ValidateExactPercentage(string approved, string actual)
        {
            var expectedPercent = ExtractPercent(approved);
            var actualPercent = ExtractPercent(actual);

            if (expectedPercent == actualPercent)
            {
                return Pass("Alcohol Content Percentage", approved, actual, "Alcohol percentage matches approved COLA.");
            }

            return Fail("Alcohol Content Percentage", approved, actual, "Alcohol percentage differs from approved COLA.");
        }

        private FieldCheckResult ValidateFormat(string approved, string actual)
        {
            if (actual.Contains("ALC/VOL"))
            {
                return Pass("Alcohol Content Format", approved, actual, "Contains ALC/VOL statement.");
            }

            return Fail("Alcohol Content Format", approved, actual, "Expected ALC/VOL statement.");
        }

        private decimal ExtractPercent(string value)
        {
            var m = Regex.Match(value ?? "", @"(\d+(\.\d+)?)");

            return m.Success ? decimal.Parse(m.Groups[1].Value) : 0;
        }
    }
}