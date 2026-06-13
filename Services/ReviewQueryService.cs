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
    }
}