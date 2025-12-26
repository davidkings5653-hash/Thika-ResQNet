using ThikaResQNet.DTOs;

namespace ThikaResQNet.Services
{
    public interface IIncidentService
    {
        Task<IEnumerable<IncidentDto>> GetAllAsync();
        Task<IncidentDto?> GetByIdAsync(int id);
        Task<IncidentDto> CreateAsync(IncidentDto dto);
        Task<bool> UpdateAsync(int id, IncidentDto dto);
        Task<bool> DeleteAsync(int id);
    }
}