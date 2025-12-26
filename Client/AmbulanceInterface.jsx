import React, { useEffect, useState, useRef } from "react";
import 'bootstrap/dist/css/bootstrap.min.css';
import MobileNavbar from './MobileNavbar';

/*
Create a page for ambulance responders.
Show assigned incidents with navigation info.
Include map integration (Google Maps API) to show route to incident.
Display patient details and hospital ETA.
Include buttons to mark 'En Route' and 'Arrived'.
*/

// AmbulanceInterface component
// Shows incidents assigned to this responder, provides navigation links and status updates
export default function AmbulanceInterface() {
  const [assignedIncidents, setAssignedIncidents] = useState([]);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState(null);
  const intervalRef = useRef(null);

  // Try to obtain responder id from local storage (set after login) or token claims
  const responderId = parseInt(localStorage.getItem('responderId') || '0', 10) || null;

  const fetchAssigned = async () => {
    setLoading(true);
    setError(null);
    try {
      const token = localStorage.getItem('thika_token');
      // Using /api/incidents endpoint and filtering client-side for assignedResponderId
      const res = await fetch('/api/incidents', {
        headers: {
          'Content-Type': 'application/json',
          ...(token ? { Authorization: `Bearer ${token}` } : {})
        }
      });
      if (!res.ok) {
        const body = await res.json().catch(() => ({}));
        setError(body?.message || `Error ${res.status}`);
        setAssignedIncidents([]);
      } else {
        const data = await res.json();
        const arr = Array.isArray(data) ? data : [];
        const assigned = arr.filter(i => Number(i.assignedResponderId) === responderId);
        setAssignedIncidents(assigned);
      }
    } catch (err) {
      setError('Network error');
      setAssignedIncidents([]);
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    fetchAssigned();
    intervalRef.current = setInterval(fetchAssigned, 15000); // refresh every 15s
    return () => clearInterval(intervalRef.current);
  }, [responderId]);

  // Helper: estimate ETA (minutes) using straight-line distance and average speed (km/h)
  const estimateEtaMinutes = (rLat, rLon, lat, lon, speedKmH = 40) => {
    if (!rLat || !rLon || !lat || !lon) return null;
    const R = 6371; // km
    const toRad = v => v * Math.PI / 180;
    const dLat = toRad(lat - rLat);
    const dLon = toRad(lon - rLon);
    const a = Math.sin(dLat/2) * Math.sin(dLat/2) + Math.cos(toRad(rLat)) * Math.cos(toRad(lat)) * Math.sin(dLon/2) * Math.sin(dLon/2);
    const c = 2 * Math.atan2(Math.sqrt(a), Math.sqrt(1-a));
    const d = R * c; // km
    const hours = d / speedKmH;
    return Math.max(1, Math.round(hours * 60));
  };

  const openNavigation = (incident) => {
    // Use current device location as origin if available, otherwise responder location not known
    const dest = incident.latitude && incident.longitude ? `${incident.latitude},${incident.longitude}` : encodeURIComponent(incident.addressText || '');
    // Google Maps directions link; recommend opening in maps app on mobile
    const url = `https://www.google.com/maps/dir/?api=1&destination=${dest}`;
    window.open(url, '_blank');
  };

  const updateStatus = async (incident, newStatus) => {
    try {
      const token = localStorage.getItem('thika_token');
      const incidentId = incident.incidentId ?? incident.id;
      // Fetch full incident, update status, then PUT
      const getRes = await fetch(`/api/incidents/${incidentId}`, {
        headers: { 'Content-Type': 'application/json', ...(token ? { Authorization: `Bearer ${token}` } : {}) }
      });
      if (!getRes.ok) { alert('Unable to load incident'); return; }
      const full = await getRes.json();
      full.status = newStatus;
      const putRes = await fetch(`/api/incidents/${incidentId}`, {
        method: 'PUT',
        headers: { 'Content-Type': 'application/json', ...(token ? { Authorization: `Bearer ${token}` } : {}) },
        body: JSON.stringify(full)
      });
      if (!putRes.ok) {
        const body = await putRes.json().catch(() => ({}));
        alert(body?.message || `Update failed: ${putRes.status}`);
      } else {
        alert(`Incident marked '${newStatus}'`);
        fetchAssigned();
      }
    } catch (err) {
      alert('Network error');
    }
  };

  return (
    <div>
      <MobileNavbar />
      <div className="container mt-4">
        <div className="d-flex justify-content-between align-items-center mb-3">
          <h2>Ambulance Responder Interface</h2>
          <small className="text-muted">Auto-refresh every 15s</small>
        </div>

        {!responderId && (
          <div className="alert alert-warning">Responder ID not found. Please set <code>responderId</code> in localStorage after login.</div>
        )}

        {error && <div className="alert alert-danger">{error}</div>}
        {loading && <div className="mb-2">Loading assigned incidents...</div>}

        <div className="row">
          {assignedIncidents.length === 0 && !loading && (
            <div className="col-12"><div className="alert alert-info">No assigned incidents at the moment.</div></div>
          )}

          {assignedIncidents.map(inc => (
            <div className="col-12 col-md-6 mb-3" key={inc.incidentId}>
              <div className={`card ${((inc.severityScore ?? 0) >= 8) ? 'border-danger' : ''}`}>
                <div className="card-body">
                  <h5 className="card-title">Incident #{inc.incidentId} <small className="text-muted">{inc.status}</small></h5>
                  <p className="card-text"><strong>Details:</strong> {inc.description || inc.note || '-'}</p>
                  <p className="card-text small"><strong>Contact:</strong> {inc.phoneNumber || '-'}</p>
                  <p className="card-text small"><strong>Location:</strong> {inc.addressText || (inc.latitude ? `${Number(inc.latitude).toFixed(5)}, ${Number(inc.longitude).toFixed(5)}` : 'Unknown')}</p>

                  <div className="mb-2">
                    {/* ETA based on responder coords stored in localStorage */}
                    {(() => {
                      const rLat = parseFloat(localStorage.getItem('responderLat') || '0');
                      const rLon = parseFloat(localStorage.getItem('responderLon') || '0');
                      if (rLat && rLon && inc.latitude && inc.longitude) {
                        const eta = estimateEtaMinutes(rLat, rLon, inc.latitude, inc.longitude);
                        return <p className="mb-1 small"><strong>Estimated ETA:</strong> {eta ? `${eta} min` : '—'}</p>;
                      }
                      return null;
                    })()}
                  </div>

                  <div className="d-flex gap-2 mb-2">
                    <button className="btn btn-outline-primary btn-sm" onClick={() => openNavigation(inc)}>Navigate</button>
                    <button className="btn btn-warning btn-sm" onClick={() => updateStatus(inc, 'InProgress')}>En Route</button>
                    <button className="btn btn-success btn-sm" onClick={() => updateStatus(inc, 'Resolved')}>Arrived</button>
                  </div>

                  <div className="text-muted small">Open in maps for full routing. To embed maps use Google Maps JavaScript API.</div>
                </div>
              </div>
            </div>
          ))}
        </div>
      </div>
    </div>
  );
}
