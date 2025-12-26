using ThikaResQNet.Data;
using ThikaResQNet.Models;
using ThikaResQNet.Repositories;
using Microsoft.EntityFrameworkCore;

namespace ThikaResQNet.Services
{
    public class DispatchService : IDispatchService
    {
        private readonly IIncidentRepository _incidentRepo;
        private readonly IResponderRepository _responderRepo;
        private readonly AppDbContext _db;

        public DispatchService(IIncidentRepository incidentRepo, IResponderRepository responderRepo, AppDbContext db)
        {
            _incidentRepo = incidentRepo;
            _responderRepo = responderRepo;
            _db = db;
        }

        public async Task<int?> FindNearestAvailableResponderAsync(double latitude, double longitude)
        {
            var responders = await _db.Responders.Where(r => r.CurrentStatus == ResponderStatus.Available).ToListAsync();
            if (!responders.Any()) return null;

            double minDist = double.MaxValue;
            Responder? best = null;

            foreach (var r in responders)
            {
                if (!r.Latitude.HasValue || !r.Longitude.HasValue) continue;
                var d = HaversineDistance(latitude, longitude, r.Latitude.Value, r.Longitude.Value);
                if (d < minDist)
                {
                    minDist = d;
                    best = r;
                }
            }

            return best?.ResponderId;
        }

        public async Task<int?> DispatchToIncidentAsync(int incidentId)
        {
            var incident = await _incidentRepo.GetByIdAsync(incidentId);
            if (incident == null) return null;

            // Prioritize high severity incidents
            // If incident is high severity (severity >= 8), ensure it's assigned first
            var isHigh = incident.SeverityScore >= 8;

            // Find candidate responders
            var responders = await _db.Responders.Where(r => r.CurrentStatus == ResponderStatus.Available).ToListAsync();
            if (!responders.Any()) return null;

            // If we want to select nearest responder
            Responder? selected = null;
            double minDist = double.MaxValue;

            foreach (var r in responders)
            {
                if (!r.Latitude.HasValue || !r.Longitude.HasValue) continue;
                if (!incident.Latitude.HasValue || !incident.Longitude.HasValue) continue;
                var d = HaversineDistance(incident.Latitude.Value, incident.Longitude.Value, r.Latitude.Value, r.Longitude.Value);
                if (d < minDist)
                {
                    minDist = d;
                    selected = r;
                }
            }

            if (selected == null) return null;

            // Assign selected responder
            incident.AssignedResponderId = selected.ResponderId;
            incident.AssignedAt = DateTime.UtcNow;
            incident.Status = IncidentStatus.InProgress;

            // mark responder as OnDuty
            selected.CurrentStatus = ResponderStatus.OnDuty;

            // Save changes
            _db.Incidents.Update(incident);
            _db.Responders.Update(selected);
            await _db.SaveChangesAsync();

            return selected.ResponderId;
        }

        public async Task<bool> AssignResponderAsync(int incidentId, int responderId, bool overrideExisting = false)
        {
            var incident = await _incidentRepo.GetByIdAsync(incidentId);
            if (incident == null) return false;

            var responder = await _responderRepo.GetByIdAsync(responderId);
            if (responder == null) return false;

            // If not overriding and incident already has an assigned responder, fail
            if (!overrideExisting && incident.AssignedResponderId.HasValue) return false;

            // If responder not available, fail unless overrideExisting true
            if (!overrideExisting && responder.CurrentStatus != ResponderStatus.Available) return false;

            // Assign
            incident.AssignedResponderId = responder.ResponderId;
            incident.AssignedAt = DateTime.UtcNow;
            incident.Status = IncidentStatus.InProgress;

            // Update responder status
            responder.CurrentStatus = ResponderStatus.OnDuty;

            _db.Incidents.Update(incident);
            _db.Responders.Update(responder);
            await _db.SaveChangesAsync();

            return true;
        }

        private static double HaversineDistance(double lat1, double lon1, double lat2, double lon2)
        {
            const double R = 6371e3; // metres
            var phi1 = DegreeToRadian(lat1);
            var phi2 = DegreeToRadian(lat2);
            var deltaPhi = DegreeToRadian(lat2 - lat1);
            var deltaLambda = DegreeToRadian(lon2 - lon1);

            var a = Math.Sin(deltaPhi / 2) * Math.Sin(deltaPhi / 2) +
                    Math.Cos(phi1) * Math.Cos(phi2) *
                    Math.Sin(deltaLambda / 2) * Math.Sin(deltaLambda / 2);
            var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));

            var d = R * c; // in metres
            return d;
        }

        private static double DegreeToRadian(double deg) => deg * (Math.PI / 180.0);
    }
}