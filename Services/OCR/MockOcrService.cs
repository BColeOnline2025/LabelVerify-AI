using LabelVerify.Web.Services.Interfaces;

namespace LabelVerify.Web.Services.OCR
{
    public class MockOcrService : IOcrService
    {
        public Task<string> ExtractTextAsync(
            Stream imageStream)
        {
            return Task.FromResult(
                """
            OLD TOM DISTILLERY

            Kentucky Straight Bourbon Whiskey

            45% Alc./Vol. (90 Proof)

            750 mL

            GOVERNMENT WARNING:
            (1) According to the Surgeon General...
            """);
        }
    }
}