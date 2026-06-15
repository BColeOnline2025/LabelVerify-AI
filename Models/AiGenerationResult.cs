namespace LabelVerify.Web.Models
{
    public class AiGenerationResult
    {
        public string Summary { get; set; } = string.Empty;
        public string ModelUsed { get; set; } = string.Empty;
        public string PromptVersion { get; set; } = string.Empty;
        public int PromptTokens { get; set; }
        public int CompletionTokens { get; set; }
        public int TotalTokens { get; set; }
        public double GenerationTimeMs { get; set; }
        public DateTime GeneratedUtc { get; set; }
    }
}