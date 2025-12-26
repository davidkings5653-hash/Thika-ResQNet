using ThikaResQNet.DTOs;

namespace ThikaResQNet.Services
{
    public interface IResponderService
    {
        Task<IEnumerable<ResponderDto>> GetAllAsync();
        Task<ResponderDto?> GetByIdAsync(int id);
        Task<ResponderDto> CreateAsync(ResponderDto dto);
        Task<bool> UpdateAsync(int id, ResponderDto dto);
        Task<bool> DeleteAsync(int id);
    }
}