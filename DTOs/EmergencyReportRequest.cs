namespace ThikaResQNet.DTOs
{
    public class EmergencyReportRequest
    {
        // ReporterId of the user submitting the report (required)
        public int ReporterId { get; set; }

        // Short description of the emergency
        public string Description { get; set; }

        // GPS coordinates (optional if AddressText provided)
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }

        // Manual address text (optional if coordinates provided)
        public string? AddressText { get; set; }

        // Optional severity score (1-10)
        public int SeverityScore { get; set; } = 1;
    }
}