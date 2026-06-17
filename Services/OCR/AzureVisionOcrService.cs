using Azure;
using Azure.AI.Vision.ImageAnalysis;
using LabelVerify.Web.Models;
using LabelVerify.Web.Options;
using LabelVerify.Web.Services.Interfaces;
using Microsoft.Extensions.Options;

namespace LabelVerify.Web.Services.OCR
{
    public class AzureVisionOcrService(IOptions<AzureVisionOptions> options) : IOcrService
    {
        private readonly AzureVisionOptions _options = options.Value;

        public async Task<string> ExtractTextAsync(Stream imageStream)
        {
            if (string.IsNullOrWhiteSpace(_options.Endpoint) || string.IsNullOrWhiteSpace(_options.ApiKey))
            {
                throw new InvalidOperationException("Azure Vision OCR is not configured.");
            }

            if (imageStream.CanSeek)
            {
                imageStream.Position = 0;
            }

            var client = new ImageAnalysisClient(new Uri(_options.Endpoint), new AzureKeyCredential(_options.ApiKey));

            var imageData = BinaryData.FromStream(imageStream);

            var result = await client.AnalyzeAsync(imageData, VisualFeatures.Read);

            var lines = result.Value.Read?.Blocks.SelectMany(x => x.Lines)
                .Select(x => x.Text)
                .Where(x => !string.IsNullOrWhiteSpace(x)) ?? [];

            return string.Join(Environment.NewLine, lines);
        }

        public async Task<OcrResult> ExtractTextWithLayoutAsync(Stream stream)
        {
            var text = await ExtractTextAsync(stream);

            return new OcrResult
            {
                Text = text,
                GovernmentWarningHeaderHeight = 0
            };
        }
    }
}