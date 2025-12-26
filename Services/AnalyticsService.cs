using Microsoft.EntityFrameworkCore;
using ThikaResQNet.Data;
using ThikaResQNet.DTOs;
using ThikaResQNet.Models;

namespace ThikaResQNet.Services
{
    public class AnalyticsService : IAnalyticsService
    {
        private readonly AppDbContext _db;

        public AnalyticsService(AppDbContext db)
        {
            _db = db;
        }

        public async Task<IEnumerable<ResponseTimeDto>> GetResponseTimesAsync(DateTime? start = null, DateTime? end = null)
        {
            var query = _db.Incidents.AsQueryable();
            if (start.HasValue) query = query.Where(i => i.CreatedAt >= start.Value);
            if (end.HasValue) query = query.Where(i => i.CreatedAt <= end.Value);

            var list = await query
                .Where(i => i.AssignedAt.HasValue)
                .Select(i => new ResponseTimeDto
                {
                    IncidentId = i.IncidentId,
                    ResponderId = i.AssignedResponderId,
                    CreatedAt = i.CreatedAt,
                    AssignedAt = i.AssignedAt,
                    ResponseMinutes = EF.Functions.DateDiffMinute(i.CreatedAt, i.AssignedAt.Value)
                }).ToListAsync();

            return list;
        }

        public async Task<MonthlyReportDto> GetMonthlyReportAsync(int year, int month)
        {
            var start = new DateTime(year, month, 1);
            var end = start.AddMonths(1).AddSeconds(-1);
            var incidents = await _db.Incidents.Where(i => i.CreatedAt >= start && i.CreatedAt <= end).ToListAsync();

            var total = incidents.Count;
            var high = incidents.Count(i => i.SeverityScore >= 8);
            var medium = incidents.Count(i => i.SeverityScore >= 4 && i.SeverityScore < 8);
            var low = incidents.Count(i => i.SeverityScore < 4);

            var avgResp = incidents.Where(i => i.AssignedAt.HasValue).Any()
                ? incidents.Where(i => i.AssignedAt.HasValue).Average(i => EF.Functions.DateDiffMinute(i.CreatedAt, i.AssignedAt.Value))
                : 0.0;

            return new MonthlyReportDto
            {
                Year = year,
                Month = month,
                TotalIncidents = total,
                HighSeverity = high,
                MediumSeverity = medium,
                LowSeverity = low,
                AverageResponseMinutes = avgResp
            };
        }

        public async Task<IEnumerable<LocationSeverityStatsDto>> GetStatisticsByLocationAndSeverityAsync(DateTime? start = null, DateTime? end = null)
        {
            var query = _db.Incidents.AsQueryable();
            if (start.HasValue) query = query.Where(i => i.CreatedAt >= start.Value);
            if (end.HasValue) query = query.Where(i => i.CreatedAt <= end.Value);

            var grouped = await query
                .GroupBy(i => new { Location = i.AddressText ?? "Unknown", Severity = GetSeverityLabel(i.SeverityScore) })
                .Select(g => new LocationSeverityStatsDto
                {
                    Location = g.Key.Location,
                    Severity = g.Key.Severity,
                    Count = g.Count()
                }).ToListAsync();

            return grouped;
        }

        private static string GetSeverityLabel(int score)
        {
            if (score >= 8) return "High";
            if (score >= 4) return "Medium";
            return "Low";
        }
    }
}