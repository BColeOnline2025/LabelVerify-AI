using System.Text.RegularExpressions;
using LabelVerify.Web.Models;

namespace LabelVerify.Web.Services
{
    public class LabelFactExtractionService
    {
        public LabelFacts Extract(string text)
        {
            var alcoholContentStatement = ExtractAlcoholContentStatement(text);
            
            return new LabelFacts
            {
                BrandName = ExtractBrandCandidate(text),
                ClassType = ExtractClassType(text),
                AlcoholContent = ExtractAlcoholPercent(alcoholContentStatement) ?? string.Empty,
                AlcoholContentStatement = alcoholContentStatement,
                NetContents = ExtractNetContents(text),
                GovernmentWarning = ExtractGovernmentWarning(text),
                Appellation = ExtractAppellation(text),
                Varietal = ExtractVarietal(text),
                ProducerStatement = ExtractProducerStatement(text),
                FancifulName = ExtractFancifulName(text),
                SulfitesStatement = ExtractSulfitesStatement(text),
                CountryOfOrigin = ExtractCountryOfOrigin(text)
            };
        }

        private string ExtractAlcoholContentStatement(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return string.Empty;

            var normalized = Regex.Replace(text, @"\s+", " ").Trim();

            // First try normal same-line formats
            var directMatch = Regex.Match(
                normalized,
                @"(?:ALCOHOL\s+\d{1,3}(?:\.\d+)?\s*%\s+BY\s+VOLUME)|
          (?:\d{1,3}(?:\.\d+)?\s*%\s*ALC\.?\s*[/\s]?\s*VOL\.?)|
          (?:\d{1,3}(?:\.\d+)?\s*%\s*ALC\.?\s+BY\s+VOL\.?)|
          (?:\d{1,3}(?:\.\d+)?\s*%\s*ALCOHOL\s+BY\s+VOLUME)",
                RegexOptions.IgnoreCase | RegexOptions.IgnorePatternWhitespace);

            if (directMatch.Success)
                return directMatch.Value.Trim();

            // Fallback: find percent anywhere near ALC/VOL wording
            var percentMatch = Regex.Match(
                normalized,
                @"\d{1,3}(?:\.\d+)?\s*%",
                RegexOptions.IgnoreCase);

            var formatMatch = Regex.Match(
                normalized,
                @"ALC\.?\s*[/\s]?\s*VOL\.?|ALC\.?\s+BY\s+VOL\.?|ALCOHOL\s+BY\s+VOLUME",
                RegexOptions.IgnoreCase);

            if (percentMatch.Success && formatMatch.Success)
            {
                return $"{percentMatch.Value.Trim()} {formatMatch.Value.Trim()}";
            }

            return string.Empty;
        }

        private string ExtractAlcoholPercent(string? statement)
        {
            if (string.IsNullOrWhiteSpace(statement))
                return null;

            var match = Regex.Match(statement, @"\d{1,3}(?:\.\d+)?\s*%");

            return match.Success ? match.Value.Trim() : null;
        }

        private static string ExtractBrandCandidate(string text)
        {
            var lines = CleanLines(text);

            var skipWords = new[]
            {
                "government warning",
                "contains sulfites",
                "alc",
                "alcohol",
                "750",
                "ml",
                "bottled",
                "produced",
                "imported",
                "warning"
            };

            var candidate = lines
                .Where(line => line.Length >= 3)
                .Where(line => !skipWords.Any(skip =>
                    line.Contains(skip, StringComparison.OrdinalIgnoreCase)))
                .OrderByDescending(line => line.Length)
                .FirstOrDefault();

            return candidate ?? string.Empty;
        }

        private static string ExtractSulfitesStatement(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                return string.Empty;
            }

            var match = Regex.Match(text, @"CONTAINS\s+SULFITES", RegexOptions.IgnoreCase);

            return match.Success ? match.Value.Trim() : string.Empty;
        }

