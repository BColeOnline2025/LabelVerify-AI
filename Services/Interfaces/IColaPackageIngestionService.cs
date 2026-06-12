using LabelVerify.Web.Models;

namespace LabelVerify.Web.Services.Interfaces
{
    public interface IColaPackageIngestionService
    {
        Task<ColaPackageExtractionResult>ExtractPackageAsync(Stream pdfStream);
    }
}