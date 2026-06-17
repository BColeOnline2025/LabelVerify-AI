using LabelVerify.Web.Models;

namespace LabelVerify.Web.Services.Interfaces
{
    public interface IOcrService
    {
        Task<string> ExtractTextAsync(Stream imageStream);
        Task<OcrResult> ExtractTextWithLayoutAsync(Stream documentStream);
    }
}