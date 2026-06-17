namespace LabelVerify.Web.Models
{
    public class MyWorkQueueItem
    {
        public Guid Id { get; set; }
        public string SourceType { get; set; } = "";
        public string? BatchName { get; set; }
        public string Title { get; set; } = "";
        public string Status { get; set; } = "";
        public DateTime CreatedUtc { get; set; }
        public DateTime? AssignedUtc { get; set; }
        public int DaysOpen => (DateTime.UtcNow - CreatedUtc).Days;
        public Guid? ReturnBatchId { get; set; }
    }
}