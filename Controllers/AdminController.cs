using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ThikaResQNet.Services;

namespace ThikaResQNet.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin")]
    public class AdminController : ControllerBase
    {
        private readonly IAnalyticsService _analytics;
        private readonly IReportExportService _export;

        public AdminController(IAnalyticsService analytics, IReportExportService export)
        {
            _analytics = analytics;
            _export = export;
        }

        [HttpGet("system-stats")]
        public async Task<IActionResult> SystemStats()
        {
            var now = DateTime.UtcNow;
            var report = await _analytics.GetMonthlyReportAsync(now.Year, now.Month);
            return Ok(report);
        }

        [HttpGet("export/csv")]
        public async Task<IActionResult> ExportCsv([FromQuery] DateTime? start, [FromQuery] DateTime? end)
        {
            var bytes = await _export.ExportIncidentsToCsvAsync(start, end);
            return File(bytes, "text/csv", $"incidents_{DateTime.UtcNow:yyyyMMddHHmm}.csv");
        }

        [HttpGet("export/pdf")]
        public async Task<IActionResult> ExportPdf([FromQuery] DateTime? start, [FromQuery] DateTime? end)
        {
            var bytes = await _export.ExportIncidentsToPdfAsync(start, end);
            return File(bytes, "application/pdf", $"incidents_{DateTime.UtcNow:yyyyMMddHHmm}.pdf");
        }
    }
}