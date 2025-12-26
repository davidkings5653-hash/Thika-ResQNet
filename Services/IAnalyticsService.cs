using ThikaResQNet.DTOs;

namespace ThikaResQNet.Services
{
    public interface IAnalyticsService
    {
        Task<IEnumerable<ResponseTimeDto>> GetResponseTimesAsync(DateTime? start = null, DateTime? end = null);
        Task<MonthlyReportDto> GetMonthlyReportAsync(int year, int month);
        Task<IEnumerable<LocationSeverityStatsDto>> GetStatisticsByLocationAndSeverityAsync(DateTime? start = null, DateTime? end = null);
    }
}