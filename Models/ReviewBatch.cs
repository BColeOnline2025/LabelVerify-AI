namespace LabelVerify.Web.Models
{
    public class ReviewBatch
    {
        public Guid Id { get; set; }
        public string BatchName { get; set; } = string.Empty;
        public DateTime CreatedUtc { get; set; }
        public string CreatedBy { get; set; } = string.Empty;
        public string Status { get; set; } = "Open";
        public List<ReviewBatchPackage> Packages { get; set; } = [];
    }
}