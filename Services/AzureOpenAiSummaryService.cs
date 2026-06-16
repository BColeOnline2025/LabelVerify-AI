using System.Text.Json;
using LabelVerify.Web.Models;
using LabelVerify.Web.Options;
using Microsoft.Extensions.Options;

namespace LabelVerify.Web.Services
{
    public class AzureOpenAiSummaryService(IOptions<AzureOpenAiOptions> options, ILogger<AzureOpenAiSummaryService> logger, HttpClient httpClient)
    {
        private readonly AzureOpenAiOptions _options = options.Value;
        private readonly ILogger<AzureOpenAiSummaryService> _logger = logger;
        private readonly HttpClient _httpClient = httpClient;

        public string ModelName =>_options.DeploymentName;

        public async Task<AiGenerationResult> GenerateComplianceSummaryAsync(ApprovedProductProfile approved,
            LabelFacts production, VerificationResult result)
        {
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();

            if (!IsConfigured())
            {
                return new AiGenerationResult
                {
                    Summary = "Azure OpenAI is not configured. Rule-based compliance summary should be reviewed instead.",
                    ModelUsed = "Not configured",
                    PromptVersion = "ComplianceSummaryPrompt_v1",
                    GeneratedUtc = DateTime.UtcNow,
                    GenerationTimeMs = stopwatch.ElapsedMilliseconds
                };
            }

            try
            {
                var payload = new
                {
                    ApprovedProfile = approved,
                    ProductionFacts = production,
                    SystemRecommendation = result.Recommendation,
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
                };

                var prompt = await BuildPromptAsync(payload);

                var endpoint = _options.Endpoint.TrimEnd('/');
                var url = $"{endpoint}/openai/v1/chat/completions";

                using var request = new HttpRequestMessage(HttpMethod.Post, url);

                request.Headers.Add("api-key", _options.ApiKey);

                var body = new
                {
                    model = _options.DeploymentName,
                    messages = new[]
                    {
                        new
                        {
                            role = "system",
                            content =
                                "You are a compliance review assistant for beverage alcohol label verification. You explain deterministic review results but do not make independent compliance decisions."
                        },
                        new
                        {
                            role = "user",
                            content = prompt
                        }
                },
                    max_completion_tokens = 700,
                    temperature = 0.2
                };

                request.Content = JsonContent.Create(body);

                using var response = await _httpClient.SendAsync(request);

                var responseBody = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    throw new InvalidOperationException($"Azure OpenAI failed: {(int)response.StatusCode} {response.ReasonPhrase}. {responseBody}");
                }

                using var document = JsonDocument.Parse(responseBody);

                var summary = document
                    .RootElement
                    .GetProperty("choices")[0]
                    .GetProperty("message")
                    .GetProperty("content")
                    .GetString();

                if (string.IsNullOrWhiteSpace(summary))
                {
                    summary = "Azure OpenAI returned an empty compliance summary.";
                }

                summary = string.Join(Environment.NewLine, summary.Split('\n').Select(x => x.TrimEnd())).Trim();

                var promptTokens = 0;
                var completionTokens = 0;
                var totalTokens = 0;

                if (document.RootElement.TryGetProperty("usage", out var usage))
                {
                    if (usage.TryGetProperty("prompt_tokens", out var promptTokensElement))
                    {
                        promptTokens = promptTokensElement.GetInt32();
                    }

                    if (usage.TryGetProperty("completion_tokens", out var completionTokensElement))
                    {
                        completionTokens = completionTokensElement.GetInt32();
                    }

                    if (usage.TryGetProperty("total_tokens", out var totalTokensElement))
                    {
                        totalTokens = totalTokensElement.GetInt32();
                    }
                }

                stopwatch.Stop();

                return new AiGenerationResult
                {
                    Summary = summary,
                    ModelUsed = _options.DeploymentName,
                    PromptVersion = "ComplianceSummaryPrompt_v1",
                    PromptTokens = promptTokens,
                    CompletionTokens = completionTokens,
                    TotalTokens = totalTokens,
                    GenerationTimeMs = stopwatch.ElapsedMilliseconds,
                    GeneratedUtc = DateTime.UtcNow
                };
            }
            catch (Exception ex)
            {
                stopwatch.Stop();

                _logger.LogError(ex, "Azure OpenAI compliance summary generation failed.");

                return new AiGenerationResult
                {
                    Summary = $"Azure OpenAI summary generation failed: {ex.Message}",
                    ModelUsed = _options.DeploymentName,
                    PromptVersion = "ComplianceSummaryPrompt_v1",
                    GeneratedUtc = DateTime.UtcNow,
                    GenerationTimeMs = stopwatch.ElapsedMilliseconds
                };
            }
        }

