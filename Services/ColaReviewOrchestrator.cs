using LabelVerify.Web.Models;
using LabelVerify.Web.Services.Compliance;
using LabelVerify.Web.Services.Interfaces;
using System.Diagnostics;

namespace LabelVerify.Web.Services
{
    public class ColaReviewOrchestrator(IColaPackageIngestionService colaPackageIngestionService,
        IOcrService ocrService, LabelFactExtractionService labelFactExtractionService,
        ColaPackageComparisonService comparisonService, ComplianceReportService complianceReportService,
        ReviewHistoryService reviewHistoryService, AzureBlobStorageService blobStorageService,
        ReviewAuditLogService auditLogService, AzureOpenAiSummaryService azureOpenAiSummaryService,
        RiskScoringService riskScoringService, ILogger<ColaReviewOrchestrator> logger,
        IServiceScopeFactory serviceScopeFactory, ComplianceInsightsService complianceInsightsService)
    {
        private readonly IColaPackageIngestionService _colaPackageIngestionService = colaPackageIngestionService;
        private readonly IOcrService _ocrService = ocrService;
        private readonly LabelFactExtractionService _labelFactExtractionService = labelFactExtractionService;
        private readonly ColaPackageComparisonService _comparisonService = comparisonService;
        private readonly ComplianceReportService _complianceReportService = complianceReportService;
        private readonly ReviewHistoryService _reviewHistoryService = reviewHistoryService;
        private readonly AzureBlobStorageService _blobStorageService = blobStorageService;
        private readonly ReviewAuditLogService _auditLogService = auditLogService;
        private readonly AzureOpenAiSummaryService _azureOpenAiSummaryService = azureOpenAiSummaryService;
        private readonly RiskScoringService _riskScoringService = riskScoringService;
        private readonly ILogger<ColaReviewOrchestrator> _logger = logger;
        private readonly IServiceScopeFactory _serviceScopeFactory = serviceScopeFactory;
        private readonly ComplianceInsightsService _complianceInsightsService = complianceInsightsService;

        private const bool EnableInlineAi = false;

        public async Task<ColaReviewProcessResult> ProcessAsync(IFormFile colaPackagePdf, List<IFormFile> productionLabelImages, string submittedBy)
        {
            await using var colaPackageStream = colaPackagePdf.OpenReadStream();

            return await ProcessAsync(colaPackageStream, colaPackagePdf.FileName, productionLabelImages, submittedBy);
        }

        public async Task<ColaReviewProcessResult> ProcessAsync(Stream colaPackageStream, string colaPackageFileName,
            List<IFormFile> productionLabelImages, string submittedBy)
        {
            var totalWatch = Stopwatch.StartNew();
            var reviewId = Guid.NewGuid();

            var productionFieldSources = new Dictionary<string, string>();

            await _auditLogService.LogAsync(reviewId, "ReviewStarted", $"Review processing started by {submittedBy}.");

            try
            {
                if (colaPackageStream.CanSeek)
                {
                    colaPackageStream.Position = 0;
                }

                var colaUploadWatch = Stopwatch.StartNew();
                var colaBlobUpload = await _blobStorageService.UploadAsync(colaPackageStream, "cola-packages", $"review-{reviewId}.pdf", "application/pdf");
                var colaBlobUrl = colaBlobUpload.BlobUrl;

                LogTiming("Upload COLA", colaUploadWatch);

                var labelUploadWatch = Stopwatch.StartNew();

                var labelUploadTasks = productionLabelImages
                    .Where(x => x.Length > 0)
                    .Select(async labelFile =>
                    {
                        await using var labelStream = labelFile.OpenReadStream();

                        var safeBlobName = $"review-{reviewId}-{Guid.NewGuid()}-{Path.GetFileName(labelFile.FileName)}";

                        var labelBlobUpload = await _blobStorageService.UploadAsync(labelStream,
                            "production-labels", safeBlobName, labelFile.ContentType);

                        return labelBlobUpload.BlobUrl;
                    })
                    .ToList();

                var labelBlobUrls = (await Task.WhenAll(labelUploadTasks)).ToList();

                LogTiming("Upload Labels Parallel", labelUploadWatch);

                if (colaPackageStream.CanSeek)
                {
                    colaPackageStream.Position = 0;
                }

                var ocrWatch = Stopwatch.StartNew();

                var packageTask = _colaPackageIngestionService.ExtractPackageAsync(colaPackageStream);

                var productionOcrTasks = productionLabelImages
                    .Where(x => x.Length > 0)
                    .Select(async file =>
                    {
                        await using var productionStream = file.OpenReadStream();

                        var ocrResult = await _ocrService.ExtractTextWithLayoutAsync(productionStream);
                        var facts = _labelFactExtractionService.Extract(ocrResult.Text);

                        facts.GovernmentWarningHeaderHeight = ocrResult.GovernmentWarningHeaderHeight;

                        return new ProductionOcrResult
                        {
                            FileName = file.FileName,
                            OcrText = ocrResult.Text,
                            Facts = facts
                        };
                    })
                    .ToList();

                await Task.WhenAll(packageTask, Task.WhenAll(productionOcrTasks));

                var packageResult = await packageTask;
                var productionResults = await Task.WhenAll(productionOcrTasks);

                LogTiming("COLA OCR + Production OCR Parallel", ocrWatch);

                await _auditLogService.LogAsync(reviewId, "ColaOcrComplete", "COLA package OCR completed.");

                await _auditLogService.LogAsync(reviewId, "ProductionOcrComplete", "Production label OCR completed.");

                var approvedProfile = packageResult.Profile;
                var packageOcrText = packageResult.RawOcrText;

                var productionOcrTexts = new List<string>();
                var productionFacts = new LabelFacts();

                foreach (var item in productionResults)
                {
                    productionOcrTexts.Add($"--- {item.FileName} ---");
                    productionOcrTexts.Add(item.OcrText);

                    MergeProductionFacts(productionFacts, item.Facts, item.FileName, productionFieldSources);
                }

                var productionLabelOcrText = string.Join(Environment.NewLine + Environment.NewLine, productionOcrTexts);

                _logger.LogInformation("COLA package extracted. Brand={BrandName}, ProductType={ProductType}",
                    approvedProfile.BrandName, approvedProfile.ProductType);

                var compareWatch = Stopwatch.StartNew();

                var verificationResult = _comparisonService.Compare(approvedProfile, productionFacts);

                ApplyFieldSourcesToResult(verificationResult, productionFieldSources);

                LogTiming("Comparison + Field Sources", compareWatch);

                _logger.LogInformation("COLA review comparison completed. Recommendation={Recommendation}, Score={Score}",
                    verificationResult.Recommendation, verificationResult.OverallScore);

                await _auditLogService.LogAsync(reviewId, "ComparisonComplete",
                    $"Recommendation={verificationResult.Recommendation}; Score={verificationResult.OverallScore}");

                var riskWatch = Stopwatch.StartNew();

                var riskAssessment = _riskScoringService.Calculate(verificationResult);

                LogTiming("Risk Score", riskWatch);

                string? aiRiskAssessment;

                AiGenerationResult aiResult;

                if (EnableInlineAi)
                {
                    var aiWatch = Stopwatch.StartNew();

                    aiRiskAssessment = await _azureOpenAiSummaryService.GenerateRiskAssessmentAsync(
                        new
                        {
                            riskAssessment.RiskScore,
                            riskAssessment.RiskLevel,
                            riskAssessment.RiskFactors,
                            verificationResult.Recommendation,
                            verificationResult.OverallScore,
                            Checks = verificationResult.Checks.Select(x => new
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

                    var failureAnalysisTasks = verificationResult.Checks
                        .Where(x =>
                            string.Equals(x.Status, "Fail", StringComparison.OrdinalIgnoreCase) ||
                            string.Equals(x.Status, "Review", StringComparison.OrdinalIgnoreCase))
                        .Select(async check =>
                        {
                            check.AiAnalysis = await _azureOpenAiSummaryService.GenerateFailureAnalysisAsync(check);
                        })
                        .ToList();

                    await Task.WhenAll(failureAnalysisTasks);

                    aiResult = await _azureOpenAiSummaryService.GenerateComplianceSummaryAsync(approvedProfile, productionFacts, verificationResult);

                    var reviewerCommentary = await _azureOpenAiSummaryService.GenerateReviewerCommentaryAsync(verificationResult);

                    await _auditLogService.LogAsync(reviewId, "AISummaryGenerated", "Azure OpenAI compliance summary generated.");

                    LogTiming("Inline AI", aiWatch);
                }
                else
                {
                    aiRiskAssessment = "AI risk assessment pending.";

                    aiResult = new AiGenerationResult
                    {
                        Summary = "AI compliance summary pending.",
                        ModelUsed = "Pending",
                        PromptVersion = "Pending",
                        GeneratedUtc = DateTime.UtcNow,
                        GenerationTimeMs = 0
                    };

                    await _auditLogService.LogAsync(reviewId, "AIDeferred", "AI generation deferred to improve processing time.");
                }

                totalWatch.Stop();

                var processingTimeMs = totalWatch.ElapsedMilliseconds;

                var saveWatch = Stopwatch.StartNew();

                var reviewSessionId = await _reviewHistoryService.SaveAsync(
                    reviewId, approvedProfile, productionFacts, verificationResult, 
                    colaPackageFileName, productionLabelImages.Select(x => x.FileName),
                    processingTimeMs, colaBlobUrl, labelBlobUrls, aiResult.Summary,
                    aiResult.GeneratedUtc, aiResult.ModelUsed, aiResult.PromptVersion,
                    aiResult.PromptTokens, aiResult.CompletionTokens, aiResult.TotalTokens,
                    aiResult.GenerationTimeMs, riskAssessment.RiskScore, riskAssessment.RiskLevel,
                    riskAssessment.RiskFactors, aiRiskAssessment);

                LogTiming("Save Review", saveWatch);

                _ = Task.Run(async () =>
                {
                    try
                    {
                        using var scope = _serviceScopeFactory.CreateScope();

                        var enrichmentService = scope.ServiceProvider.GetRequiredService<AiReviewEnrichmentService>();

                        await enrichmentService.EnrichAsync(reviewSessionId);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Background AI enrichment task failed. ReviewSessionId={ReviewSessionId}", reviewSessionId);
                    }
                });
                
                var uploadedFiles = productionLabelImages.Select(x => x.FileName).ToList();

                var auditReport = _complianceReportService.Create(approvedProfile, productionFacts, verificationResult, uploadedFiles);

                auditReport.ReviewId = reviewSessionId;
                auditReport.AiComplianceSummary = aiResult.Summary;
                auditReport.AiRiskAssessment = aiRiskAssessment;
                auditReport.RiskScore = riskAssessment.RiskScore;
                auditReport.RiskLevel = riskAssessment.RiskLevel;
                auditReport.RiskFactors = riskAssessment.RiskFactors;

                var complianceInsights = _complianceInsightsService.Generate(verificationResult);

                auditReport.ComplianceInsights = complianceInsights;
                
                await _auditLogService.LogAsync(reviewId, "ReviewSaved", "Review successfully saved.");

                _logger.LogInformation("COLA review saved. ReviewSessionId={ReviewSessionId}, ProcessingTimeMs={ProcessingTimeMs}",
                    reviewSessionId, processingTimeMs);

                return new ColaReviewProcessResult
                {
                    ReviewSessionId = reviewSessionId,
                    ApprovedProfile = approvedProfile,
                    ProductionFacts = productionFacts,
                    VerificationResult = verificationResult,
                    PackageOcrText = packageOcrText,
                    ProductionLabelOcrText = productionLabelOcrText,
                    ProcessingTimeMs = processingTimeMs,
                    AuditReport = auditReport
                };
            }
            catch (Exception ex)
            {
                totalWatch.Stop();

                _logger.LogError(ex, "COLA review orchestration failed. ReviewId={ReviewId}", reviewId);

                await _auditLogService.LogAsync(reviewId, "ReviewFailed", $"Review failed: {ex.Message}");

                throw;
            }
        }

        private void LogTiming(string step, Stopwatch stopwatch)
        {
            stopwatch.Stop();

            _logger.LogInformation("Timing: {Step} took {ElapsedMs} ms", step, stopwatch.ElapsedMilliseconds);
        }

        private void MergeProductionFacts(LabelFacts combined, LabelFacts current, string fileName, Dictionary<string, string> productionFieldSources)
        {
            SetIfPresent(current.BrandName, value => combined.BrandName = value, "Brand Name", fileName, productionFieldSources);
            SetIfPresent(current.FancifulName, value => combined.FancifulName = value, "Fanciful Name", fileName, productionFieldSources);
            SetIfPresent(current.ClassType, value => combined.ClassType = value, "Class / Type", fileName, productionFieldSources);
            SetIfPresent(current.AlcoholContent, value => combined.AlcoholContent = value, "Alcohol Content", fileName, productionFieldSources);
            SetIfPresent(current.AlcoholContentStatement, value => combined.AlcoholContentStatement = value, "Alcohol Content Format", fileName, productionFieldSources); SetIfPresent(current.NetContents, value => combined.NetContents = value, "Net Contents", fileName, productionFieldSources);
            SetIfPresent(current.GovernmentWarning, value => combined.GovernmentWarning = value, "Government Warning", fileName, productionFieldSources);
            SetIfPresent(current.ProducerStatement, value => combined.ProducerStatement = value, "Producer Statement", fileName, productionFieldSources);
            SetIfPresent(current.CountryOfOrigin, value => combined.CountryOfOrigin = value, "Country of Origin", fileName, productionFieldSources);
            SetIfPresent(current.Appellation, value => combined.Appellation = value, "Appellation", fileName, productionFieldSources);
            SetIfPresent(current.Varietal, value => combined.Varietal = value, "Varietal", fileName, productionFieldSources);
            SetIfPresent(current.SulfitesStatement, value => combined.SulfitesStatement = value, "Sulfites Statement", fileName, productionFieldSources);

            if (current.GovernmentWarningHeaderHeight > 0)
            {
                combined.GovernmentWarningHeaderHeight = current.GovernmentWarningHeaderHeight;
            }
        }

        private void SetIfPresent(string value, Action<string> setValue, string fieldName, string fileName, Dictionary<string, string> productionFieldSources)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return;
            }

            setValue(value);

            if (!productionFieldSources.ContainsKey(fieldName))
            {
                productionFieldSources[fieldName] = fileName;
            }
        }

        private void ApplyFieldSourcesToResult(VerificationResult result, Dictionary<string, string> productionFieldSources)
        {
            foreach (var check in result.Checks)
            {
                if (productionFieldSources.TryGetValue(check.FieldName, out var sourceLabel))
                {
                    check.SourceLabel = sourceLabel;
                }
                else if (check.FieldName.StartsWith("Government Warning") &&
                         productionFieldSources.TryGetValue("Government Warning", out var governmentWarningSource))
                {
                    check.SourceLabel = governmentWarningSource;
                }
                else if (check.FieldName.StartsWith("Alcohol Content") && productionFieldSources.TryGetValue("Alcohol Content", out var alcoholSource))
                {
                    check.SourceLabel = alcoholSource;
                }
                else if (
                    check.FieldName == "Alcohol Content Format" && productionFieldSources.TryGetValue("Alcohol Content Format", out var alcoholFormatSource))
                {
                    check.SourceLabel = alcoholFormatSource;
                }
                else if (
                    check.FieldName.StartsWith("Sulfites") && productionFieldSources.TryGetValue("Sulfites Statement", out var sulfiteSource))
                {
                    check.SourceLabel = sulfiteSource;
                }
                else
                {
                    check.SourceLabel = "Not found";
                }
            }
        }

        public class ColaReviewProcessResult
        {
            public Guid ReviewSessionId { get; set; }
            public ApprovedProductProfile ApprovedProfile { get; set; } = new();
            public LabelFacts ProductionFacts { get; set; } = new();
            public VerificationResult VerificationResult { get; set; } = new();
            public string? PackageOcrText { get; set; }
            public string? ProductionLabelOcrText { get; set; }
            public long ProcessingTimeMs { get; set; }
            public ComplianceAuditReport AuditReport { get; set; } = new();
        }

        private sealed class ProductionOcrResult
        {
            public string FileName { get; set; } = string.Empty;
            public string OcrText { get; set; } = string.Empty;
            public LabelFacts Facts { get; set; } = new();
        }
    }
}