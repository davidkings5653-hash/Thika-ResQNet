using ThikaResQNet.DTOs;

namespace ThikaResQNet.Services
{
    public interface IHospitalService
    {
        Task<IEnumerable<HospitalDto>> GetAllAsync();
        Task<HospitalDto?> GetByIdAsync(int id);
        Task<HospitalDto> CreateAsync(HospitalDto dto);
        Task<bool> UpdateAsync(int id, HospitalDto dto);
        Task<bool> DeleteAsync(int id);
    }
}