using LabelVerify.Web.Models;
using Microsoft.EntityFrameworkCore;

namespace LabelVerify.Web.Data
{
    public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : DbContext(options)
    {
        public DbSet<ReviewSession> ReviewSessions => Set<ReviewSession>();
        public DbSet<ReviewResultEntity> ReviewResults => Set<ReviewResultEntity>();
        public DbSet<ReviewAuditLog> ReviewAuditLogs => Set<ReviewAuditLog>();
    }
}