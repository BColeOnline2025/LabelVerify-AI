using LabelVerify.Web.Data;
using LabelVerify.Web.Models;

namespace LabelVerify.Web.Services
{
    public class ReviewAuditLogService(ApplicationDbContext db)
    {
        private readonly ApplicationDbContext _db = db;

        public async Task LogAsync(Guid reviewSessionId, string eventType,
            string message, string severity = "Information")
        {
            _db.ReviewAuditLogs.Add(
                new ReviewAuditLog
                {
                    Id = Guid.NewGuid(),
                    ReviewSessionId = reviewSessionId,
                    TimestampUtc = DateTime.UtcNow,
                    EventType = eventType,
                    Message = message,
                    Severity = severity
                });

            await _db.SaveChangesAsync();
        }
    }
}