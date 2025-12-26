using ThikaResQNet.DTOs;

namespace ThikaResQNet.Services
{
    public interface IDispatchService
    {
        // Attempt to dispatch a responder to an incident
        // Returns the assigned responder id or null if no assignment
        Task<int?> DispatchToIncidentAsync(int incidentId);

        // Find nearest available responder given coordinates
        Task<int?> FindNearestAvailableResponderAsync(double latitude, double longitude);

        // Manually assign (or override) a responder to an incident
        Task<bool> AssignResponderAsync(int incidentId, int responderId, bool overrideExisting = false);
    }
}