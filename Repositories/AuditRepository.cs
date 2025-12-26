using Microsoft.EntityFrameworkCore;
using ThikaResQNet.Data;
using ThikaResQNet.Models;

namespace ThikaResQNet.Repositories
{
    public class AuditRepository : IAuditRepository
    {
        private readonly AppDbContext _db;

        public AuditRepository(AppDbContext db)
        {
            _db = db;
        }

        public async Task AddAsync(AuditLog log)
        {
            _db.AuditLogs.Add(log);
            await _db.SaveChangesAsync();
        }

        public async Task<IEnumerable<AuditLog>> GetRecentAsync(int limit = 100)
        {
            return await _db.AuditLogs.OrderByDescending(a => a.PerformedAt).Take(limit).ToListAsync();
        }
    }
}