using LabelVerify.Web.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;

namespace LabelVerify.Web.Data
{
    public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : IdentityDbContext<Models.ApplicationUser>(options)
    {
        public DbSet<ReviewSession> ReviewSessions => Set<ReviewSession>();
        public DbSet<ReviewResultEntity> ReviewResults => Set<ReviewResultEntity>();
        public DbSet<ReviewAuditLog> ReviewAuditLogs => Set<ReviewAuditLog>();
    }
}