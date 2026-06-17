using LabelVerify.Web.Models;
using System.Globalization;
using System.Text.RegularExpressions;

namespace LabelVerify.Web.Services.Compliance
{
    public class AlcoholContentValidator : ComplianceValidatorBase
    {
        public List<FieldCheckResult> Validate(ApprovedProductProfile approved, LabelFacts production)
        {
            return
            [
                ValidateAlcoholPercentage(approved, production),
                ValidateAlcoholContentFormat(approved, production)
            ];
        }

        private FieldCheckResult ValidateAlcoholPercentage(
            ApprovedProductProfile approved,
            LabelFacts production)
        {
            var approvedPercent = ExtractPercent(approved.AlcoholContent);
            var productionPercent = ExtractPercent(production.AlcoholContent);

            if (!approvedPercent.HasValue)
            {
                return Fail(
                    "Alcohol Content Percentage",
                    approved.AlcoholContent,
                    production.AlcoholContent,
                    "Alcohol percentage not found on approved COLA.");
            }

            if (!productionPercent.HasValue)
            {
                return Fail(
                    "Alcohol Content Percentage",
                    approved.AlcoholContent,
                    production.AlcoholContent,
                    "Alcohol percentage not found on production label.");
            }

            if (approvedPercent.Value == productionPercent.Value)
            {
                return Pass(
                    "Alcohol Content Percentage",
                    approved.AlcoholContent,
                    production.AlcoholContent,
                    $"Alcohol percentage matches approved COLA: {productionPercent:0.##}%.");
            }

            return Fail(
                "Alcohol Content Percentage",
                approved.AlcoholContent,
                production.AlcoholContent,
                $"Alcohol percentage differs. Approved COLA shows {approvedPercent:0.##}%, production label shows {productionPercent:0.##}%.");
        }

        private FieldCheckResult ValidateAlcoholContentFormat(
            ApprovedProductProfile approved,
            LabelFacts production)
        {
            var approvedStatement = approved.AlcoholContentStatement;
            var productionStatement = production.AlcoholContentStatement;

            var approvedFormat = ExtractAlcoholFormat(approvedStatement);
            var productionFormat = ExtractAlcoholFormat(productionStatement);

            if (approvedFormat is null)
            {
                return Fail(
                    "Alcohol Content Format",
                    approvedStatement,
                    productionStatement,
                    "Alcohol content format not found on approved COLA.");
            }

            if (productionFormat is null)
            {
                return Fail(
                    "Alcohol Content Format",
                    approvedFormat,
                    string.IsNullOrWhiteSpace(productionStatement) ? "Not detected" : productionStatement,
                    $"Alcohol content format not found on production label. Production AlcoholContent was '{production.AlcoholContent}'.");
            }

            if (approvedFormat == productionFormat)
            {
                return Pass(
                    "Alcohol Content Format",
                    approvedFormat,
                    productionFormat,
                    $"Alcohol content format matches: {productionFormat}.");
            }

            return Fail(
                "Alcohol Content Format",
                approvedFormat,
                productionFormat,
                $"Expected '{approvedFormat}' but found '{productionFormat}'.");
        }

        private static decimal? ExtractPercent(string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return null;

            var match = Regex.Match(
                value,
                @"(?<percent>\d{1,3}(?:\.\d+)?)\s*%",
                RegexOptions.IgnoreCase);

            if (!match.Success)
                return null;

            return decimal.TryParse(
                match.Groups["percent"].Value,
                NumberStyles.AllowDecimalPoint,
                CultureInfo.InvariantCulture,
                out var percent)
                    ? percent
                    : null;
        }

        private static string? ExtractAlcoholFormat(string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return null;

            var normalized = Regex.Replace(value.ToUpperInvariant(), @"\s+", " ").Trim();

            if (Regex.IsMatch(normalized, @"ALC\.?\s*/\s*VOL\.?"))
                return "ALC./VOL.";

            if (Regex.IsMatch(normalized, @"ALC\.?\s+VOL\.?"))
                return "ALC./VOL.";

            if (Regex.IsMatch(normalized, @"ALC\.?\s+BY\s+VOL\.?"))
                return "ALC. BY VOL.";

            if (normalized.Contains("ALCOHOL") && normalized.Contains("BY VOLUME"))
                return "ALCOHOL BY VOLUME";

            return null;
        }
    }
}