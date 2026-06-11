using Azure;
using Azure.AI.Vision.ImageAnalysis;
using LabelVerify.Web.Options;
using LabelVerify.Web.Services.Interfaces;
using Microsoft.Extensions.Options;

namespace LabelVerify.Web.Services.OCR
{
    public class AzureVisionOcrService : IOcrService
    {
        private readonly AzureVisionOptions _options;

        public AzureVisionOcrService(IOptions<AzureVisionOptions> options)
        {
            _options = options.Value;
        }

        public async Task<string> ExtractTextAsync(Stream imageStream)
        {
            if (string.IsNullOrWhiteSpace(_options.Endpoint) ||
                string.IsNullOrWhiteSpace(_options.ApiKey))
            {
                throw new InvalidOperationException(
                    "Azure Vision OCR is not configured. Endpoint and ApiKey are required.");
            }

            var client = new ImageAnalysisClient(
                new Uri(_options.Endpoint),
                new AzureKeyCredential(_options.ApiKey));

            var imageData = BinaryData.FromStream(imageStream);

            var result = await client.AnalyzeAsync(
                imageData,
                VisualFeatures.Read);

            var lines = result.Value.Read?.Blocks?
                .SelectMany(block => block.Lines)
                .Select(line => line.Text)
                .Where(text => !string.IsNullOrWhiteSpace(text))
                .ToList() ?? [];

            return string.Join(Environment.NewLine, lines);
        }
    }
}