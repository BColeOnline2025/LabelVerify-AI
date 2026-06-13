using LabelVerify.Web.Models;
using Microsoft.EntityFrameworkCore;

namespace LabelVerify.Web.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        public DbSet<ReviewSession> ReviewSessions => Set<ReviewSession>();

        public DbSet<ReviewResultEntity> ReviewResults => Set<ReviewResultEntity>();
    }
}
