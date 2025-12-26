using Microsoft.EntityFrameworkCore;
using ThikaResQNet.Models;

namespace ThikaResQNet.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<Item> Items { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<Incident> Incidents { get; set; }
        public DbSet<Responder> Responders { get; set; }
        public DbSet<Hospital> Hospitals { get; set; }
        public DbSet<AuditLog> AuditLogs { get; set; }
    }
}