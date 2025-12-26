using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ThikaResQNet.DTOs;
using ThikaResQNet.Models;
using ThikaResQNet.Services;

namespace ThikaResQNet.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class EmergencyReportsController : ControllerBase
    {
        private readonly IIncidentService _incidentService;
        private readonly ISeverityService _severityService;
        private readonly IAuditService _auditService;

        public EmergencyReportsController(IIncidentService incidentService, ISeverityService severityService, IAuditService auditService)
        {
            _incidentService = incidentService;
            _severityService = severityService;
            _auditService = auditService;
        }

        // Allow anonymous submissions from public users
        [AllowAnonymous]
        [HttpPost]
        public async Task<IActionResult> Report([FromBody] EmergencyReportRequest req)
        {
            if (req == null) return BadRequest("Request body is required.");
            if (req.ReporterId <= 0) return BadRequest(new { message = "ReporterId is required" });
            if (string.IsNullOrWhiteSpace(req.Description)) return BadRequest(new { message = "Description is required" });

            var hasCoords = req.Latitude.HasValue && req.Longitude.HasValue;
            var hasAddress = !string.IsNullOrWhiteSpace(req.AddressText);
            if (!hasCoords && !hasAddress) return BadRequest(new { message = "Either GPS coordinates or AddressText must be provided" });

            // Validate severity if provided
            if (req.SeverityScore < 1 || req.SeverityScore > 10) return BadRequest(new { message = "SeverityScore must be between 1 and 10" });

            // Calculate severity using AI severity service based on description
            var level = _severityService.CalculateSeverity(req.Description);
            var calculatedScore = MapSeverityToScore(level);

            var dto = new IncidentDto
            {
                ReporterId = req.ReporterId,
                Description = req.Description,
                Latitude = req.Latitude,
                Longitude = req.Longitude,
                AddressText = req.AddressText,
                SeverityScore = calculatedScore,
                Status = "Open"
            };

            var created = await _incidentService.CreateAsync(dto);

            // Audit log the submission
            var performedBy = User?.Identity?.Name ?? req.ReporterId.ToString();
            var ip = HttpContext.Connection.RemoteIpAddress?.ToString();
            await _auditService.LogAsync("ReportSubmitted", performedBy, $"Incident:{created.IncidentId}", created.Description, ip, Request.Headers["User-Agent"].ToString());

            return CreatedAtAction(nameof(GetById), "Incidents", new { id = created.IncidentId }, created);
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

        // Helper route to fetch by id (proxies to Incidents controller)
        [AllowAnonymous]
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var item = await _incidentService.GetByIdAsync(id);
            if (item == null) return NotFound();
            return Ok(item);
        }
    }
}