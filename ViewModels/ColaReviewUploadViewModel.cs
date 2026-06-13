namespace LabelVerify.Web.ViewModels
{
    public class ColaReviewUploadViewModel
    {
        public IFormFile? ColaPackagePdf { get; set; }
        public List<IFormFile> ProductionLabelImages { get; set; }
    }
}