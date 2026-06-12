using System.Text.RegularExpressions;
using LabelVerify.Web.Models;

namespace LabelVerify.Web.Services
{
    public class ApplicationFieldExtractionService
    {
        public LabelApplication ExtractLabelApplication(string text)
        {
            return new LabelApplication
            {
                BrandName = ExtractOfficialFieldValue(text, "6. BRAND NAME"),
                ClassType = ExtractClassType(text),
                AlcoholContent = ExtractAlcoholContent(text),
                NetContents = ExtractNetContents(text),
                GovernmentWarning = ExtractGovernmentWarning(text)
            };
        }

        public ApprovedProductProfile ExtractApprovedProductProfile(string text)
        {
            return ExtractOfficialTtb510031(text);
        }

        public ApprovedProductProfile ExtractOfficialTtb510031(string text)
        {
            return new ApprovedProductProfile
            {
                BrandName = ExtractOfficialFieldValue(text, "6. BRAND NAME"),
                FancifulName = ExtractOfficialFieldValue(text, "7. FANCIFUL NAME"),
                ProductType = ExtractProductType(text),
                ClassType = ExtractClassType(text),
                FormulaNumber = ExtractOfficialFieldValue(text, "9. FORMULA"),
                AlcoholContent = ExtractAlcoholContent(text),
                NetContents = ExtractNetContents(text),
                GovernmentWarning = ExtractGovernmentWarning(text),
                SulfitesStatement = ExtractSulfitesStatement(text),
                ProducerStatement = ExtractProducerStatement(text),
                CountryOfOrigin = ExtractCountryOfOrigin(text)
            };
        }

        private static string ExtractOfficialFieldValue(string text, string fieldLabel)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                return string.Empty;
            }

            var pattern =
                $@"(?im)^\s*{Regex.Escape(fieldLabel)}.*?$[\r\n]+^\s*(.+?)\s*$";

            var match = Regex.Match(text, pattern);

            if (!match.Success)
            {
                return string.Empty;
            }

            var value = CleanValue(match.Groups[1].Value);

