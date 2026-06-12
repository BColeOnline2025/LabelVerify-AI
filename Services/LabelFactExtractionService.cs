using System.Text.RegularExpressions;
using LabelVerify.Web.Models;

namespace LabelVerify.Web.Services
{
    public class LabelFactExtractionService
    {
        public LabelFacts Extract(string text)
        {
            return new LabelFacts
            {
                BrandName = ExtractBrandCandidate(text),
                ClassType = ExtractClassType(text),
                AlcoholContent = ExtractAlcoholContent(text),
                NetContents = ExtractNetContents(text),
                GovernmentWarning = ExtractGovernmentWarning(text),
                Appellation = ExtractAppellation(text),
                Varietal = ExtractVarietal(text),
                ProducerStatement = ExtractProducerStatement(text),
                CountryOfOrigin = ExtractCountryOfOrigin(text)
            };
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
            return text.Contains("GOVERNMENT WARNING", StringComparison.OrdinalIgnoreCase)
                ? "Present"
                : string.Empty;
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

        private static string ExtractCountryOfOrigin(string text)
        {
            var match = Regex.Match(
                text,
                @"(?i)\b(product of|imported from)\s+([a-zA-Z\s]+)");

            return match.Success ? match.Value.Trim() : string.Empty;
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
    }
}