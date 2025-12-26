using ThikaResQNet.DTOs;
using ThikaResQNet.Models;
using ThikaResQNet.Repositories;

namespace ThikaResQNet.Services
{
    public class ItemService : IItemService
    {
        private readonly IItemRepository _repo;

        public ItemService(IItemRepository repo)
        {
            _repo = repo;
        }

        public async Task<ItemDto> CreateAsync(ItemDto dto)
        {
            var item = new Item
            {
                Name = dto.Name,
                Description = dto.Description,
                CreatedAt = dto.CreatedAt == default ? DateTime.UtcNow : dto.CreatedAt
            };

            var created = await _repo.AddAsync(item);
            dto.Id = created.Id;
            dto.CreatedAt = created.CreatedAt;
            return dto;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var item = await _repo.GetByIdAsync(id);
            if (item == null) return false;
            await _repo.DeleteAsync(item);
            return true;
        }

        public async Task<IEnumerable<ItemDto>> GetAllAsync()
        {
            var items = await _repo.GetAllAsync();
            return items.Select(i => new ItemDto { Id = i.Id, Name = i.Name, Description = i.Description, CreatedAt = i.CreatedAt });
        }

        public async Task<ItemDto?> GetByIdAsync(int id)
        {
            var i = await _repo.GetByIdAsync(id);
            if (i == null) return null;
            return new ItemDto { Id = i.Id, Name = i.Name, Description = i.Description, CreatedAt = i.CreatedAt };
        }

        public async Task<bool> UpdateAsync(int id, ItemDto dto)
        {
            var item = await _repo.GetByIdAsync(id);
            if (item == null) return false;
            item.Name = dto.Name;
            item.Description = dto.Description;
            await _repo.UpdateAsync(item);
            return true;
        }
    }
}