namespace LabelVerify.Web.Models
{
    public class RiskAssessment
    {
        public int RiskScore { get; set; }
        public string RiskLevel { get; set; } = "";
        public string RiskFactors { get; set; } = "";
    }
}