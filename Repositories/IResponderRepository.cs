using ThikaResQNet.Models;

namespace ThikaResQNet.Repositories
{
    public interface IResponderRepository
    {
        Task<IEnumerable<Responder>> GetAllAsync();
        Task<Responder?> GetByIdAsync(int id);
        Task<Responder> AddAsync(Responder responder);
        Task UpdateAsync(Responder responder);
        Task DeleteAsync(Responder responder);
    }
}