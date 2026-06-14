using LabelVerify.Web.Data;
using LabelVerify.Web.Models;
using Microsoft.EntityFrameworkCore;

namespace LabelVerify.Web.Services
{
    public class DashboardAnalyticsService(ApplicationDbContext db)
    {
        private readonly ApplicationDbContext _db = db;

        public async Task<Dictionary<string, int>>GetWorkflowStatusCountsAsync()
        {
            return await _db.ReviewSessions
                .GroupBy(x => x.WorkflowStatus)
                .Select(x => new
                {
                    Status = x.Key,
                    Count = x.Count()
                })
                .ToDictionaryAsync(
                    x => x.Status ?? "Unknown",
                    x => x.Count);
        }

        public async Task<ChartData> GetMonthlyReviewCountsAsync()
        {
            var rawData = await _db.ReviewSessions
                .GroupBy(x => new
                {
                    x.ReviewDateUtc.Year,
                    x.ReviewDateUtc.Month
                })
                .Select(g => new
                {
                    g.Key.Year,
                    g.Key.Month,
                    Count = g.Count()
                })
                .OrderBy(x => x.Year)
                .ThenBy(x => x.Month)
                .ToListAsync();

            return new ChartData
            {
                Labels = [.. rawData
                    .Select(x => new DateTime(x.Year, x.Month, 1).ToString("MMM yyyy"))],
                Values = [.. rawData.Select(x => x.Count)]
            };
        }

        public async Task<ChartData> GetTopFailureReasonsChartAsync()
        {
            var data = await _db.ReviewResults
                .Where(x => x.Status == "Fail")
                .GroupBy(x => x.FieldName)
                .Select(g => new
                {
                    FieldName = g.Key,
                    Count = g.Count()
                })
                .OrderByDescending(x => x.Count)
                .Take(10)
                .ToListAsync();

            return new ChartData
            {
                Labels = data.Select(x => x.FieldName).ToList(),
                Values = data.Select(x => x.Count).ToList()
            };
        }

        public async Task<ChartData> GetReviewerProductivityChartAsync()
        {
            var data = await _db.ReviewSessions
                .Where(x => !string.IsNullOrEmpty(x.ReviewerName) &&
                    (x.WorkflowStatus == "Approved" || x.WorkflowStatus == "Rejected"))
                .GroupBy(x => x.ReviewerName)
                .Select(g => new
                {
                    Reviewer = g.Key,
                    Count = g.Count()
                })
                .OrderByDescending(x => x.Count)
                .Take(10)
                .ToListAsync();

            return new ChartData
            {
                Labels = data.Select(x => x.Reviewer).ToList(),
                Values = data.Select(x => x.Count).ToList()
            };
        }

        public async Task<List<string>> GetOperationalInsightsAsync()
        {
            var insights = new List<string>();

            var reviews = await _db.ReviewSessions.ToListAsync();

            if (!reviews.Any())
            {
                return insights;
            }

            var approved = reviews.Count(x => x.WorkflowStatus == "Approved");
            var rejected = reviews.Count(x => x.WorkflowStatus == "Rejected");
            var completed = approved + rejected;

            if (completed > 0)
            {
                var approvalRate = (double)approved / completed * 100;

                insights.Add($"Approval rate is {approvalRate:N0}%.");
            }

            var activeReviews = reviews.Count(x => x.WorkflowStatus == "Submitted" ||
                x.WorkflowStatus == "Assigned" || x.WorkflowStatus == "In Review");

            insights.Add($"{activeReviews} active reviews currently require attention.");

            var staleReviews = reviews.Count(x => x.WorkflowStatus != "Approved" &&
                x.WorkflowStatus != "Rejected" && (DateTime.UtcNow - x.ReviewDateUtc).Days > 14);

            if (staleReviews > 0)
            {
                insights.Add($"{staleReviews} reviews exceed the 14-day SLA threshold.");
            }

            return insights;
        }
    }
}