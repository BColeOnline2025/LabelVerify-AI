using LabelVerify.Web.Models;

namespace LabelVerify.Web.Services
{
    public class ProductionLabelExtractionService
    {
        public class ProductionLabelExtractionResult
        {
            public LabelFacts Facts { get; set; } = new();
            public Dictionary<string, string> FieldSources { get; set; } = [];
            public string CombinedOcrText { get; set; } = string.Empty;
        }
    }
}