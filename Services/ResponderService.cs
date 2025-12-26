using ThikaResQNet.DTOs;
using ThikaResQNet.Models;
using ThikaResQNet.Repositories;

namespace ThikaResQNet.Services
{
    public class ResponderService : IResponderService
    {
        private readonly IResponderRepository _repo;

        public ResponderService(IResponderRepository repo)
        {
            _repo = repo;
        }

        public async Task<ResponderDto> CreateAsync(ResponderDto dto)
        {
            var model = new Responder
            {
                VehicleNumber = dto.VehicleNumber,
                CurrentStatus = dto.CurrentStatus,
                Latitude = dto.Latitude,
                Longitude = dto.Longitude,
                PhoneNumber = dto.PhoneNumber
            };
            var created = await _repo.AddAsync(model);
            dto.ResponderId = created.ResponderId;
            return dto;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var model = await _repo.GetByIdAsync(id);
            if (model == null) return false;
            await _repo.DeleteAsync(model);
            return true;
        }

        public async Task<IEnumerable<ResponderDto>> GetAllAsync()
        {
            var items = await _repo.GetAllAsync();
            return items.Select(i => new ResponderDto
            {
                ResponderId = i.ResponderId,
                VehicleNumber = i.VehicleNumber,
                CurrentStatus = i.CurrentStatus,
                Latitude = i.Latitude,
                Longitude = i.Longitude,
                PhoneNumber = i.PhoneNumber
            });
        }

        public async Task<ResponderDto?> GetByIdAsync(int id)
        {
            var i = await _repo.GetByIdAsync(id);
            if (i == null) return null;
            return new ResponderDto
            {
                ResponderId = i.ResponderId,
                VehicleNumber = i.VehicleNumber,
                CurrentStatus = i.CurrentStatus,
                Latitude = i.Latitude,
                Longitude = i.Longitude,
                PhoneNumber = i.PhoneNumber
            };
        }

        public async Task<bool> UpdateAsync(int id, ResponderDto dto)
        {
            var model = await _repo.GetByIdAsync(id);
            if (model == null) return false;
            model.VehicleNumber = dto.VehicleNumber;
            model.CurrentStatus = dto.CurrentStatus;
            model.Latitude = dto.Latitude;
            model.Longitude = dto.Longitude;
            model.PhoneNumber = dto.PhoneNumber;
            await _repo.UpdateAsync(model);
            return true;
        }
    }
}