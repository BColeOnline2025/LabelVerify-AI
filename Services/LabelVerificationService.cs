using LabelVerify.Web.Models;
using LabelVerify.Web.Rules;

namespace LabelVerify.Web.Services
{
    public class LabelVerificationService
    {
        private readonly IEnumerable<ILabelRule> _rules;

        public LabelVerificationService(
            IEnumerable<ILabelRule> rules)
        {
            _rules = rules;
        }

        public VerificationResult Verify(
            LabelApplication application,
            string extractedText)
        {
            var checks = _rules
                .Select(r => r.Evaluate(
                    application,
                    extractedText))
                .ToList();

            var score =
                checks.Any()
                ? (int)checks.Average(x => x.ConfidenceScore)
                : 0;

            return new VerificationResult
            {
                Checks = checks,
                OverallScore = score,
                Recommendation =
                    score >= 95
                        ? "Approve"
                        : score >= 80
                            ? "Review"
                            : "Reject"
            };
        }
    }
}