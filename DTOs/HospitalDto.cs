namespace ThikaResQNet.DTOs
{
    public class HospitalDto
    {
        public int HospitalId { get; set; }
        public string Name { get; set; }
        public string? Location { get; set; }
        public int AvailableBeds { get; set; }
        public int ICUCapacity { get; set; }
        public string? ContactNumber { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}