using ThikaResQNet.Models;
using ThikaResQNet.Repositories;

namespace ThikaResQNet.Services
{
    public class AuditService : IAuditService
    {
        private readonly IAuditRepository _repo;

        public AuditService(IAuditRepository repo)
        {
            _repo = repo;
        }

        public async Task LogAsync(string action, string? performedBy = null, string? resource = null, string? details = null, string? ip = null, string? userAgent = null)
        {
            var log = new AuditLog
            {
                Action = action,
                PerformedBy = performedBy,
                Resource = resource,
                Details = details,
                IpAddress = ip,
                UserAgent = userAgent,
                PerformedAt = DateTime.UtcNow
            };
            await _repo.AddAsync(log);
        }

        public Task<IEnumerable<AuditLog>> GetRecentAsync(int limit = 100)
        {
            return _repo.GetRecentAsync(limit);
        }
    }
}