using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ThikaResQNet.Models
{
    public class Hospital
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int HospitalId { get; set; }

        [Required]
        [MaxLength(200)]
        public string Name { get; set; }

        [MaxLength(500)]
        public string? Location { get; set; }

        // Number of available general beds
        public int AvailableBeds { get; set; } = 0;

        // Number of ICU beds
        public int ICUCapacity { get; set; } = 0;

        [Phone]
        [MaxLength(20)]
        public string? ContactNumber { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}