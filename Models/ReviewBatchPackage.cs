namespace LabelVerify.Web.Models
{
    public class ReviewBatchPackage
    {
        public Guid Id { get; set; }
        public Guid ReviewBatchId { get; set; }
        public ReviewBatch? ReviewBatch { get; set; }
        public string ColaPackageFileName { get; set; } = string.Empty;
        public string ColaPackageBlobUrl { get; set; } = string.Empty;
        public string Status { get; set; } = "Pending";
        public Guid? ReviewSessionId { get; set; }
        public DateTime UploadedUtc { get; set; }
        public DateTime? CompletedUtc { get; set; }
        public string? AssignedReviewer { get; set; }
        public DateTime? AssignedUtc { get; set; }
    }
}