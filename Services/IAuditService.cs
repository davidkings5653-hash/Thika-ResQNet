using ThikaResQNet.Models;

namespace ThikaResQNet.Services
{
    public interface IAuditService
    {
        Task LogAsync(string action, string? performedBy = null, string? resource = null, string? details = null, string? ip = null, string? userAgent = null);
        Task<IEnumerable<AuditLog>> GetRecentAsync(int limit = 100);
    }
}