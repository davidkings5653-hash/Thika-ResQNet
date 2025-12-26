using ThikaResQNet.Models;
using System.Text.Json.Serialization;

namespace ThikaResQNet.DTOs
{
    public class UserDto
    {
        public int UserId { get; set; }
        public string FullName { get; set; }
        public string PhoneNumber { get; set; }

        [JsonConverter(typeof(JsonStringEnumConverter))]
        public UserRole Role { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}