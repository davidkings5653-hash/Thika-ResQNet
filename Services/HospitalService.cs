using ThikaResQNet.DTOs;
using ThikaResQNet.Models;
using ThikaResQNet.Repositories;

namespace ThikaResQNet.Services
{
    public class HospitalService : IHospitalService
    {
        private readonly IHospitalRepository _repo;

        public HospitalService(IHospitalRepository repo)
        {
            _repo = repo;
        }

        public async Task<HospitalDto> CreateAsync(HospitalDto dto)
        {
            var model = new Hospital
            {
                Name = dto.Name,
                Location = dto.Location,
                AvailableBeds = dto.AvailableBeds,
                ICUCapacity = dto.ICUCapacity,
                ContactNumber = dto.ContactNumber
            };
            var created = await _repo.AddAsync(model);
            dto.HospitalId = created.HospitalId;
            dto.CreatedAt = created.CreatedAt;
            return dto;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var model = await _repo.GetByIdAsync(id);
            if (model == null) return false;
            await _repo.DeleteAsync(model);
            return true;
        }

        public async Task<IEnumerable<HospitalDto>> GetAllAsync()
        {
            var items = await _repo.GetAllAsync();
            return items.Select(i => new HospitalDto
            {
                HospitalId = i.HospitalId,
                Name = i.Name,
                Location = i.Location,
                AvailableBeds = i.AvailableBeds,
                ICUCapacity = i.ICUCapacity,
                ContactNumber = i.ContactNumber,
                CreatedAt = i.CreatedAt
            });
        }

        public async Task<HospitalDto?> GetByIdAsync(int id)
        {
            var i = await _repo.GetByIdAsync(id);
            if (i == null) return null;
            return new HospitalDto
            {
                HospitalId = i.HospitalId,
                Name = i.Name,
                Location = i.Location,
                AvailableBeds = i.AvailableBeds,
                ICUCapacity = i.ICUCapacity,
                ContactNumber = i.ContactNumber,
                CreatedAt = i.CreatedAt
            };
        }

        public async Task<bool> UpdateAsync(int id, HospitalDto dto)
        {
            var model = await _repo.GetByIdAsync(id);
            if (model == null) return false;
            model.Name = dto.Name;
            model.Location = dto.Location;
            model.AvailableBeds = dto.AvailableBeds;
            model.ICUCapacity = dto.ICUCapacity;
            model.ContactNumber = dto.ContactNumber;
            await _repo.UpdateAsync(model);
            return true;
        }
    }
}