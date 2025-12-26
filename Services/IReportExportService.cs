namespace ThikaResQNet.Services
{
    public interface IReportExportService
    {
        // Export incidents to CSV and return bytes
        Task<byte[]> ExportIncidentsToCsvAsync(DateTime? start = null, DateTime? end = null);

        // Export incidents to PDF and return bytes
        Task<byte[]> ExportIncidentsToPdfAsync(DateTime? start = null, DateTime? end = null);
    }
}