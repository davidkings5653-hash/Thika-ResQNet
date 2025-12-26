import React, { useEffect, useState, useRef } from "react";
import 'bootstrap/dist/css/bootstrap.min.css';

/*
Notifications component.
- Polls /api/notifications (fallback to /api/incidents?status=Open) for new alerts
- Optional WebSocket support via wsUrl prop
- Supports mark-as-read (POST /api/notifications/mark-read) and dismiss locally
- Uses Bootstrap alert classes
*/

const Notifications = ({ pollInterval = 15000, wsUrl = null }) => {
  const [alerts, setAlerts] = useState([]);
  const [error, setError] = useState(null);
  const wsRef = useRef(null);

  useEffect(() => {
    let mounted = true;

    async function fetchAlerts() {
      try {
        const token = localStorage.getItem('thika_token');
        // try a dedicated notifications endpoint first
        let res = await fetch('/api/notifications', {
          headers: {
            'Content-Type': 'application/json',
            ...(token ? { Authorization: `Bearer ${token}` } : {})
          }
        });

        let items = [];
        if (res.ok) {
          items = await res.json();
        } else {
          // fallback: open incidents
          res = await fetch('/api/incidents?status=Open', {
            headers: {
              'Content-Type': 'application/json',
              ...(token ? { Authorization: `Bearer ${token}` } : {})
            }
          });
          if (res.ok) items = await res.json();
        }

        if (!mounted) return;

        // map incidents/notifications to a standard shape
        const mapped = (Array.isArray(items) ? items : []).map(i => ({
          id: i.id ?? i.incidentId ? `inc-${i.incidentId}` : i.notificationId ?? i.uid,
          title: i.title ?? (i.incidentId ? `Incident #${i.incidentId}` : 'Notification'),
          message: i.message ?? i.description ?? i.addressText ?? '',
          severity: i.severity ?? (i.severityScore ? (i.severityScore >= 8 ? 'High' : i.severityScore >= 4 ? 'Medium' : 'Low') : 'Low'),
          createdAt: i.createdAt ?? new Date().toISOString(),
          read: !!i.read,
          meta: i
        }));

        // merge preserving existing read/dismiss state
        setAlerts(prev => {
          const map = new Map(prev.map(a => [a.id, a]));
          for (const m of mapped) {
            if (!map.has(m.id)) map.set(m.id, m);
            else {
              const existing = map.get(m.id);
              // preserve read flag if already set locally
              m.read = existing.read || m.read;
              map.set(m.id, { ...existing, ...m });
            }
          }
          // sort newest first
          return Array.from(map.values()).sort((a, b) => new Date(b.createdAt) - new Date(a.createdAt));
        });
      } catch (err) {
        if (!mounted) return;
        setError('Failed to fetch notifications');
      }
    }

    fetchAlerts();
    const interval = setInterval(fetchAlerts, pollInterval);

    // optional websocket
    if (wsUrl) {
      try {
        wsRef.current = new WebSocket(wsUrl);
        wsRef.current.onmessage = (evt) => {
          try {
            const payload = JSON.parse(evt.data);
            const note = {
              id: payload.id ?? (payload.incidentId ? `inc-${payload.incidentId}` : `note-${Date.now()}`),
              title: payload.title ?? (payload.incidentId ? `Incident #${payload.incidentId}` : 'Notification'),
              message: payload.message ?? payload.description ?? '',
              severity: payload.severity ?? (payload.severityScore ? (payload.severityScore >= 8 ? 'High' : payload.severityScore >= 4 ? 'Medium' : 'Low') : 'Low'),
              createdAt: payload.createdAt ?? new Date().toISOString(),
              read: false,
              meta: payload
            };
            setAlerts(prev => [note, ...prev.filter(n => n.id !== note.id)]);
          } catch (e) { /* ignore invalid messages */ }
        };
      } catch (e) { /* ignore websocket setup errors */ }
    }

    return () => {
      mounted = false;
      clearInterval(interval);
      if (wsRef.current) wsRef.current.close();
    };
  }, [pollInterval, wsUrl]);

  const markAsRead = async (id) => {
    setAlerts(prev => prev.map(a => a.id === id ? { ...a, read: true } : a));
    try {
      const token = localStorage.getItem('thika_token');
      await fetch('/api/notifications/mark-read', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json', ...(token ? { Authorization: `Bearer ${token}` } : {}) },
        body: JSON.stringify({ id })
      });
    } catch (e) {
      // ignore errors, local state already updated
    }
  };

  const dismiss = (id) => {
    setAlerts(prev => prev.filter(a => a.id !== id));
  };

  return (
    <div className="position-fixed top-0 end-0 p-3" style={{ zIndex: 1050, maxWidth: 360 }}>
      {error && <div className="alert alert-danger">{error}</div>}

      {alerts.map(alert => (
        <div key={alert.id} className={`alert alert-${alert.severity === 'High' ? 'danger' : alert.severity === 'Medium' ? 'warning' : 'success'} alert-dismissible fade show`} role="alert">
          <div className="d-flex justify-content-between">
            <div>
              <strong>{alert.title}</strong>
              <div className="small">{new Date(alert.createdAt).toLocaleString()}</div>
              <div className="mt-1">{alert.message}</div>
            </div>
            <div className="ms-2 text-end">
              {!alert.read && <button className="btn btn-sm btn-light mb-1" onClick={() => markAsRead(alert.id)}>Mark read</button>}
              <button className="btn btn-sm btn-outline-secondary" onClick={() => dismiss(alert.id)} aria-label="Close">&times;</button>
            </div>
          </div>
        </div>
      ))}

      {alerts.length === 0 && <div className="text-muted small">No new notifications</div>}
    </div>
  );
};

export default Notifications;
