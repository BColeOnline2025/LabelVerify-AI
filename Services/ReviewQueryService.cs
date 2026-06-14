using LabelVerify.Web.Data;
using LabelVerify.Web.Models;
using Microsoft.EntityFrameworkCore;

namespace LabelVerify.Web.Services
{
    public class ReviewQueryService(ApplicationDbContext db)
    {
        private readonly ApplicationDbContext _db = db;

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

            if (!string.IsNullOrWhiteSpace(criteria.WorkflowStatus))
            {
                query = query.Where(x => x.WorkflowStatus == criteria.WorkflowStatus);
            }

            if (!string.IsNullOrWhiteSpace(criteria.AssignedReviewer))
            {
                if (criteria.AssignedReviewer == "Unassigned")
                {
                    query = query.Where(x => string.IsNullOrEmpty(x.AssignedReviewer));
                }
                else
                {
                    query = query.Where(x => x.AssignedReviewer == criteria.AssignedReviewer);
                }
            }

            if (!string.IsNullOrWhiteSpace(criteria.FinalDisposition))
            {
                query = query.Where(x => x.FinalDisposition == criteria.FinalDisposition);
            }

            return await query
                .OrderByDescending(x => x.ReviewDateUtc)
                .ToListAsync();
        }

        public async Task<ReviewDashboardMetrics> GetMetricsAsync()
        {
            var reviews = await _db.ReviewSessions.ToListAsync();
            var now = DateTime.UtcNow;

            return new ReviewDashboardMetrics
            {
                TotalReviews = reviews.Count,
                ApprovedCount = reviews.Count(x => x.Recommendation == "Approve"),
                ReviewCount = reviews.Count(x => x.Recommendation == "Review"),
                RejectedCount = reviews.Count(x => x.Recommendation == "Reject" || x.Recommendation == "Fail"),
                AverageScore = reviews.Count != 0 ? reviews.Average(x => x.OverallScore) : 0,
                Submitted = reviews.Count(x => x.WorkflowStatus == "Submitted"),
                Assigned = reviews.Count(x => x.WorkflowStatus == "Assigned"),
                InReview = reviews.Count(x => x.WorkflowStatus == "In Review"),
                Approved = reviews.Count(x => x.WorkflowStatus == "Approved"),
                Rejected = reviews.Count(x => x.WorkflowStatus == "Rejected"),
                AgingOver7Days = reviews.Count(x => (now - x.ReviewDateUtc).Days > 7 &&
                    x.WorkflowStatus != "Approved" && x.WorkflowStatus != "Rejected"),
                AgingOver14Days = reviews.Count(x => (now - x.ReviewDateUtc).Days > 14 &&
                    x.WorkflowStatus != "Approved" && x.WorkflowStatus != "Rejected"),
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

        public async Task<PagedResult<ReviewSession>> SearchPagedAsync(ReviewSearchCriteria criteria, int pageNumber,int pageSize)
        {
            var query = _db.ReviewSessions.AsQueryable();

            if (!string.IsNullOrWhiteSpace(criteria.WorkflowStatus))
            {
                query = query.Where(x => x.WorkflowStatus == criteria.WorkflowStatus);
            }

            if (!string.IsNullOrWhiteSpace(criteria.AssignedReviewer))
            {
                if (criteria.AssignedReviewer == "Unassigned")
                {
                    query = query.Where(x => string.IsNullOrEmpty(x.AssignedReviewer));
                }
                else
                {
                    query = query.Where(x => x.AssignedReviewer == criteria.AssignedReviewer);
                }
            }

            if (!string.IsNullOrWhiteSpace(criteria.FinalDisposition))
            {
                query = query.Where(x => x.FinalDisposition == criteria.FinalDisposition);
            }

            var totalRecords = await query.CountAsync();

            var reviews = await query
                .OrderByDescending(x => x.ReviewDateUtc)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new PagedResult<ReviewSession>
            {
                Items = reviews,
                TotalRecords = totalRecords,
                PageNumber = pageNumber,
                PageSize = pageSize
            };
        }

        public async Task<List<ReviewAuditLog>> GetAuditLogAsync(Guid reviewId)
        {
            return await _db.ReviewAuditLogs
                .Where(x => x.ReviewSessionId == reviewId)
                .OrderByDescending(x => x.TimestampUtc)
                .ToListAsync();
        }

        public async Task UpdateDispositionAsync(Guid reviewId, string reviewerName,
            string disposition, string notes)
        {
            var review = await _db.ReviewSessions.FirstOrDefaultAsync(x => x.Id == reviewId);

            if (review == null)
            {
                return;
            }

            review.ReviewerName = reviewerName;
            review.ReviewerNotes = notes;
            review.FinalDisposition = disposition;
            review.DispositionDateUtc = DateTime.UtcNow;
            review.WorkflowStatus = disposition == "Approve" ? "Approved" : "Rejected";

            await _db.SaveChangesAsync();
        }

        public async Task SaveWorkflowStatusAsync(Guid reviewId, string status)
        {
            var review = await _db.ReviewSessions.FirstOrDefaultAsync(x => x.Id == reviewId);

            if (review == null)
            {
                return;
            }

            review.WorkflowStatus = status;

            await _db.SaveChangesAsync();
        }

        public async Task AssignReviewerAsync(Guid reviewId, string reviewer)
        {
            var review = await _db.ReviewSessions.FirstOrDefaultAsync(x => x.Id == reviewId);

            if (review == null)
            {
                return;
            }

            review.AssignedReviewer = reviewer;
            review.AssignedDateUtc = DateTime.UtcNow;

            if (review.WorkflowStatus == "Submitted")
            {
                review.WorkflowStatus = "Assigned";
            }

            await _db.SaveChangesAsync();
        }

        public async Task<ReviewDashboardMetrics> GetDashboardMetricsAsync()
        {
            return new ReviewDashboardMetrics
            {
                Submitted = await _db.ReviewSessions.CountAsync(x => x.WorkflowStatus == "Submitted"),

                Assigned = await _db.ReviewSessions.CountAsync(x => x.WorkflowStatus == "Assigned"),

                InReview = await _db.ReviewSessions.CountAsync(x => x.WorkflowStatus == "In Review"),

                Approved = await _db.ReviewSessions.CountAsync(x => x.WorkflowStatus == "Approved"),

                Rejected = await _db.ReviewSessions.CountAsync(x => x.WorkflowStatus == "Rejected")
            };
        }
    }
}