using LabelVerify.Web.Models;
using LabelVerify.Web.Services.Interfaces;
using System.Diagnostics;

namespace LabelVerify.Web.Services
{
    public class ColaPackageIngestionService(AzureDocumentOcrService documentOcrService,
        ApplicationFieldExtractionService fieldExtractionService) : IColaPackageIngestionService
    {
        private readonly AzureDocumentOcrService _documentOcrService = documentOcrService;
        private readonly ApplicationFieldExtractionService _fieldExtractionService = fieldExtractionService;

        public async Task<ColaPackageExtractionResult>ExtractPackageAsync(Stream pdfStream)
        {
            var sw = Stopwatch.StartNew();
            
            var text = await _documentOcrService.ExtractTextAsync(pdfStream);

            var ocrTime = sw.ElapsedMilliseconds;
            
            var profile = _fieldExtractionService.ExtractOfficialTtb510031(text);

            var extractionTime = sw.ElapsedMilliseconds;

            Console.WriteLine($"OCR: {ocrTime} ms | Extraction: {extractionTime - ocrTime} ms");
            
            return new ColaPackageExtractionResult
            {
                Profile = profile,
                RawOcrText = text
            };
        }
    }
}