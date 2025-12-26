using ThikaResQNet.Models;

namespace ThikaResQNet.DTOs
{
    public class ResponderDto
    {
        public int ResponderId { get; set; }
        public string VehicleNumber { get; set; }
        public ResponderStatus CurrentStatus { get; set; }
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
        public string PhoneNumber { get; set; }
    }
}