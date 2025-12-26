namespace ThikaResQNet.DTOs
{
    public class ResponseTimeDto
    {
        public int IncidentId { get; set; }
        public int? ResponderId { get; set; }
        public double ResponseMinutes { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? AssignedAt { get; set; }
    }

    public class MonthlyReportDto
    {
        public int Year { get; set; }
        public int Month { get; set; }
        public int TotalIncidents { get; set; }
        public int HighSeverity { get; set; }
        public int MediumSeverity { get; set; }
        public int LowSeverity { get; set; }
        public double AverageResponseMinutes { get; set; }
    }

    public class LocationSeverityStatsDto
    {
        public string Location { get; set; }
        public string Severity { get; set; }
        public int Count { get; set; }
    }
}