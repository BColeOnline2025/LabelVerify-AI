using LabelVerify.Web.Models;
using LabelVerify.Web.Rules;

namespace LabelVerify.Web.Services
{
    public class LabelVerificationService
    {
        private readonly IEnumerable<ILabelRule> _rules;

        public LabelVerificationService(IEnumerable<ILabelRule> rules)
        {
            _rules = rules;
        }

        public VerificationResult Verify(
            LabelApplication application,
            string extractedText)
        {
            var checks = _rules
                .Select(rule => rule.Evaluate(application, extractedText))
                .ToList();

            var scoredChecks = checks
                .Where(check => !check.WasSkipped)
                .ToList();

            var overallScore = scoredChecks.Any()
                ? (int)Math.Round(scoredChecks.Average(check => check.ConfidenceScore))
                : 0;

            var hasFail = scoredChecks.Any(check =>
                string.Equals(check.Status, "Fail", StringComparison.OrdinalIgnoreCase));

            var hasReview = scoredChecks.Any(check =>
                string.Equals(check.Status, "Review", StringComparison.OrdinalIgnoreCase));

            var recommendation = hasFail
                ? "Reject"
                : hasReview
                    ? "Review"
                    : "Approve";

            return new VerificationResult
            {
                Checks = checks,
                OverallScore = overallScore,
                Recommendation = recommendation
            };
        }
    }
}