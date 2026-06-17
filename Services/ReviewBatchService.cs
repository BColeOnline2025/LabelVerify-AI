using LabelVerify.Web.Data;
using LabelVerify.Web.Models;
using Microsoft.EntityFrameworkCore;

namespace LabelVerify.Web.Services
{
    public class ReviewBatchService(ApplicationDbContext db)
    {
        private readonly ApplicationDbContext _db = db;

        public async Task<Guid> CreateBatchAsync(string batchName, string createdBy, IEnumerable<(string FileName, string BlobUrl)> colaPackages)
        {
            var batch = new ReviewBatch
            {
                Id = Guid.NewGuid(),
                BatchName = batchName,
                CreatedBy = createdBy,
                CreatedUtc = DateTime.UtcNow,
                Status = "Open"
            };

            foreach (var package in colaPackages)
            {
                batch.Packages.Add(new ReviewBatchPackage
                {
                    Id = Guid.NewGuid(),
                    ColaPackageFileName = package.FileName,
                    ColaPackageBlobUrl = package.BlobUrl,
                    Status = "Pending",
                    UploadedUtc = DateTime.UtcNow
                });
            }

            _db.ReviewBatches.Add(batch);

            await _db.SaveChangesAsync();

            return batch.Id;
        }

        public async Task<ReviewBatch?> GetBatchAsync(Guid id)
        {
            return await _db.ReviewBatches.Include(x => x.Packages).FirstOrDefaultAsync(x => x.Id == id);
        }

        public async Task<ReviewBatchPackage?> GetPackageAsync(Guid packageId)
        {
            return await _db.ReviewBatchPackages.Include(x => x.ReviewBatch).FirstOrDefaultAsync(x => x.Id == packageId);
        }

        public async Task MarkPackageCompletedAsync(Guid packageId, Guid reviewSessionId)
        {
            var package = await _db.ReviewBatchPackages.FirstOrDefaultAsync(x => x.Id == packageId);

            if (package == null)
            {
                return;
            }

            package.Status = "Completed";
            package.CompletedUtc = DateTime.UtcNow;
            package.ReviewSessionId = reviewSessionId;

            await _db.SaveChangesAsync();
        }

        public async Task<List<ReviewBatch>> GetBatchesAsync()
        {
            return await _db.ReviewBatches
                .Include(x => x.Packages)
                .OrderByDescending(x => x.CreatedUtc)
                .ToListAsync();
        }

        public async Task AssignReviewerAsync(Guid packageId, string reviewer)
        {
            var package = await _db.ReviewBatchPackages.FirstOrDefaultAsync(x => x.Id == packageId);

            if (package == null)
            {
                return;
            }

            package.AssignedReviewer = reviewer;
            package.AssignedUtc = DateTime.UtcNow;

            if (package.Status == "Pending")
            {
                package.Status = "Assigned";
            }

            await _db.SaveChangesAsync();
        }

        public async Task MarkInReviewAsync(Guid packageId)
        {
            var package = await _db.ReviewBatchPackages.FirstOrDefaultAsync(x => x.Id == packageId);

            if (package == null)
            {
                return;
            }

            if (package.Status == "Assigned")
            {
                package.Status = "In Review";

                await _db.SaveChangesAsync();
            }
        }

        public async Task<List<ReviewBatchPackage>>GetAssignedPackagesAsync(string reviewer)
        {
            return await _db.ReviewBatchPackages
                .Include(x => x.ReviewBatch)
                .Where(x =>
                    x.AssignedReviewer == reviewer &&
                    x.Status != "Completed")
                .OrderBy(x => x.UploadedUtc)
                .ToListAsync();
        }

        public async Task<int> GetMyWorkQueueCountAsync(string reviewer)
        {
            var batchPackageCount =
                await _db.ReviewBatchPackages
                    .CountAsync(x =>
                        x.AssignedReviewer == reviewer &&
                        x.Status != "Completed");

            var reviewSessionCount =
                await _db.ReviewSessions
                    .CountAsync(x =>
                        x.AssignedReviewer == reviewer &&
                        x.WorkflowStatus != "Completed" &&
                        x.WorkflowStatus != "Approved" &&
                        x.WorkflowStatus != "Rejected" &&
                        !x.CompletedUtc.HasValue);

            return batchPackageCount + reviewSessionCount;
        }
    }
}