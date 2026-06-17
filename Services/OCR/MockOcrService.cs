using LabelVerify.Web.Models;
using LabelVerify.Web.Services.Interfaces;

namespace LabelVerify.Web.Services.OCR
{
    public class MockOcrService : IOcrService
    {
        public Task<string> ExtractTextAsync(Stream documentStream)
        {
            return Task.FromResult("Mock OCR text");
        }

        public async Task<OcrResult> ExtractTextWithLayoutAsync(Stream documentStream)
        {
            var text = await ExtractTextAsync(documentStream);

            return new OcrResult
            {
                Text = text,
                GovernmentWarningHeaderHeight = 0
            };
        }
    }
}