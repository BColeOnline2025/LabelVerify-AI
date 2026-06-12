using Azure;
using Azure.AI.DocumentIntelligence;
using LabelVerify.Web.Options;
using Microsoft.Extensions.Options;

namespace LabelVerify.Web.Services
{
    public class AzureDocumentOcrService(IOptions<AzureDocumentIntelligenceOptions> options)
    {
        private readonly AzureDocumentIntelligenceOptions _options = options.Value;

        public async Task<string> ExtractTextAsync(Stream documentStream)
        {
            if (string.IsNullOrWhiteSpace(_options.Endpoint) ||
                string.IsNullOrWhiteSpace(_options.ApiKey))
            {
                throw new InvalidOperationException("Azure Document Intelligence is not configured.");
            }

            var client = new DocumentIntelligenceClient(
                new Uri(_options.Endpoint),
                new AzureKeyCredential(_options.ApiKey));

            var operation = await client.AnalyzeDocumentAsync(
                WaitUntil.Completed,
                "prebuilt-read",
                BinaryData.FromStream(documentStream));

            var result = operation.Value;

            var lines = result.Pages
                .SelectMany(page => page.Lines)
                .Select(line => line.Content)
                .Where(line => !string.IsNullOrWhiteSpace(line));

            var text = string.Join(Environment.NewLine, lines);

            return TrimToApplicationAndLabels(text);
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
    }
}