using ThikaResQNet.Models;

namespace ThikaResQNet.Repositories
{
    public interface IIncidentRepository
    {
        Task<IEnumerable<Incident>> GetAllAsync();
        Task<Incident?> GetByIdAsync(int id);
        Task<Incident> AddAsync(Incident incident);
        Task UpdateAsync(Incident incident);
        Task DeleteAsync(Incident incident);
    }
}