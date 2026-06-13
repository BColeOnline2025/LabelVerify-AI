using LabelVerify.Web.Data;
using LabelVerify.Web.Models;
using Microsoft.EntityFrameworkCore;

namespace LabelVerify.Web.Services
{
    public class ReviewQueryService
    {
        private readonly ApplicationDbContext _db;

        public ReviewQueryService(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<List<ReviewSession>> GetRecentAsync(int count = 100)
        {
            return await _db.ReviewSessions
                .OrderByDescending(x => x.ReviewDateUtc)
                .Take(count)
                .ToListAsync();
        }

        public async Task<ReviewSession?> GetByIdAsync(Guid reviewId)
        {
            return await _db.ReviewSessions
                .Include(x => x.Results)
                .FirstOrDefaultAsync(x => x.Id == reviewId);
        }

        public async Task<List<ReviewSession>> SearchAsync(ReviewSearchCriteria criteria)
        {
            var query = _db.ReviewSessions.AsQueryable();

            if (!string.IsNullOrWhiteSpace(criteria.Recommendation))
            {
                query = query.Where(x => x.Recommendation == criteria.Recommendation);
            }

            if (criteria.MinimumScore.HasValue)
            {
                query = query.Where(x => x.OverallScore >= criteria.MinimumScore.Value);
            }

            if (!string.IsNullOrWhiteSpace(criteria.ColaPackageFileName))
            {
                query = query.Where(x => x.ColaPackageFileName.Contains(criteria.ColaPackageFileName));
            }

            if (criteria.StartDate.HasValue)
            {
                var localStart = criteria.StartDate.Value.Date;

                var utcStart = localStart.ToUniversalTime();

                query = query.Where(x => x.ReviewDateUtc >= utcStart);
            }

            if (criteria.EndDate.HasValue)
            {
                var localEndExclusive = criteria.EndDate.Value.Date.AddDays(1);

                var utcEndExclusive = localEndExclusive.ToUniversalTime();

                query = query.Where(x => x.ReviewDateUtc < utcEndExclusive);
            }

            return await query
                .OrderByDescending(x => x.ReviewDateUtc)
                .ToListAsync();
        }

        public async Task<ReviewDashboardMetrics> GetMetricsAsync()
        {
            var reviews = await _db.ReviewSessions.ToListAsync();

            return new ReviewDashboardMetrics
            {
                TotalReviews = reviews.Count,
                ApprovedCount = reviews.Count(x => x.Recommendation == "Approve"),
                ReviewCount = reviews.Count(x => x.Recommendation == "Review"),
                RejectedCount = reviews.Count(x => x.Recommendation == "Reject" || x.Recommendation == "Fail"),
                AverageScore = reviews.Any() ? reviews.Average(x => x.OverallScore) : 0
            };
        }

        public async Task<List<FailureMetric>> GetTopFailureReasonsAsync(int count = 10)
        {
            return await _db.ReviewResults
                .Where(x => x.Status == "Fail")
                .GroupBy(x => x.FieldName)
                .Select(g => new FailureMetric
                {
                    FieldName = g.Key,
                    Count = g.Count()
                })
                .OrderByDescending(x => x.Count)
                .Take(count)
                .ToListAsync();
        }
    }
}