        private static string ExtractClassType(string text)
        {
            var knownTypes = new[]
            {
                "Kentucky Straight Bourbon Whiskey",
                "Bourbon Whiskey",
                "Straight Bourbon Whiskey",
                "Tennessee Whiskey",
                "Rye Whiskey",
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
            var match = Regex.Match(
                text,
                @"(?i)([0-9]{1,2}(?:\.[0-9]+)?)\s*%\s*(?:alc\.?/vol\.?|abv|by\s+vol\.?)?");

            return match.Success ? $"{match.Groups[1].Value}%" : string.Empty;
        }

        private static string ExtractNetContents(string text)
        {
            var match = Regex.Match(
                text,
                @"(?i)\b([0-9]{1,4}(?:\.[0-9]+)?)\s*(ml|mL|milliliter|milliliters|l|liter|liters)\b");

            return match.Success
                ? $"{match.Groups[1].Value} {NormalizeUnit(match.Groups[2].Value)}"
                : string.Empty;
        }

        private static string ExtractGovernmentWarning(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                return string.Empty;
            }

            var normalized = text.Replace("\r", " ").Replace("\n", " ");

            normalized = Regex.Replace(normalized, @"\s+", " ").Trim();

            var match = Regex.Match(normalized, @"(?<warning>government\s+warning\s*:.*?health\s+problems\.?)", RegexOptions.IgnoreCase);

            return match.Success ? match.Groups["warning"].Value.Trim() : string.Empty;
        }

        private static string ExtractAppellation(string text)
        {
            var known = new[]
            {
                "Napa Valley",
                "Sonoma County",
                "California",
                "Willamette Valley",
                "Columbia Valley",
                "Paso Robles",
                "Russian River Valley"
            };

            return known.FirstOrDefault(x => text.Contains(x, StringComparison.OrdinalIgnoreCase)) ?? string.Empty;
        }

        private static string ExtractVarietal(string text)
        {
            var known = new[]
            {
                "Cabernet Sauvignon",
                "Merlot",
                "Pinot Noir",
                "Chardonnay",
                "Sauvignon Blanc",
                "Riesling",
                "Zinfandel",
                "Syrah",
                "Malbec"
            };

            return known.FirstOrDefault(x => text.Contains(x, StringComparison.OrdinalIgnoreCase)) ?? string.Empty;
        }

        private static string ExtractProducerStatement(string text)
        {
            var match = Regex.Match(
                text,
                @"(?i)\b(bottled by|produced by|imported by|vinted by|cellared by)\s+(.+)");

            return match.Success ? match.Value.Trim() : string.Empty;
        }

        private static string ExtractFancifulName(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                return string.Empty;
            }

            var knownFancifulNames = new[]
            {
                "Small Batch Reserve",
                "Reserve Collection",
                "Single Barrel",
                "Barrel Select",
                "Limited Release"
            };

            foreach (var name in knownFancifulNames)
            {
                if (text.Contains(name, StringComparison.OrdinalIgnoreCase))
                {
                    return name;
                }
            }

            return string.Empty;
        }

        private static string ExtractCountryOfOrigin(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                return string.Empty;
            }

            var patterns = new[]
            {
                @"(?im)^\s*(produced in the usa)\s*$",
                @"(?im)^\s*(produced in usa)\s*$",
                @"(?im)^\s*(product of the usa)\s*$",
                @"(?im)^\s*(product of usa)\s*$",
                @"(?im)^\s*(made in the usa)\s*$",
                @"(?im)^\s*(made in usa)\s*$",
                @"(?im)^\s*(imported from\s+[a-zA-Z\s]+)\s*$"
            };

            foreach (var pattern in patterns)
            {
                var match = Regex.Match(text, pattern);

                if (match.Success)
                {
                    return CleanValue(match.Groups[1].Value);
                }
            }

            if (text.Contains("PRODUCED IN THE USA", StringComparison.OrdinalIgnoreCase))
            {
                return "Produced In The USA";
            }

            if (text.Contains("PRODUCT OF USA", StringComparison.OrdinalIgnoreCase))
            {
                return "Product Of USA";
            }

            return string.Empty;
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

        private static List<string> CleanLines(string text)
        {
            return [.. text
                .Split(Environment.NewLine)
                .Select(x => x.Trim())
                .Where(x => !string.IsNullOrWhiteSpace(x))];
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