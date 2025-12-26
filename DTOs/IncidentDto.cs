namespace ThikaResQNet.DTOs
{
    public class IncidentDto
    {
        public int IncidentId { get; set; }
        public int ReporterId { get; set; }
        public string Description { get; set; }
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
        public string? AddressText { get; set; }
        public int SeverityScore { get; set; }
        public string Status { get; set; }
        public DateTime CreatedAt { get; set; }

        // Assignment info
        public int? AssignedResponderId { get; set; }
        public DateTime? AssignedAt { get; set; }
    }
}