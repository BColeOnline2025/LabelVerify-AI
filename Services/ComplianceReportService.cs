using LabelVerify.Web.Models;

namespace LabelVerify.Web.Services
{
    public class ComplianceReportService
    {
        public ComplianceAuditReport Create(ApprovedProductProfile approved, LabelFacts production,
            VerificationResult result, IEnumerable<string> uploadedFiles)
        {
            return new ComplianceAuditReport
            {
                ReviewDate = DateTime.UtcNow,
                Recommendation = result.Recommendation,
                OverallScore = result.OverallScore,
                ApprovedProfile = approved,
                ProductionFacts = production,
                VerificationResult = result,
                UploadedLabels = [.. uploadedFiles]
            };
        }
    }
}