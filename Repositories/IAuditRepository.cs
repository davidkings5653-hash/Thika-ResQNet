using ThikaResQNet.Models;

namespace ThikaResQNet.Repositories
{
    public interface IAuditRepository
    {
        Task AddAsync(AuditLog log);
        Task<IEnumerable<AuditLog>> GetRecentAsync(int limit = 100);
    }
}