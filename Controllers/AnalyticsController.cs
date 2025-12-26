using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ThikaResQNet.Services;

namespace ThikaResQNet.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin,Dispatcher,HospitalAdmin")]
    public class AnalyticsController : ControllerBase
    {
        private readonly IAnalyticsService _analytics;

        public AnalyticsController(IAnalyticsService analytics)
        {
            _analytics = analytics;
        }

        [HttpGet("response-times")]
        public async Task<IActionResult> ResponseTimes([FromQuery] DateTime? start, [FromQuery] DateTime? end)
        {
            var list = await _analytics.GetResponseTimesAsync(start, end);
            return Ok(list);
        }

        [HttpGet("monthly-report")]
        public async Task<IActionResult> MonthlyReport([FromQuery] int year, [FromQuery] int month)
        {
            var report = await _analytics.GetMonthlyReportAsync(year, month);
            return Ok(report);
        }

        [HttpGet("stats-by-location")]
        public async Task<IActionResult> StatsByLocation([FromQuery] DateTime? start, [FromQuery] DateTime? end)
        {
            var stats = await _analytics.GetStatisticsByLocationAndSeverityAsync(start, end);
            return Ok(stats);
        }
    }
}