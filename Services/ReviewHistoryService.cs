using LabelVerify.Web.Data;
using LabelVerify.Web.Models;

namespace LabelVerify.Web.Services
{
    public class ReviewHistoryService
    {
        private readonly ApplicationDbContext _db;

        public ReviewHistoryService(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<Guid> SaveAsync(
            ApprovedProductProfile approved,
            LabelFacts production,
            VerificationResult result,
            string colaPackageFileName,
            IEnumerable<string> productionLabelFiles,
            long processingTimeMs)
        {
            var session = new ReviewSession
            {
                ReviewDateUtc = DateTime.UtcNow,
                Recommendation = result.Recommendation,
                OverallScore = result.OverallScore,
                ProcessingTimeMs = processingTimeMs,
                ColaPackageFileName = colaPackageFileName,
                UploadedLabelFiles = string.Join("; ", productionLabelFiles)
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