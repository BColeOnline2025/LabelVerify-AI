using Azure;
using Azure.AI.DocumentIntelligence;
using LabelVerify.Web.Models;
using LabelVerify.Web.Options;
using Microsoft.Extensions.Options;

namespace LabelVerify.Web.Services
{
    public class AzureDocumentOcrService(IOptions<AzureDocumentIntelligenceOptions> options)
    {
        private readonly AzureDocumentIntelligenceOptions _options = options.Value;

        public async Task<string> ExtractTextAsync(Stream documentStream)
        {
            var result = await ExtractTextWithLayoutAsync(documentStream);

            return result.Text;
        }

        private static string TrimToApplicationAndLabels(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                return string.Empty;
            }

            var cutoffMarker = "I. PURPOSE OF THIS CERTIFICATE";

            var cutoff = text.IndexOf(cutoffMarker, StringComparison.OrdinalIgnoreCase);

            if (cutoff > 0)
            {
                return text[..cutoff].Trim();
            }

            return text.Trim();
        }

        private static double EstimateLineHeight(DocumentLine line)
        {
            if (line.Polygon == null || line.Polygon.Count < 8)
            {
                return 0;
            }

            var minY = double.MaxValue;
            var maxY = double.MinValue;

            for (int i = 1; i < line.Polygon.Count; i += 2)
            {
                var y = line.Polygon[i];

                if (y < minY)
                    minY = y;

                if (y > maxY)
                    maxY = y;
            }

            return maxY - minY;
        }

        public async Task<OcrResult> ExtractTextWithLayoutAsync(Stream stream)
        {
            if (string.IsNullOrWhiteSpace(_options.Endpoint) || string.IsNullOrWhiteSpace(_options.ApiKey))
            {
                throw new InvalidOperationException("Azure Document Intelligence is not configured.");
            }

            if (stream.CanSeek)
            {
                stream.Position = 0;
            }

            var client = new DocumentIntelligenceClient(new Uri(_options.Endpoint), new AzureKeyCredential(_options.ApiKey));

            var operation = await client.AnalyzeDocumentAsync(WaitUntil.Completed, "prebuilt-read", BinaryData.FromStream(stream));

            var result = operation.Value;

            var lines = new List<string>();
            var warningHeaderHeight = 0.0;

            foreach (var page in result.Pages)
            {
                foreach (var line in page.Lines)
                {
                    if (!string.IsNullOrWhiteSpace(line.Content))
                    {
                        lines.Add(line.Content);
                    }

                    if (line.Content.Contains("GOVERNMENT WARNING:", StringComparison.OrdinalIgnoreCase))
                    {
                        warningHeaderHeight = EstimateLineHeight(line);
                    }
                }
            }

            var text = string.Join(Environment.NewLine, lines);

            return new OcrResult
            {
                Text = TrimToApplicationAndLabels(text),
                GovernmentWarningHeaderHeight = warningHeaderHeight
            };
        }
    }
}