        public async Task<string> GenerateFailureAnalysisAsync(FieldCheckResult check)
        {
            if (!IsConfigured())
            {
                return "Azure OpenAI is not configured.";
            }

            try
            {
                var payload = new
                {
                    check.FieldName,
                    check.ExpectedValue,
                    check.ActualValue,
                    check.SourceLabel,
                    check.Status,
                    check.ConfidenceScore,
                    check.Notes
                };

                var prompt = await BuildFailureAnalysisPromptAsync(payload);

                var endpoint = _options.Endpoint.TrimEnd('/');
                var url = $"{endpoint}/openai/v1/chat/completions";

                using var request = new HttpRequestMessage(HttpMethod.Post, url);

                request.Headers.Add("api-key", _options.ApiKey);

                var body = new
                {
                    model = _options.DeploymentName,
                    messages = new[]
                    {
                        new
                        {
                            role = "system",
                            content = "You explain alcohol label verification findings without changing compliance decisions."
                        },
                        new
                        {
                            role = "user",
                            content = prompt
                        }
                    },
                    max_completion_tokens = 250,
                    temperature = 0.2
                };

                request.Content = JsonContent.Create(body);

                using var response = await _httpClient.SendAsync(request);

                var responseBody = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    throw new InvalidOperationException($"Azure OpenAI failed: {(int)response.StatusCode} {response.ReasonPhrase}. {responseBody}");
                }

                using var document = JsonDocument.Parse(responseBody);

                var analysis = document
                    .RootElement
                    .GetProperty("choices")[0]
                    .GetProperty("message")
                    .GetProperty("content")
                    .GetString();

                if (string.IsNullOrWhiteSpace(analysis))
                {
                    return "AI analysis unavailable.";
                }

                analysis = string.Join(Environment.NewLine, analysis.Split('\n').Select(x => x.TrimEnd()));

                return analysis.Trim();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Azure OpenAI failure analysis generation failed for field {FieldName}.", check.FieldName);

                return $"AI analysis unavailable for {check.FieldName}.";
            }
        }

        public async Task<string> GenerateOperationalInsightsAsync(object metrics)
        {
            if (!IsConfigured())
            {
                return "Azure OpenAI is not configured.";
            }

            try
            {
                var prompt = await BuildOperationalInsightsPromptAsync(metrics);

                var endpoint = _options.Endpoint.TrimEnd('/');
                var url = $"{endpoint}/openai/v1/chat/completions";

                using var request = new HttpRequestMessage(HttpMethod.Post, url);

                request.Headers.Add("api-key", _options.ApiKey);

                var body = new
                {
                    model = _options.DeploymentName,
                    messages = new[]
                    {
                    new
                    {
                        role = "system",
                        content =
                            "You are a compliance operations analyst. You explain operational metrics without inventing facts."
                    },
                    new
                    {
                        role = "user",
                        content = prompt
                    }
                },
                    max_completion_tokens = 500,
                    temperature = 0.2
                };

                request.Content = JsonContent.Create(body);

                using var response = await _httpClient.SendAsync(request);

                var responseBody = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    throw new InvalidOperationException($"Azure OpenAI failed: {(int)response.StatusCode} {response.ReasonPhrase}. {responseBody}");
                }

                using var document = JsonDocument.Parse(responseBody);

                var insights = document
                    .RootElement
                    .GetProperty("choices")[0]
                    .GetProperty("message")
                    .GetProperty("content")
                    .GetString();

                if (string.IsNullOrWhiteSpace(insights))
                {
                    return "Azure OpenAI returned no operational insights.";
                }

                insights = string.Join(Environment.NewLine, insights.Split('\n').Select(x => x.TrimEnd()));

                return insights.Trim();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Azure OpenAI operational insights generation failed.");

                return $"Operational insights unavailable: {ex.Message}";
            }
        }

        private static async Task<string> BuildFailureAnalysisPromptAsync(object payload)
        {
            var promptPath = Path.Combine(AppContext.BaseDirectory, "Prompts", "FailureAnalysisPrompt.txt");

            if (!File.Exists(promptPath))
            {
                throw new FileNotFoundException("Failure analysis prompt file was not found.", promptPath);
            }

            var template = await File.ReadAllTextAsync(promptPath);

            return template.Replace("{{DATA}}", JsonSerializer.Serialize(payload));
        }

        private static async Task<string> BuildOperationalInsightsPromptAsync(object payload)
        {
            var promptPath = Path.Combine(AppContext.BaseDirectory, "Prompts", "OperationalInsightsPrompt.txt");

            if (!File.Exists(promptPath))
            {
                throw new FileNotFoundException("Operational insights prompt file was not found.", promptPath);
            }

            var template = await File.ReadAllTextAsync(promptPath);

            return template.Replace("{{DATA}}", JsonSerializer.Serialize(payload));
        }

        public async Task<string> TestConnectionAsync()
        {
            if (!IsConfigured())
            {
                return "Azure OpenAI is not configured.";
            }

            var endpoint = _options.Endpoint.TrimEnd('/');
            var url = $"{endpoint}/openai/v1/chat/completions";

            using var request = new HttpRequestMessage(HttpMethod.Post, url);

            request.Headers.Add("api-key", _options.ApiKey);

            var body = new
            {
                model = _options.DeploymentName,
                messages = new[]
                {
                    new
                    {
                        role = "user",
                        content = "Respond with: Azure OpenAI connection successful."
                    }
                },
                max_completion_tokens = 50,
                temperature = 0.0
            };

            request.Content = JsonContent.Create(body);

            using var response = await _httpClient.SendAsync(request);

            var responseBody = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                return $"Azure OpenAI failed: {(int)response.StatusCode} {response.ReasonPhrase}. {responseBody}";
            }

            using var document = JsonDocument.Parse(responseBody);

            return document
                .RootElement
                .GetProperty("choices")[0]
                .GetProperty("message")
                .GetProperty("content")
                .GetString()
                ?? "No response returned.";
        }

        private static async Task<string> BuildPromptAsync(object payload)
        {
            var promptPath = Path.Combine(AppContext.BaseDirectory, "Prompts", "ComplianceSummaryPrompt.txt");

            if (!File.Exists(promptPath))
            {
                throw new FileNotFoundException("Compliance summary prompt file was not found.", promptPath);
            }

            var template = await File.ReadAllTextAsync(promptPath);

            return template.Replace("{{DATA}}", JsonSerializer.Serialize(payload));
        }

        private bool IsConfigured()
        {
            return !string.IsNullOrWhiteSpace(_options.Endpoint)
                && !string.IsNullOrWhiteSpace(_options.ApiKey) && !string.IsNullOrWhiteSpace(_options.DeploymentName);
        }

        public async Task<string> GenerateMonthlyComplianceReportAsync(object metrics)
        {
            if (!IsConfigured())
            {
                return "Azure OpenAI is not configured. Monthly compliance report could not be generated.";
            }

            try
            {
                var prompt = await BuildPromptFromFileAsync("MonthlyComplianceReportPrompt.txt", metrics);

                var endpoint = _options.Endpoint.TrimEnd('/');
                var url = $"{endpoint}/openai/v1/chat/completions";

                using var request = new HttpRequestMessage(HttpMethod.Post, url);

                request.Headers.Add("api-key", _options.ApiKey);

                var body = new
                {
                    model = _options.DeploymentName,
                    messages = new[]
                    {
                    new
                    {
                        role = "system",
                        content =
                            "You are a senior compliance operations executive. You generate concise management-ready compliance operations reports based only on supplied metrics."
                    },
                    new
                    {
                        role = "user",
                        content = prompt
                    }
                },
                    max_completion_tokens = 1200,
                    temperature = 0.2
                };

                request.Content = JsonContent.Create(body);

                using var response = await _httpClient.SendAsync(request);

                var responseBody = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    throw new InvalidOperationException($"Azure OpenAI failed: {(int)response.StatusCode} {response.ReasonPhrase}. {responseBody}");
                }

                using var document = JsonDocument.Parse(responseBody);

                var report = document
                    .RootElement
                    .GetProperty("choices")[0]
                    .GetProperty("message")
                    .GetProperty("content")
                    .GetString();

                if (string.IsNullOrWhiteSpace(report))
                {
                    return "Azure OpenAI returned an empty monthly compliance report.";
                }

                report = string.Join(
                    Environment.NewLine,
                    report
                        .Split('\n')
                        .Select(x => x.TrimEnd()))
                    .Trim();

                return report;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Azure OpenAI monthly compliance report generation failed.");

                return $"Monthly compliance report generation failed: {ex.Message}";
            }
        }

        private static async Task<string> BuildPromptFromFileAsync( string fileName, object payload)
        {
            var promptPath = Path.Combine(AppContext.BaseDirectory, "Prompts", fileName);

            if (!File.Exists(promptPath))
            {
                throw new FileNotFoundException($"Prompt file was not found: {fileName}", promptPath);
            }

            var template = await File.ReadAllTextAsync(promptPath);

            return template.Replace("{{DATA}}", JsonSerializer.Serialize(payload, new JsonSerializerOptions{WriteIndented = true}));
        }

        public async Task<string> GenerateRiskAssessmentAsync(object payload)
        {
            if (!IsConfigured())
            {
                return "Azure OpenAI is not configured.";
            }

            try
            {
                var prompt = await BuildPromptFromFileAsync("RiskAssessmentPrompt.txt", payload);

                var endpoint = _options.Endpoint.TrimEnd('/');
                var url = $"{endpoint}/openai/v1/chat/completions";

                using var request = new HttpRequestMessage(HttpMethod.Post, url);

                request.Headers.Add("api-key", _options.ApiKey);

                var body = new
                {
                    model = _options.DeploymentName,
                    messages = new[]
                    {
                    new
                    {
                        role = "system",
                        content =
                            "You are a senior TTB compliance reviewer specializing in risk prioritization."
                    },
                    new
                    {
                        role = "user",
                        content = prompt
                    }
                },
                    max_completion_tokens = 700,
                    temperature = 0.2
                };

                request.Content = JsonContent.Create(body);

                using var response = await _httpClient.SendAsync(request);

                var responseBody = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    throw new InvalidOperationException($"Azure OpenAI failed: {(int)response.StatusCode} {response.ReasonPhrase}. {responseBody}");
                }

                using var document = JsonDocument.Parse(responseBody);

                var riskAssessment = document
                    .RootElement
                    .GetProperty("choices")[0]
                    .GetProperty("message")
                    .GetProperty("content")
                    .GetString();

                if (string.IsNullOrWhiteSpace(riskAssessment))
                {
                    return "Azure OpenAI returned an empty risk assessment.";
                }

                riskAssessment = string.Join(Environment.NewLine, riskAssessment.Split('\n').Select(x => x.TrimEnd())).Trim();

                return riskAssessment;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "AI risk assessment generation failed.");

                return $"Risk assessment generation failed: {ex.Message}";
            }
        }

        public async Task<string> GenerateQueueRecommendationAsync(object payload)
        {
            if (!IsConfigured())
            {
                return "Azure OpenAI is not configured.";
            }

            try
            {
                var prompt = await BuildPromptFromFileAsync("QueueRecommendationPrompt.txt", payload);

                var endpoint = _options.Endpoint.TrimEnd('/');
                var url = $"{endpoint}/openai/v1/chat/completions";

                using var request = new HttpRequestMessage(HttpMethod.Post, url);

                request.Headers.Add("api-key", _options.ApiKey);

                var body = new
                {
                    model = _options.DeploymentName,
                    messages = new[]
                    {
                        new
                        {
                            role = "system",
                            content =
                                "You are a senior compliance operations manager. You provide queue management recommendations using only supplied operational data."
                        },
                        new
                        {
                            role = "user",
                            content = prompt
                        }
                    },
                    max_completion_tokens = 600,
                    temperature = 0.2
                };

                request.Content = JsonContent.Create(body);

                using var response = await _httpClient.SendAsync(request);

                var responseBody = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    throw new InvalidOperationException($"Azure OpenAI failed: {(int)response.StatusCode} {response.ReasonPhrase}. {responseBody}");
                }

                using var document = JsonDocument.Parse(responseBody);

                var recommendation = document
                    .RootElement
                    .GetProperty("choices")[0]
                    .GetProperty("message")
                    .GetProperty("content")
                    .GetString();

                if (string.IsNullOrWhiteSpace(recommendation))
                {
                    return "Azure OpenAI returned no queue recommendation.";
                }

                recommendation = string.Join(Environment.NewLine, recommendation.Split('\n').Select(x => x.TrimEnd())).Trim();

                return recommendation;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "AI queue recommendation generation failed.");

                return $"Queue recommendation unavailable: {ex.Message}";
            }
        }
    }
}