            return IsLikelyFieldHeader(value)
                ? string.Empty
                : value;
        }

        private static string ExtractProductType(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                return string.Empty;
            }

            var lines = GetCleanLines(text);

            for (var i = 0; i < lines.Count; i++)
            {
                if (!lines[i].Contains("TYPE OF PRODUCT", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                var window = lines
                    .Skip(i)
                    .Take(10)
                    .ToList();

                if (window.Any(x => x.Contains('✓') &&
                                    x.Contains("DISTILLED SPIRITS", StringComparison.OrdinalIgnoreCase)))
                {
                    return "Distilled Spirits";
                }

                if (window.Any(x => x.Contains('✓') &&
                                    x.Contains("WINE", StringComparison.OrdinalIgnoreCase)))
                {
                    return "Wine";
                }

                if (window.Any(x => x.Contains('✓') &&
                                    x.Contains("MALT BEVERAGES", StringComparison.OrdinalIgnoreCase)))
                {
                    return "Malt Beverages";
                }

                var checkIndex = window.FindIndex(x => x.Contains('✓'));

                if (checkIndex >= 0)
                {
                    for (var j = checkIndex; j < window.Count; j++)
                    {
                        if (window[j].Contains("DISTILLED SPIRITS", StringComparison.OrdinalIgnoreCase))
                        {
                            return "Distilled Spirits";
                        }

                        if (window[j].Equals("WINE", StringComparison.OrdinalIgnoreCase))
                        {
                            return "Wine";
                        }

                        if (window[j].Contains("MALT BEVERAGES", StringComparison.OrdinalIgnoreCase))
                        {
                            return "Malt Beverages";
                        }
                    }
                }
            }

            if (text.Contains("✓\r\nDISTILLED SPIRITS", StringComparison.OrdinalIgnoreCase) ||
                text.Contains("✓\nDISTILLED SPIRITS", StringComparison.OrdinalIgnoreCase))
            {
                return "Distilled Spirits";
            }

            if (text.Contains("DISTILLED SPIRITS", StringComparison.OrdinalIgnoreCase))
            {
                return "Distilled Spirits";
            }

            if (text.Contains("MALT BEVERAGES", StringComparison.OrdinalIgnoreCase))
            {
                return "Malt Beverages";
            }

            if (text.Contains("WINE", StringComparison.OrdinalIgnoreCase))
            {
                return "Wine";
            }

            return string.Empty;
        }

        private static string ExtractClassType(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                return string.Empty;
            }

            var knownTypes = new[]
            {
            "Kentucky Straight Bourbon Whiskey",
            "Straight Bourbon Whiskey",
            "Bourbon Whiskey",
            "Straight Rye Whiskey",
            "Rye Whiskey",
            "Tennessee Whiskey",
            "Vodka",
            "Gin",
            "Rum",
            "Tequila",
            "Cabernet Sauvignon",
            "Merlot",
            "Pinot Noir",
            "Chardonnay",
            "Sauvignon Blanc",
            "Red Wine",
            "White Wine",
            "Rose Wine",
            "American IPA",
            "IPA",
            "Lager",
            "Pilsner",
            "Stout"
        };

            foreach (var type in knownTypes)
            {
                if (text.Contains(type, StringComparison.OrdinalIgnoreCase))
                {
                    return type;
                }
            }

            return string.Empty;
        }

        private static string ExtractAlcoholContent(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                return string.Empty;
            }

            var match = Regex.Match(
                text,
                @"(?i)\b([0-9]{1,2}(?:\.[0-9]+)?)\s*%\s*(?:alc\.?/vol\.?|abv|by\s+vol\.?|alcohol\s+by\s+volume)?");

            return match.Success
                ? $"{match.Groups[1].Value}%"
                : string.Empty;
        }

        private static string ExtractNetContents(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                return string.Empty;
            }

            var match = Regex.Match(
                text,
                @"(?i)\b([0-9]{1,4}(?:\.[0-9]+)?)\s*(ml|mL|ML|milliliter|milliliters|l|L|liter|liters)\b");

            if (!match.Success)
            {
                return string.Empty;
            }

            var amount = match.Groups[1].Value;
            var unit = NormalizeUnit(match.Groups[2].Value);

            return $"{amount} {unit}";
        }

        private static string ExtractGovernmentWarning(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                return string.Empty;
            }

            return text.Contains("GOVERNMENT WARNING", StringComparison.OrdinalIgnoreCase)
                ? "Present"
                : string.Empty;
        }

        private static string ExtractSulfitesStatement(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                return string.Empty;
            }

            if (text.Contains("CONTAINS SULFITES", StringComparison.OrdinalIgnoreCase) ||
                text.Contains("CONTAINS SULPHITES", StringComparison.OrdinalIgnoreCase))
            {
                return "Contains Sulfites";
            }

            if (text.Contains("DISTILLED SPIRITS", StringComparison.OrdinalIgnoreCase) ||
                text.Contains("WHISKEY", StringComparison.OrdinalIgnoreCase) ||
                text.Contains("VODKA", StringComparison.OrdinalIgnoreCase) ||
                text.Contains("GIN", StringComparison.OrdinalIgnoreCase) ||
                text.Contains("RUM", StringComparison.OrdinalIgnoreCase) ||
                text.Contains("TEQUILA", StringComparison.OrdinalIgnoreCase))
            {
                return "Not Applicable";
            }

            return string.Empty;
        }

        private static string ExtractProducerStatement(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                return string.Empty;
            }

            var match = Regex.Match(
                text,
                @"(?im)^\s*(distilled and bottled by|bottled by|produced by|imported by|vinted by|cellared by)\s*$[\r\n]+^\s*(.+?)\s*$[\r\n]+^\s*(.+?)\s*$",
                RegexOptions.IgnoreCase);

            if (match.Success)
            {
                return CleanValue(
                    $"{match.Groups[1].Value} {match.Groups[2].Value} {match.Groups[3].Value}");
            }

            var singleLineMatch = Regex.Match(
                text,
                @"(?im)^\s*(distilled and bottled by|bottled by|produced by|imported by|vinted by|cellared by)\s+(.+)$",
                RegexOptions.IgnoreCase);

            return singleLineMatch.Success
                ? CleanValue(singleLineMatch.Value)
                : string.Empty;
        }

        private static string ExtractCountryOfOrigin(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                return string.Empty;
            }

            var match = Regex.Match(
                text,
                @"(?im)^\s*(produced in the usa|product of usa|product of the usa|produced in usa|imported from\s+[a-zA-Z\s]+)\s*$",
                RegexOptions.IgnoreCase);

            return match.Success
                ? CleanValue(match.Groups[1].Value)
                : string.Empty;
        }

        private static string NormalizeUnit(string value)
        {
            var unit = value.ToLowerInvariant();

            return unit switch
            {
                "ml" or "milliliter" or "milliliters" => "mL",
                "l" or "liter" or "liters" => "L",
                _ => value
            };
        }

        private static List<string> GetCleanLines(string text)
        {
            return [.. text
                .Split(
                    ["\r\n", "\n"],
                    StringSplitOptions.RemoveEmptyEntries)
                .Select(x => x.Trim())
                .Where(x => !string.IsNullOrWhiteSpace(x))];
        }

        private static bool IsLikelyFieldHeader(string value)
        {
            var headers = new[]
            {
            "required",
            "formula",
            "fanciful name",
            "brand name",
            "grape varietal",
            "wine appellation",
            "type of application",
            "phone number",
            "email address"
        };

            return headers.Any(x =>
                value.Contains(x, StringComparison.OrdinalIgnoreCase));
        }

        private static string CleanValue(string value)
        {
            return value
                .Trim()
                .Trim(':')
                .Trim('-')
                .Trim();
        }
    }
}