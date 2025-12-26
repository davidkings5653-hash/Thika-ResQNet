namespace ThikaResQNet.Services
{
    public interface ISeverityService
    {
        // Determine severity level from a free-text description
        // Returns one of: "High", "Medium", "Low"
        string CalculateSeverity(string description);
    }
}