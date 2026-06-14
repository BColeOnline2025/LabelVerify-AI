using LabelVerify.Web.Data;
using LabelVerify.Web.Models;
using System.Text.Json;

namespace LabelVerify.Web.Services
{
    public class ReviewHistoryService(ApplicationDbContext db)
    {
        private readonly ApplicationDbContext _db = db;

        public async Task<Guid> SaveAsync(Guid reviewId, ApprovedProductProfile approved, LabelFacts production,
            VerificationResult result, string colaPackageFileName, IEnumerable<string> productionLabelFiles,
            long processingTimeMs, string? colaBlobUrl, List<string>? labelBlobUrls)
        {
            var session = new ReviewSession
            {
                Id = reviewId,
                ReviewDateUtc = DateTime.UtcNow,
                Recommendation = result.Recommendation,
                OverallScore = result.OverallScore,
                ProcessingTimeMs = processingTimeMs,
                ColaPackageFileName = colaPackageFileName,
                UploadedLabelFiles = string.Join("; ", productionLabelFiles),
                ApprovedProfileJson = JsonSerializer.Serialize(approved),
                ProductionFactsJson = JsonSerializer.Serialize(production),
                ColaPackageBlobUrl = colaBlobUrl,
                ProductionLabelBlobUrlsJson = JsonSerializer.Serialize(labelBlobUrls),
                WorkflowStatus = "Submitted"
            };

            foreach (var check in result.Checks)
            {
                session.Results.Add(
                    new ReviewResultEntity
                    {
                        FieldName = check.FieldName,
                        ExpectedValue = check.ExpectedValue,
                        ActualValue = check.ActualValue,
                        SourceLabel = check.SourceLabel,
                        Status = check.Status,
                        ConfidenceScore = check.ConfidenceScore,
                        Notes = check.Notes
                    });
            }

            _db.ReviewSessions.Add(session);

            await _db.SaveChangesAsync();

            return session.Id;
        }
    }
}