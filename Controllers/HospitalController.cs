using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ThikaResQNet.DTOs;
using ThikaResQNet.Models;
using ThikaResQNet.Services;

namespace ThikaResQNet.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class HospitalController : ControllerBase
    {
        private readonly IHospitalService _hospitalService;
        private readonly IIncidentService _incidentService;
        private readonly IAuditService _auditService;

        public HospitalController(IHospitalService hospitalService, IIncidentService incidentService, IAuditService auditService)
        {
            _hospitalService = hospitalService;
            _incidentService = incidentService;
            _auditService = auditService;
        }

        // Dispatchers notify hospitals about incidents
        [Authorize(Roles = "Dispatcher,Admin")]
        [HttpPost("alerts")]
        public async Task<IActionResult> ReceiveAlert([FromBody] HospitalAlertRequest req)
        {
            if (req == null) return BadRequest("Request body required");
            var hospital = await _hospitalService.GetByIdAsync(req.HospitalId);
            if (hospital == null) return NotFound(new { message = "Hospital not found" });

            var incident = await _incidentService.GetByIdAsync(req.IncidentId);
            if (incident == null) return NotFound(new { message = "Incident not found" });

            // Optionally update incident status to InProgress to indicate hospital notified
            incident.Status = IncidentStatus.InProgress.ToString();
            await _incidentService.UpdateAsync(incident.IncidentId, incident);

            // Audit
            var performedBy = User?.Identity?.Name ?? "dispatcher";
            var ip = HttpContext.Connection.RemoteIpAddress?.ToString();
            await _auditService.LogAsync("HospitalAlert", performedBy, $"Hospital:{req.HospitalId}", $"Incident:{req.IncidentId} Note:{req.Note}", ip, Request.Headers["User-Agent"].ToString());

            // Return hospital + incident info to caller
            return Ok(new { hospital, incident, note = req.Note });
        }

        // Hospital admins update bed availability
        [Authorize(Roles = "HospitalAdmin,Admin")]
        [HttpPut("{hospitalId}/beds")]
        public async Task<IActionResult> UpdateBeds(int hospitalId, [FromBody] UpdateBedsRequest req)
        {
            var hospital = await _hospitalService.GetByIdAsync(hospitalId);
            if (hospital == null) return NotFound(new { message = "Hospital not found" });

            if (req.AvailableBeds < 0 || req.ICUCapacity < 0) return BadRequest(new { message = "Bed counts must be non-negative" });

            hospital.AvailableBeds = req.AvailableBeds;
            hospital.ICUCapacity = req.ICUCapacity;

            var ok = await _hospitalService.UpdateAsync(hospitalId, hospital);
            if (!ok) return StatusCode(500, new { message = "Unable to update hospital" });

            // Audit
            var performedBy = User?.Identity?.Name ?? "hospitaladmin";
            var ip = HttpContext.Connection.RemoteIpAddress?.ToString();
            await _auditService.LogAsync("HospitalBedsUpdated", performedBy, $"Hospital:{hospitalId}", $"Beds:{req.AvailableBeds} ICU:{req.ICUCapacity}", ip, Request.Headers["User-Agent"].ToString());

            return NoContent();
        }

        // Confirm patient arrival at hospital for an incident
        [Authorize(Roles = "HospitalAdmin,Admin")]
        [HttpPost("{hospitalId}/confirm-arrival")]
        public async Task<IActionResult> ConfirmArrival(int hospitalId, [FromBody] ConfirmArrivalRequest req)
        {
            if (req == null) return BadRequest("Request body required");

            var hospital = await _hospitalService.GetByIdAsync(hospitalId);
            if (hospital == null) return NotFound(new { message = "Hospital not found" });

            var incident = await _incidentService.GetByIdAsync(req.IncidentId);
            if (incident == null) return NotFound(new { message = "Incident not found" });

            // Ensure there is at least one available bed
            if (hospital.AvailableBeds <= 0) return BadRequest(new { message = "No available beds" });

            // Decrement bed count
            hospital.AvailableBeds = Math.Max(0, hospital.AvailableBeds - 1);
            var hospitalUpdated = await _hospitalService.UpdateAsync(hospitalId, hospital);
            if (!hospitalUpdated) return StatusCode(500, new { message = "Unable to update hospital beds" });

            // Update incident status to Resolved (or a more appropriate status)
            incident.Status = IncidentStatus.Resolved.ToString();
            await _incidentService.UpdateAsync(incident.IncidentId, incident);

            // Audit
            var performedBy = User?.Identity?.Name ?? "hospitaladmin";
            var ip = HttpContext.Connection.RemoteIpAddress?.ToString();
            await _auditService.LogAsync("PatientArrivalConfirmed", performedBy, $"Hospital:{hospitalId}", $"Incident:{req.IncidentId}", ip, Request.Headers["User-Agent"].ToString());

            return Ok(new { message = "Arrival confirmed", hospital, incident });
        }
    }

    public class HospitalAlertRequest
    {
        public int IncidentId { get; set; }
        public int HospitalId { get; set; }
        public string? Note { get; set; }
    }

    public class UpdateBedsRequest
    {
        public int AvailableBeds { get; set; }
        public int ICUCapacity { get; set; }
    }

    public class ConfirmArrivalRequest
    {
        public int IncidentId { get; set; }
    }
}