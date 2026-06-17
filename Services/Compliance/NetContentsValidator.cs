using LabelVerify.Web.Models;
using System.Text.RegularExpressions;

namespace LabelVerify.Web.Services.Compliance
{
    public class NetContentsValidator : ComplianceValidatorBase
    {
        public List<FieldCheckResult> Validate(string approved, string actual)
        {
            var checks = new List<FieldCheckResult>();

            checks.Add(ValidateEquivalentVolume(approved, actual));
            checks.Add(ValidateFormat(approved, actual));

            return checks;
        }

        private FieldCheckResult ValidateEquivalentVolume(string approved, string actual)
        {
            var approvedMl = ConvertToMilliliters(approved);
            var actualMl = ConvertToMilliliters(actual);

            if (approvedMl == actualMl)
            {
                return Pass("Net Contents Volume", approved, actual, "Equivalent container volume.");
            }

            return Fail("Net Contents Volume", approved, actual, "Net contents differs from approved COLA.");
        }

        private FieldCheckResult ValidateFormat(string approved, string actual)
        {
            if (approved.Equals(actual, StringComparison.OrdinalIgnoreCase))
            {
                return Pass("Net Contents Format", approved, actual, "Matches approved format.");
            }

            return Fail("Net Contents Format", approved, actual, "Equivalent volume but different presentation.");
        }

        private decimal ConvertToMilliliters(string value)
        {
            value = value.ToUpper();

            if (value.Contains("ML"))
            {
                return decimal.Parse(Regex.Match(value, @"\d+").Value);
            }

            if (value.Contains("CL"))
            {
                return decimal.Parse(Regex.Match(value, @"\d+").Value) * 10;
            }

            if (value.Contains("L"))
            {
                return decimal.Parse(Regex.Match(value, @"[\d\.]+").Value) * 1000;
            }

            return 0;
        }
    }
}