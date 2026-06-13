namespace LabelVerify.Web.Models
{
    public class LabelFactsSource
    {
        public LabelFacts Facts { get; set; } = new();
        public Dictionary<string, string> FieldSources { get; set; } = [];
    }
}