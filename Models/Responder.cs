using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ThikaResQNet.Models
{
    public enum ResponderStatus
    {
        Offline,
        Available,
        OnDuty,
        Busy
    }

    public class Responder
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ResponderId { get; set; }

        [Required]
        [MaxLength(50)]
        public string VehicleNumber { get; set; }

        [Required]
        public ResponderStatus CurrentStatus { get; set; } = ResponderStatus.Offline;

        public double? Latitude { get; set; }
        public double? Longitude { get; set; }

        [Required]
        [Phone]
        [MaxLength(20)]
        public string PhoneNumber { get; set; }
    }
}