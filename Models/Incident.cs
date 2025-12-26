using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ThikaResQNet.Models
{
    public enum IncidentStatus
    {
        Open,
        InProgress,
        Resolved,
        Closed,
        Cancelled
    }

    public class Incident
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int IncidentId { get; set; }

        // Reporter (foreign key to User.UserId)
        [Required]
        public int ReporterId { get; set; }

        [ForeignKey(nameof(ReporterId))]
        public User? Reporter { get; set; }

        [Required]
        [MaxLength(2000)]
        public string Description { get; set; }

        // Geographic coordinates
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }

        [MaxLength(1024)]
        public string? AddressText { get; set; }

        // Severity score (e.g., 1-10)
        public int SeverityScore { get; set; } = 1;

        public IncidentStatus Status { get; set; } = IncidentStatus.Open;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Assignment fields
        public int? AssignedResponderId { get; set; }
        public DateTime? AssignedAt { get; set; }

        [ForeignKey(nameof(AssignedResponderId))]
        public Responder? AssignedResponder { get; set; }
    }
}