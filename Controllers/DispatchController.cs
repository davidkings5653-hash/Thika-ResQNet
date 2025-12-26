using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ThikaResQNet.Services;
using ThikaResQNet.DTOs;

namespace ThikaResQNet.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Dispatcher,Admin")]
    public class DispatchController : ControllerBase
    {
        private readonly IDispatchService _dispatchService;
        private readonly IIncidentService _incidentService;
        private readonly IAuditService _auditService;

        public DispatchController(IDispatchService dispatchService, IIncidentService incidentService, IAuditService auditService)
        {
            _dispatchService = dispatchService;
            _incidentService = incidentService;
            _auditService = auditService;
        }

        [HttpGet("active-incidents")]
        public async Task<IActionResult> GetActiveIncidents()
        {
            var all = await _incidentService.GetAllAsync();
            var active = all.Where(i => i.Status == "Open" || i.Status == "InProgress");
            return Ok(active);
        }

        [HttpPost("assign")]
        public async Task<IActionResult> Assign([FromBody] ManualAssignRequest req)
        {
            if (req == null) return BadRequest();
            var ok = await _dispatchService.AssignResponderAsync(req.IncidentId, req.ResponderId, req.OverrideExisting);
            if (!ok) return BadRequest(new { message = "Unable to assign responder" });

            var performedBy = User?.Identity?.Name ?? "dispatcher";
            var ip = HttpContext.Connection.RemoteIpAddress?.ToString();
            await _auditService.LogAsync("DispatchAssigned", performedBy, $"Incident:{req.IncidentId}", $"Responder:{req.ResponderId}", ip, Request.Headers["User-Agent"].ToString());

            return Ok(new { message = "Assigned" });
        }
    }

    public class ManualAssignRequest
    {
        public int IncidentId { get; set; }
        public int ResponderId { get; set; }
        public bool OverrideExisting { get; set; } = false;
    }
}