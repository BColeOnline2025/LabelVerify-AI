using LabelVerify.Web.Models;

namespace LabelVerify.Web.Services
{
    public class ReviewerCommentaryService(AzureOpenAiSummaryService azureOpenAiSummaryService)
    {
        private readonly AzureOpenAiSummaryService _azureOpenAiSummaryService = azureOpenAiSummaryService;

        public async Task<string> GenerateAsync(VerificationResult result)
        {
            var findings = result.Checks
                .Where(x => x.Status != "Pass")
                .Select(x =>
                    $"{x.FieldName}: {x.Status}. Expected: {x.ExpectedValue}. Actual: {x.ActualValue}. Notes: {x.Notes}")
                .ToList();

            if (findings.Count == 0)
            {
                return "No material reviewer commentary required. No failed or review-level findings were detected.";
            }

            var prompt =
                "You are a senior TTB Label Specialist.\n\n" +
                "Review the findings below.\n\n" +
                "Do not simply restate the findings. Explain why they are significant, describe likely regulatory implications, and indicate whether approval should be delayed, returned for correction, or rejected.\n\n" +
                "Findings:\n" +
                string.Join("\n", findings);

            return await _azureOpenAiSummaryService.GenerateCustomPromptAsync(
                "You are a senior TTB Label Specialist reviewing alcohol beverage label compliance findings.",
                prompt,
                700);
        }
    }
}