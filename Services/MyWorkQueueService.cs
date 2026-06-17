using LabelVerify.Web.Data;
using LabelVerify.Web.Models;
using Microsoft.EntityFrameworkCore;

namespace LabelVerify.Web.Services
{
    public class MyWorkQueueService
    {
        private readonly ApplicationDbContext _db;

        public MyWorkQueueService(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<List<MyWorkQueueItem>> GetMyWorkQueueAsync(string reviewer)
        {
            if (string.IsNullOrWhiteSpace(reviewer))
            {
                return [];
            }

            var batchItems =
                await _db.ReviewBatchPackages
                    .Include(x => x.ReviewBatch)
                    .Where(x =>
                        x.AssignedReviewer == reviewer &&
                        x.Status != "Completed")
                    .Select(x => new MyWorkQueueItem
                    {
                        Id = x.Id,
                        SourceType = "Batch",
                        BatchName = x.ReviewBatch != null
                            ? x.ReviewBatch.BatchName
                            : null,
                        Title = x.ColaPackageFileName,
                        Status = x.Status,
                        CreatedUtc = x.UploadedUtc,
                        AssignedUtc = x.AssignedUtc,
                        ReturnBatchId = x.ReviewBatchId
                    })
                    .ToListAsync();

            var singleReviewItems =
                await _db.ReviewSessions
                    .Where(x =>
                        x.AssignedReviewer == reviewer &&
                        !x.CompletedUtc.HasValue &&
                        x.WorkflowStatus != "Completed" &&
                        x.WorkflowStatus != "Approved" &&
                        x.WorkflowStatus != "Rejected")
                    .Select(x => new MyWorkQueueItem
                    {
                        Id = x.Id,
                        SourceType = "Single",
                        BatchName = null,
                        Title = x.ColaPackageFileName,
                        Status = string.IsNullOrWhiteSpace(x.WorkflowStatus)
                            ? "Assigned"
                            : x.WorkflowStatus,
                        CreatedUtc = x.ReviewDateUtc,
                        AssignedUtc = x.AssignedUtc,
                        ReturnBatchId = null
                    })
                    .ToListAsync();

            return batchItems
                .Concat(singleReviewItems)
                .OrderBy(x => x.AssignedUtc ?? x.CreatedUtc)
                .ThenBy(x => x.Title)
                .ToList();
        }
    }
}