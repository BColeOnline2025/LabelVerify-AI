namespace LabelVerify.Web.Models
{
    public class ColaPackageExtractionResult
    {
        public ApprovedProductProfile Profile { get; set; } = new();
        public string RawOcrText { get; set; } = string.Empty;
    }
}