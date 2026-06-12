namespace LabelVerify.Web.Services.Interfaces
{
    public interface IOcrService
    {
        Task<string> ExtractTextAsync(Stream imageStream);
    }
}