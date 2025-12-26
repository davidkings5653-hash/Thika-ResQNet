import React, { useState, useEffect, useRef } from "react";
import 'bootstrap/dist/css/bootstrap.min.css';

/*
Create a hospital portal page.
Display incoming patient alerts.
Show available beds (ER, ICU).
Include form to update bed availability.
Show status updates from ambulance (En Route, ETA).
Responsive table for patient alerts with sorting by severity.
*/

const HospitalPortal = () => {
  const [patients, setPatients] = useState([]);
  const [beds, setBeds] = useState({ ER: 0, ICU: 0 });
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState(null);
  const [sortKey, setSortKey] = useState('severity');
  const [message, setMessage] = useState(null);
  const intervalRef = useRef(null);

  // hospitalId should be set in localStorage or default to 1
  const hospitalId = Number(localStorage.getItem('hospitalId') || 1);

  useEffect(() => {
    fetchData();
    intervalRef.current = setInterval(fetchData, 30000); // refresh every 30s
    return () => clearInterval(intervalRef.current);
  }, []);

  async function fetchData() {
    setLoading(true);
    setError(null);
    try {
      const token = localStorage.getItem('thika_token');

      // Fetch hospital info
      const hospitalRes = await fetch(`/api/hospitals/${hospitalId}`, {
        headers: { 'Content-Type': 'application/json', ...(token ? { Authorization: `Bearer ${token}` } : {}) }
      });
      if (hospitalRes.ok) {
        const h = await hospitalRes.json();
        setBeds({ ER: h.availableBeds ?? 0, ICU: h.icuCapacity ?? 0 });
      }

      // Try a dedicated alerts endpoint first
      const alertsRes = await fetch(`/api/hospital/alerts?hospitalId=${hospitalId}`, {
        headers: { ...(token ? { Authorization: `Bearer ${token}` } : {}) }
      });

      let alerts = [];
      if (alertsRes.ok) {
        alerts = await alertsRes.json();
      } else {
        // fallback: get incidents and filter by hospitalId if possible
        const incRes = await fetch('/api/incidents', { headers: { ...(token ? { Authorization: `Bearer ${token}` } : {}) } });
        if (incRes.ok) {
          const all = await incRes.json();
          alerts = (Array.isArray(all) ? all : []).filter(i => i.assignedHospitalId === hospitalId || i.hospitalId === hospitalId || i.targetHospitalId === hospitalId);
        }
      }

      // Normalize alerts to patient rows
      const rows = (Array.isArray(alerts) ? alerts : []).map(a => ({
        incidentId: a.incidentId ?? a.id,
        name: a.reporterName ?? a.patientName ?? (a.description ? a.description.substring(0, 30) : 'Unknown'),
        status: a.status ?? 'Open',
        severityScore: a.severityScore ?? (a.severity === 'High' ? 9 : a.severity === 'Medium' ? 5 : 1),
        severity: severityLabel(a.severityScore ?? (a.severity === 'High' ? 9 : a.severity === 'Medium' ? 5 : 1)),
        eta: computeEta(a),
        addressText: a.addressText,
        latitude: a.latitude,
        longitude: a.longitude,
        assignedResponderId: a.assignedResponderId ?? null,
        createdAt: a.createdAt
      }));

      setPatients(rows);
    } catch (err) {
      setError('Failed to load hospital data');
      setPatients([]);
    } finally {
      setLoading(false);
    }
  }

  function severityLabel(score) {
    if (score >= 8) return 'High';
    if (score >= 4) return 'Medium';
    return 'Low';
  }

  function computeEta(a) {
    // If responder location is available in localStorage, estimate ETA to incident
    try {
      const rLat = parseFloat(localStorage.getItem('responderLat') || '0');
      const rLon = parseFloat(localStorage.getItem('responderLon') || '0');
      if (rLat && rLon && a.latitude && a.longitude) {
        const minutes = estimateEtaMinutes(rLat, rLon, a.latitude, a.longitude);
        return `${minutes} min`;
      }
    } catch { }
    return a.status === 'InProgress' ? 'En Route' : '-';
  }

  function estimateEtaMinutes(rLat, rLon, lat, lon, speedKmH = 40) {
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
  }

  async function handleBedUpdate() {
    setMessage(null);
    setError(null);
    try {
      const token = localStorage.getItem('thika_token');
      const body = { AvailableBeds: Number(beds.ER), ICUCapacity: Number(beds.ICU) };
      const res = await fetch(`/api/hospital/${hospitalId}/beds`, {
        method: 'PUT',
        headers: { 'Content-Type': 'application/json', ...(token ? { Authorization: `Bearer ${token}` } : {}) },
        body: JSON.stringify(body)
      });
      if (!res.ok) {
        const data = await res.json().catch(() => ({}));
        setError(data?.message || `Update failed (${res.status})`);
      } else {
        setMessage({ type: 'success', text: 'Beds updated' });
        // refresh data
        fetchData();
      }
    } catch (err) {
      setError('Network error');
    }
  }

  function sortedPatients() {
    const copy = [...patients];
    if (sortKey === 'severity') {
      copy.sort((a, b) => (b.severityScore || 0) - (a.severityScore || 0));
    } else if (sortKey === 'time') {
      copy.sort((a, b) => new Date(b.createdAt) - new Date(a.createdAt));
    }
    return copy;
  }

  return (
    <div className="container mt-4">
      <div className="d-flex justify-content-between align-items-center mb-3">
        <h2>Hospital Portal</h2>
        <div>
          <button className="btn btn-outline-secondary me-2" onClick={fetchData}>Refresh</button>
          <small className="text-muted">Auto-refresh every 30s</small>
        </div>
      </div>

      {message && <div className={`alert alert-${message.type}`}>{message.text}</div>}
      {error && <div className="alert alert-danger">{error}</div>}
      {loading && <div className="mb-2">Loading...</div>}

      <div className="mb-4">
        <h5>Available Beds</h5>
        <form className="row g-3" onSubmit={(e) => { e.preventDefault(); handleBedUpdate(); }}>
          <div className="col-6 col-md-3">
            <label className="form-label">ER Beds</label>
            <input type="number" className="form-control" value={beds.ER} onChange={(e) => setBeds({...beds, ER: Number(e.target.value)})} />
          </div>
          <div className="col-6 col-md-3">
            <label className="form-label">ICU Beds</label>
            <input type="number" className="form-control" value={beds.ICU} onChange={(e) => setBeds({...beds, ICU: Number(e.target.value)})} />
          </div>
          <div className="col-12 col-md-3 align-self-end">
            <button type="submit" className="btn btn-primary">Update Beds</button>
          </div>
        </form>
      </div>

      <div className="mb-3 d-flex align-items-center gap-2">
        <label className="mb-0">Sort by:</label>
        <select className="form-select w-auto" value={sortKey} onChange={(e) => setSortKey(e.target.value)}>
          <option value="severity">Severity</option>
          <option value="time">Time</option>
        </select>
      </div>

      <div className="table-responsive">
        <table className="table table-bordered align-middle">
          <thead className="table-dark">
            <tr>
              <th>Patient Name</th>
              <th>Incident ID</th>
              <th>Status</th>
              <th>Severity</th>
              <th>Ambulance ETA</th>
            </tr>
          </thead>
          <tbody>
            {sortedPatients().length === 0 && !loading && (
              <tr><td colSpan="5" className="text-center text-muted">No incoming alerts</td></tr>
            )}

            {sortedPatients().map((p) => (
              <tr key={p.incidentId} className={p.severityScore >= 8 ? 'table-danger' : ''}>
                <td>{p.name}</td>
                <td>{p.incidentId}</td>
                <td>{p.status}</td>
                <td>{p.severity}</td>
                <td>{p.eta}</td>
              </tr>
            ))}
          </tbody>
        </table>
      </div>
    </div>
  );
};

export default HospitalPortal;
