import React, { useState, useEffect, useRef } from "react";
import 'bootstrap/dist/css/bootstrap.min.css';

/*
Create a dispatcher dashboard page.
Show a table or card list of active incidents: Incident ID, Location, Severity, Status.
Include buttons to Assign Ambulance, Update Status, or Reassign.
Display real-time updates with auto-refresh every 30 seconds.
Highlight high severity incidents in red.
Use Bootstrap cards or DataTables for dynamic table.
*/

const DispatcherDashboard = () => {
  const [incidents, setIncidents] = useState([]);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState(null);
  const intervalRef = useRef(null);

  const fetchIncidents = async () => {
    setLoading(true);
    setError(null);
    try {
      const token = localStorage.getItem('thika_token');
      const res = await fetch('/api/dispatch/active-incidents', {
        headers: {
          'Content-Type': 'application/json',
          ...(token ? { Authorization: `Bearer ${token}` } : {})
        }
      });
      if (!res.ok) {
        const body = await res.json().catch(() => ({}));
        setError(body?.message || `Error ${res.status}`);
        setIncidents([]);
      } else {
        const data = await res.json();
        // normalize incidents to expected fields
        const normalized = (Array.isArray(data) ? data : []).map(i => ({
          incidentId: i.incidentId ?? i.id ?? i.incidentID,
          description: i.description,
          addressText: i.addressText || i.location || '',
          latitude: i.latitude,
          longitude: i.longitude,
          severityScore: i.severityScore ?? (i.severity === 'High' ? 9 : i.severity === 'Medium' ? 5 : 1),
          status: i.status,
          assignedResponderId: i.assignedResponderId ?? null
        }));
        setIncidents(normalized);
      }
    } catch (err) {
      setError('Network error');
      setIncidents([]);
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    fetchIncidents();
    intervalRef.current = setInterval(fetchIncidents, 30000); // auto-refresh every 30s
    return () => clearInterval(intervalRef.current);
  }, []);

  const assignResponder = async (incidentId, override = false) => {
    const responderIdStr = prompt('Enter Responder ID to assign:');
    if (!responderIdStr) return;
    const responderId = parseInt(responderIdStr, 10);
    if (Number.isNaN(responderId)) { alert('Invalid responder id'); return; }

    try {
      const token = localStorage.getItem('thika_token');
      const res = await fetch('/api/dispatch/assign', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json', ...(token ? { Authorization: `Bearer ${token}` } : {}) },
        body: JSON.stringify({ incidentId, responderId, overrideExisting: override })
      });
      if (!res.ok) {
        const body = await res.json().catch(() => ({}));
        alert(body?.message || `Assign failed (${res.status})`);
      } else {
        alert('Responder assigned');
        fetchIncidents();
      }
    } catch (err) {
      alert('Network error');
    }
  };

  const handleAssign = (id) => assignResponder(id, false);
  const handleReassign = (id) => assignResponder(id, true);

  const handleUpdateStatus = async (incidentId) => {
    const status = prompt('Enter new status (Open, InProgress, Resolved, Closed, Cancelled):');
    if (!status) return;
    try {
      const token = localStorage.getItem('thika_token');
      // fetch current incident
      const getRes = await fetch(`/api/incidents/${incidentId}`, {
        headers: { 'Content-Type': 'application/json', ...(token ? { Authorization: `Bearer ${token}` } : {}) }
      });
      if (!getRes.ok) { alert('Unable to load incident'); return; }
      const incident = await getRes.json();
      // update status and PUT
      incident.status = status;
      const putRes = await fetch(`/api/incidents/${incidentId}`, {
        method: 'PUT',
        headers: { 'Content-Type': 'application/json', ...(token ? { Authorization: `Bearer ${token}` } : {}) },
        body: JSON.stringify(incident)
      });
      if (!putRes.ok) {
        const body = await putRes.json().catch(() => ({}));
        alert(body?.message || `Update failed: ${putRes.status}`);
      } else {
        alert('Status updated');
        fetchIncidents();
      }
    } catch (err) {
      alert('Network error');
    }
  };

  const renderLocation = (inc) => {
    if (inc.addressText) return inc.addressText;
    if (inc.latitude && inc.longitude) return `${Number(inc.latitude).toFixed(5)}, ${Number(inc.longitude).toFixed(5)}`;
    return 'Unknown';
  };

  const severityLabel = (score) => {
    if (score >= 8) return 'High';
    if (score >= 4) return 'Medium';
    return 'Low';
  };

  return (
    <div className="container mt-4">
      <div className="d-flex justify-content-between align-items-center mb-3">
        <h2>Dispatcher Dashboard</h2>
        <div>
          <button className="btn btn-outline-secondary me-2" onClick={fetchIncidents}>Refresh</button>
          <small className="text-muted">Auto-refresh every 30s</small>
        </div>
      </div>

      {error && <div className="alert alert-danger">{error}</div>}

      <div className="table-responsive">
        <table className="table table-bordered align-middle">
          <thead className="table-dark">
            <tr>
              <th>Incident ID</th>
              <th>Location</th>
              <th>Severity</th>
              <th>Status</th>
              <th>Assigned</th>
              <th>Actions</th>
            </tr>
          </thead>
          <tbody>
            {incidents.length === 0 && !loading && (
              <tr><td colSpan="6" className="text-center text-muted">No active incidents</td></tr>
            )}

            {incidents.map((inc) => (
              <tr key={inc.incidentId} className={inc.severityScore >= 8 ? 'table-danger' : ''}>
                <td>{inc.incidentId}</td>
                <td style={{ minWidth: 200 }}>{renderLocation(inc)}</td>
                <td>{severityLabel(inc.severityScore)}</td>
                <td>{inc.status}</td>
                <td>{inc.assignedResponderId ?? '-'}</td>
                <td>
                  <div className="btn-group" role="group">
                    <button className="btn btn-sm btn-primary" onClick={() => handleAssign(inc.incidentId)}>Assign Ambulance</button>
                    <button className="btn btn-sm btn-warning" onClick={() => handleUpdateStatus(inc.incidentId)}>Update Status</button>
                    <button className="btn btn-sm btn-outline-danger" onClick={() => handleReassign(inc.incidentId)}>Reassign</button>
                  </div>
                </td>
              </tr>
            ))}
          </tbody>
        </table>
      </div>

    </div>
  );
};

export default DispatcherDashboard;
