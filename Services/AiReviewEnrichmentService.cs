using System.Text.Json;
using LabelVerify.Web.Data;
using LabelVerify.Web.Models;
using Microsoft.EntityFrameworkCore;

namespace LabelVerify.Web.Services
{
    public class AiReviewEnrichmentService(ApplicationDbContext db, AzureOpenAiSummaryService azureOpenAiSummaryService,
        RiskScoringService riskScoringService, ILogger<AiReviewEnrichmentService> logger)
    {
        private readonly ApplicationDbContext _db = db;
        private readonly AzureOpenAiSummaryService _azureOpenAiSummaryService = azureOpenAiSummaryService;
        private readonly RiskScoringService _riskScoringService = riskScoringService;
        private readonly ILogger<AiReviewEnrichmentService> _logger = logger;

        public async Task EnrichAsync(Guid reviewSessionId)
        {
            var review = await _db.ReviewSessions.FirstOrDefaultAsync(x => x.Id == reviewSessionId);

            if (review == null)
            {
                _logger.LogWarning("AI enrichment skipped. Review not found. ReviewSessionId={ReviewSessionId}", reviewSessionId);

                return;
            }

            try
            {
                var approved = JsonSerializer.Deserialize<ApprovedProductProfile>(review.ApprovedProfileJson ?? "{}") ?? new ApprovedProductProfile();

                var production = JsonSerializer.Deserialize<LabelFacts>(review.ProductionFactsJson ?? "{}") ?? new LabelFacts();

                var result = JsonSerializer.Deserialize<VerificationResult>(review.VerificationResultJson ?? "{}") ?? new VerificationResult();

                var riskAssessment = _riskScoringService.Calculate(result);

                var aiRiskAssessment = await _azureOpenAiSummaryService.GenerateRiskAssessmentAsync(
                    new
                    {
                        riskAssessment.RiskScore,
                        riskAssessment.RiskLevel,
                        riskAssessment.RiskFactors,
                        result.Recommendation,
                        result.OverallScore,
                        Checks = result.Checks.Select(x => new
                        {
                            x.FieldName,
                            x.ExpectedValue,
                            x.ActualValue,
                            x.SourceLabel,
                            x.Status,
                            x.ConfidenceScore,
                            x.Notes
                        })
                    });

                foreach (var check in result.Checks.Where(x =>
                    string.Equals(x.Status, "Fail", StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(x.Status, "Review", StringComparison.OrdinalIgnoreCase)))
                {
                    check.AiAnalysis = await _azureOpenAiSummaryService.GenerateFailureAnalysisAsync(check);
                }

                var aiResult = await _azureOpenAiSummaryService.GenerateComplianceSummaryAsync(approved, production, result);

                review.AiComplianceSummary = aiResult.Summary;
                review.AiSummaryGeneratedUtc = aiResult.GeneratedUtc;
                review.AiModelUsed = aiResult.ModelUsed;
                review.AiPromptVersion = aiResult.PromptVersion;
                review.AiPromptTokens = aiResult.PromptTokens;
                review.AiCompletionTokens = aiResult.CompletionTokens;
                review.AiTotalTokens = aiResult.TotalTokens;
                review.AiGenerationTimeMs = aiResult.GenerationTimeMs;
                review.RiskScore = riskAssessment.RiskScore;
                review.RiskLevel = riskAssessment.RiskLevel;
                review.RiskFactors = riskAssessment.RiskFactors;
                review.AiRiskAssessment = aiRiskAssessment;
                review.VerificationResultJson = JsonSerializer.Serialize(result);

                await _db.SaveChangesAsync();

                _logger.LogInformation("AI enrichment completed. ReviewSessionId={ReviewSessionId}", reviewSessionId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "AI enrichment failed. ReviewSessionId={ReviewSessionId}", reviewSessionId);

                review.AiComplianceSummary = $"AI enrichment failed: {ex.Message}";

                review.AiModelUsed = "Failed";
                review.AiSummaryGeneratedUtc = DateTime.UtcNow;

                await _db.SaveChangesAsync();
            }
        }
    }
}