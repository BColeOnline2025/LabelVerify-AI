using LabelVerify.Web.Models;
using LabelVerify.Web.Services.Interfaces;

namespace LabelVerify.Web.Services
{
    public class ColaPackageIngestionService(AzureDocumentOcrService documentOcrService,
        ApplicationFieldExtractionService fieldExtractionService) : IColaPackageIngestionService
    {
        private readonly AzureDocumentOcrService _documentOcrService = documentOcrService;
        private readonly ApplicationFieldExtractionService _fieldExtractionService = fieldExtractionService;

        public async Task<ColaPackageExtractionResult>ExtractPackageAsync(Stream pdfStream)
        {
            var text = await _documentOcrService.ExtractTextAsync(pdfStream);

            var profile = _fieldExtractionService.ExtractOfficialTtb510031(text);

            return new ColaPackageExtractionResult
            {
                Profile = profile,
                RawOcrText = text
            };
        }
    }
}