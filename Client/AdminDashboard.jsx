import React, { useState, useEffect } from "react";
import 'bootstrap/dist/css/bootstrap.min.css';
import { Bar, Pie } from 'react-chartjs-2';
import MobileNavbar from './MobileNavbar';

/*
Create an admin dashboard page.
Display analytics cards: Total Incidents, Average Response Time, Incidents by Severity.
Include charts: Bar chart for incidents per day, Pie chart for severity distribution.
Enable export of reports (CSV / PDF).
*/

const AdminDashboard = () => {
  const [totalIncidents, setTotalIncidents] = useState(0);
  const [avgResponseTime, setAvgResponseTime] = useState(0);
  const [incidentsBySeverity, setIncidentsBySeverity] = useState({ High: 0, Medium: 0, Low: 0 });
  const [incidentsPerDay, setIncidentsPerDay] = useState([]);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState(null);

  useEffect(() => {
    loadAnalytics();
  }, []);

  const loadAnalytics = async () => {
    setLoading(true);
    setError(null);
    try {
      const token = localStorage.getItem('thika_token');
      const now = new Date();

      // Fetch monthly summary
      const repRes = await fetch(`/api/analytics/monthly-report?year=${now.getUTCFullYear()}&month=${now.getUTCMonth() + 1}`, {
        headers: { ...(token ? { Authorization: `Bearer ${token}` } : {}) }
      });
      if (repRes.ok) {
        const rep = await repRes.json();
        setTotalIncidents(rep.totalIncidents ?? 0);
        setAvgResponseTime(rep.averageResponseMinutes ?? 0);
        setIncidentsBySeverity({ High: rep.highSeverity ?? 0, Medium: rep.mediumSeverity ?? 0, Low: rep.lowSeverity ?? 0 });
      }

      // Fetch incidents / response-times to build incidentsPerDay
      const rtRes = await fetch('/api/analytics/response-times', { headers: { ...(token ? { Authorization: `Bearer ${token}` } : {}) } });
      if (rtRes.ok) {
        const list = await rtRes.json();
        // Aggregate incidents per day
        const counts = {};
        (list || []).forEach(i => {
          const d = new Date(i.createdAt).toISOString().substring(0, 10);
          counts[d] = (counts[d] || 0) + 1;
        });
        const keys = Object.keys(counts).sort();
        setIncidentsPerDay(keys.map(k => ({ date: k, count: counts[k] })));
      }
    } catch (err) {
      setError('Failed to load analytics');
    } finally {
      setLoading(false);
    }
  };

  const exportCsv = async () => {
    try {
      const token = localStorage.getItem('thika_token');
      const res = await fetch('/api/admin/export/csv', { headers: { ...(token ? { Authorization: `Bearer ${token}` } : {}) } });
      if (!res.ok) throw new Error('Export failed');
      const blob = await res.blob();
      const url = window.URL.createObjectURL(blob);
      const a = document.createElement('a');
      a.href = url;
      a.download = `incidents_${new Date().toISOString().slice(0,16).replace(/[:T]/g,'_')}.csv`;
      a.click();
    } catch (e) {
      alert('CSV export failed');
    }
  };

  const exportPdf = async () => {
    try {
      const token = localStorage.getItem('thika_token');
      const res = await fetch('/api/admin/export/pdf', { headers: { ...(token ? { Authorization: `Bearer ${token}` } : {}) } });
      if (!res.ok) throw new Error('Export failed');
      const blob = await res.blob();
      const url = window.URL.createObjectURL(blob);
      const a = document.createElement('a');
      a.href = url;
      a.download = `incidents_${new Date().toISOString().slice(0,16).replace(/[:T]/g,'_')}.pdf`;
      a.click();
    } catch (e) {
      alert('PDF export failed');
    }
  };

  const barData = {
    labels: incidentsPerDay.map(i => i.date),
    datasets: [
      {
        label: 'Incidents',
        backgroundColor: 'rgba(54, 162, 235, 0.6)',
        borderColor: 'rgba(54, 162, 235, 1)',
        borderWidth: 1,
        data: incidentsPerDay.map(i => i.count)
      }
    ]
  };

  const pieData = {
    labels: ['High', 'Medium', 'Low'],
    datasets: [
      {
        data: [incidentsBySeverity.High, incidentsBySeverity.Medium, incidentsBySeverity.Low],
        backgroundColor: ['#dc3545', '#ffc107', '#198754']
      }
    ]
  };

  return (
    <div>
      <MobileNavbar />
      <div className="container mt-4">
        <div className="d-flex justify-content-between align-items-center mb-3">
          <h2>Admin Analytics Dashboard</h2>
          <div>
            <button className="btn btn-outline-secondary me-2" onClick={loadAnalytics}>Refresh</button>
            <button className="btn btn-sm btn-success me-1" onClick={exportCsv}>Export CSV</button>
            <button className="btn btn-sm btn-danger" onClick={exportPdf}>Export PDF</button>
          </div>
        </div>

        {error && <div className="alert alert-danger">{error}</div>}
        {loading && <div className="mb-2">Loading analytics...</div>}

        <div className="row mb-4">
          <div className="col-md-4 mb-3">
            <div className="card text-white bg-primary h-100">
              <div className="card-body">
                <h5 className="card-title">Total Incidents</h5>
                <p className="card-text display-6">{totalIncidents}</p>
              </div>
            </div>
          </div>

          <div className="col-md-4 mb-3">
            <div className="card text-white bg-success h-100">
              <div className="card-body">
                <h5 className="card-title">Average Response Time</h5>
                <p className="card-text display-6">{Math.round(avgResponseTime * 10) / 10} mins</p>
              </div>
            </div>
          </div>

          <div className="col-md-4 mb-3">
            <div className="card text-white bg-warning h-100">
              <div className="card-body">
                <h5 className="card-title">Incidents by Severity</h5>
                <p className="card-text">High: {incidentsBySeverity.High} • Medium: {incidentsBySeverity.Medium} • Low: {incidentsBySeverity.Low}</p>
              </div>
            </div>
          </div>
        </div>

        <div className="row">
          <div className="col-12 col-lg-8 mb-4" style={{minHeight: 300}}>
            <div className="card h-100">
              <div className="card-body">
                <h6 className="card-subtitle mb-3">Incidents per Day</h6>
                <div style={{height: 300}}>
                  <Bar data={barData} options={{maintainAspectRatio: false}} />
                </div>
              </div>
            </div>
          </div>

          <div className="col-12 col-lg-4 mb-4" style={{minHeight: 300}}>
            <div className="card h-100">
              <div className="card-body">
                <h6 className="card-subtitle mb-3">Severity Distribution</h6>
                <div style={{height: 300}}>
                  <Pie data={pieData} options={{maintainAspectRatio: false}} />
                </div>
              </div>
            </div>
          </div>
        </div>

      </div>
    </div>
  );
};

export default AdminDashboard;
