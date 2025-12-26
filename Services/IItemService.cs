using ThikaResQNet.DTOs;

namespace ThikaResQNet.Services
{
    public interface IItemService
    {
        Task<IEnumerable<ItemDto>> GetAllAsync();
        Task<ItemDto?> GetByIdAsync(int id);
        Task<ItemDto> CreateAsync(ItemDto dto);
        Task<bool> UpdateAsync(int id, ItemDto dto);
        Task<bool> DeleteAsync(int id);
    }
}