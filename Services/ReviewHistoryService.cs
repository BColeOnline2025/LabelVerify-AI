using LabelVerify.Web.Data;
using LabelVerify.Web.Models;
using System.Text.Json;

namespace LabelVerify.Web.Services
{
    public class ReviewHistoryService(ApplicationDbContext db)
    {
        private readonly ApplicationDbContext _db = db;

        public async Task<Guid> SaveAsync(Guid reviewId, ApprovedProductProfile approved, LabelFacts production,
            VerificationResult verificationResult, string colaPackageFileName, IEnumerable<string> productionLabelFiles,
            long processingTimeMs, string? colaBlobUrl, List<string>? labelBlobUrls, string? aiComplianceSummary,
            DateTime? aiSummaryGeneratedUtc, string? aiModelUsed, string? aiPromptVersion, int? aiPromptTokens,
            int? aiCompletionTokens, int? aiTotalTokens, double? aiGenerationTimeMs, int riskScore,
            string? riskLevel, string? riskFactors, string? aiRiskAssessment)
        {
            var session = new ReviewSession
            {
                Id = reviewId,
                ReviewDateUtc = DateTime.UtcNow,
                BrandName = approved.BrandName,
                Recommendation = verificationResult.Recommendation,
                OverallScore = verificationResult.OverallScore,
                ProcessingTimeMs = processingTimeMs,
                ColaPackageFileName = colaPackageFileName,
                UploadedLabelFiles = string.Join("; ", productionLabelFiles),
                ApprovedProfileJson = JsonSerializer.Serialize(approved),
                ProductionFactsJson = JsonSerializer.Serialize(production),
                VerificationResultJson = JsonSerializer.Serialize(verificationResult),
                ColaPackageBlobUrl = colaBlobUrl,
                ProductionLabelBlobUrlsJson = JsonSerializer.Serialize(labelBlobUrls),
                WorkflowStatus = "Submitted",
                AiComplianceSummary = aiComplianceSummary,
                AiSummaryGeneratedUtc = aiSummaryGeneratedUtc,
                AiModelUsed = aiModelUsed,
                AiPromptVersion = aiPromptVersion,
                AiPromptTokens = aiPromptTokens,
                AiCompletionTokens = aiCompletionTokens,
                AiTotalTokens = aiTotalTokens,
                RiskScore = riskScore,
                RiskLevel = riskLevel,
                RiskFactors = riskFactors,
                AiRiskAssessment = aiRiskAssessment,
                AiGenerationTimeMs = aiGenerationTimeMs
            };

            foreach (var check in verificationResult.Checks)
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
                        AiAnalysis = check.AiAnalysis,
                        Notes = check.Notes
                    });
            }

            _db.ReviewSessions.Add(session);

            await _db.SaveChangesAsync();

            return session.Id;
        }
    }
}