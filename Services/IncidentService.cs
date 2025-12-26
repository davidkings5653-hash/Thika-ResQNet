using ThikaResQNet.DTOs;
using ThikaResQNet.Models;
using ThikaResQNet.Repositories;

namespace ThikaResQNet.Services
{
    public class IncidentService : IIncidentService
    {
        private readonly IIncidentRepository _repo;
        private readonly ISeverityService _severityService;

        public IncidentService(IIncidentRepository repo, ISeverityService severityService)
        {
            _repo = repo;
            _severityService = severityService;
        }

        public async Task<IncidentDto> CreateAsync(IncidentDto dto)
        {
            // Determine severity level from description using the severity service
            var level = _severityService.CalculateSeverity(dto.Description ?? string.Empty);
            var calculatedScore = MapSeverityToScore(level);

            var model = new Incident
            {
                ReporterId = dto.ReporterId,
                Description = dto.Description,
                Latitude = dto.Latitude,
                Longitude = dto.Longitude,
                AddressText = dto.AddressText,
                SeverityScore = calculatedScore,
                Status = Enum.TryParse<IncidentStatus>(dto.Status, true, out var s) ? s : IncidentStatus.Open,
                AssignedResponderId = dto.AssignedResponderId,
                AssignedAt = dto.AssignedAt
            };
            var created = await _repo.AddAsync(model);
            dto.IncidentId = created.IncidentId;
            dto.CreatedAt = created.CreatedAt;
            dto.Status = created.Status.ToString();
            dto.SeverityScore = created.SeverityScore;
            dto.AssignedResponderId = created.AssignedResponderId;
            dto.AssignedAt = created.AssignedAt;
            return dto;
        }

        private static int MapSeverityToScore(string level)
        {
            return level?.ToLowerInvariant() switch
            {
                "high" => 9,
                "medium" => 5,
                "low" => 1,
                _ => 1
            };
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var model = await _repo.GetByIdAsync(id);
            if (model == null) return false;
            await _repo.DeleteAsync(model);
            return true;
        }

        public async Task<IEnumerable<IncidentDto>> GetAllAsync()
        {
            var items = await _repo.GetAllAsync();
            return items.Select(i => new IncidentDto
            {
                IncidentId = i.IncidentId,
                ReporterId = i.ReporterId,
                Description = i.Description,
                Latitude = i.Latitude,
                Longitude = i.Longitude,
                AddressText = i.AddressText,
                SeverityScore = i.SeverityScore,
                Status = i.Status.ToString(),
                CreatedAt = i.CreatedAt,
                AssignedResponderId = i.AssignedResponderId,
                AssignedAt = i.AssignedAt
            });
        }

        public async Task<IncidentDto?> GetByIdAsync(int id)
        {
            var i = await _repo.GetByIdAsync(id);
            if (i == null) return null;
            return new IncidentDto
            {
                IncidentId = i.IncidentId,
                ReporterId = i.ReporterId,
                Description = i.Description,
                Latitude = i.Latitude,
                Longitude = i.Longitude,
                AddressText = i.AddressText,
                SeverityScore = i.SeverityScore,
                Status = i.Status.ToString(),
                CreatedAt = i.CreatedAt,
                AssignedResponderId = i.AssignedResponderId,
                AssignedAt = i.AssignedAt
            };
        }

        public async Task<bool> UpdateAsync(int id, IncidentDto dto)
        {
            var model = await _repo.GetByIdAsync(id);
            if (model == null) return false;
            model.Description = dto.Description;
            model.Latitude = dto.Latitude;
            model.Longitude = dto.Longitude;
            model.AddressText = dto.AddressText;
            model.SeverityScore = dto.SeverityScore;
            if (Enum.TryParse<IncidentStatus>(dto.Status, true, out var s)) model.Status = s;
            model.AssignedResponderId = dto.AssignedResponderId;
            model.AssignedAt = dto.AssignedAt;
            await _repo.UpdateAsync(model);
            return true;
        }
    }
}