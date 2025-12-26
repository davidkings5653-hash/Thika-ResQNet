import React, { useState, useEffect } from "react";
import 'bootstrap/dist/css/bootstrap.min.css';

/*
Create a web form for citizens to report emergencies.
Fields: Description, GPS Coordinates (auto-detect if allowed), Address Text, Phone Number.
Include severity selection (High, Medium, Low) as optional dropdown (AI will auto-assign if not selected).
Use responsive design and Bootstrap.
On submit, send POST request to /api/emergencyreports.
*/

const CitizenReport = () => {
  const [formData, setFormData] = useState({
    description: "",
    latitude: "",
    longitude: "",
    address: "",
    phoneNumber: "",
    severity: "",
  });

  const [errors, setErrors] = useState({});
  const [loading, setLoading] = useState(false);
  const [message, setMessage] = useState(null);

  useEffect(() => {
    // Auto-detect GPS coordinates if allowed
    if (navigator.geolocation) {
      navigator.geolocation.getCurrentPosition((position) => {
        setFormData(prev => ({
          ...prev,
          latitude: position.coords.latitude,
          longitude: position.coords.longitude
        }));
      }, () => {}, { enableHighAccuracy: true, timeout: 5000 });
    }
  }, []);

  const handleChange = (e) => {
    const { name, value } = e.target;
    setFormData(prev => ({ ...prev, [name]: value }));
  };

  const phoneRegex = /^\+?\d{7,15}$/;

  const validate = () => {
    const e = {};
    if (!formData.description || formData.description.trim().length < 5) e.description = 'Please provide a brief description (min 5 chars)';
    const hasCoords = formData.latitude !== "" && formData.longitude !== "";
    const hasAddress = formData.address && formData.address.trim().length > 3;
    if (!hasCoords && !hasAddress) e.location = 'Provide GPS coordinates or an address';
    if (!formData.phoneNumber || !phoneRegex.test(formData.phoneNumber)) e.phoneNumber = 'Valid phone number is required (7-15 digits, optional +)';
    setErrors(e);
    return Object.keys(e).length === 0;
  };

  const mapSeverityToScore = (s) => {
    if (s === 'High') return 9;
    if (s === 'Medium') return 5;
    if (s === 'Low') return 1;
    return 0; // 0 means let server/AI auto-assign
  };

  const handleSubmit = async (e) => {
    e.preventDefault();
    setMessage(null);
    if (!validate()) return;
    setLoading(true);

    const payload = {
      ReporterId: 0, // anonymous; backend can map phone to user
      Description: formData.description,
      Latitude: formData.latitude === "" ? null : Number(formData.latitude),
      Longitude: formData.longitude === "" ? null : Number(formData.longitude),
      AddressText: formData.address,
      SeverityScore: mapSeverityToScore(formData.severity),
      Status: 'Open',
      // include phone so backend can associate reporter
      PhoneNumber: formData.phoneNumber
    };

    try {
      const token = localStorage.getItem('thika_token');
      const res = await fetch('/api/emergencyreports', {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
          ...(token ? { Authorization: `Bearer ${token}` } : {})
        },
        body: JSON.stringify(payload)
      });

      if (!res.ok) {
        const data = await res.json().catch(() => ({}));
        setMessage({ type: 'danger', text: data?.message || `Submission failed (${res.status})` });
      } else {
        const data = await res.json().catch(() => null);
        setMessage({ type: 'success', text: 'Report submitted successfully' });
        // clear form except GPS (so user can submit again if needed)
        setFormData(prev => ({ ...prev, description: '', address: '', phoneNumber: '', severity: '' }));
        setErrors({});
      }
    } catch (err) {
      setMessage({ type: 'danger', text: 'Network error' });
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="container mt-4">
      <h2 className="text-center mb-4">Report an Emergency</h2>
      <div className="row justify-content-center">
        <div className="col-12 col-md-10 col-lg-8">
          <div className="card shadow-sm">
            <div className="card-body">
              {message && (
                <div className={`alert alert-${message.type}`} role="alert">{message.text}</div>
              )}

              <form onSubmit={handleSubmit} noValidate>
                <div className="mb-3">
                  <label className="form-label">Description *</label>
                  <textarea className={`form-control ${errors.description ? 'is-invalid' : ''}`} name="description" value={formData.description} onChange={handleChange} rows={3}></textarea>
                  {errors.description && <div className="invalid-feedback">{errors.description}</div>}
                </div>

                <div className="row">
                  <div className="col-md-6 mb-3">
                    <label className="form-label">Latitude</label>
                    <input type="number" step="any" className="form-control" name="latitude" value={formData.latitude ?? ''} onChange={handleChange} />
                  </div>
                  <div className="col-md-6 mb-3">
                    <label className="form-label">Longitude</label>
                    <input type="number" step="any" className="form-control" name="longitude" value={formData.longitude ?? ''} onChange={handleChange} />
                  </div>
                </div>

                <div className="mb-3">
                  <label className="form-label">Address</label>
                  <input type="text" className={`form-control ${errors.location ? 'is-invalid' : ''}`} name="address" value={formData.address} onChange={handleChange} />
                  {errors.location && <div className="invalid-feedback">{errors.location}</div>}
                </div>

                <div className="mb-3">
                  <label className="form-label">Phone Number *</label>
                  <input type="tel" className={`form-control ${errors.phoneNumber ? 'is-invalid' : ''}`} name="phoneNumber" value={formData.phoneNumber} onChange={handleChange} placeholder="e.g. +254700000000" />
                  {errors.phoneNumber && <div className="invalid-feedback">{errors.phoneNumber}</div>}
                </div>

                <div className="mb-3">
                  <label className="form-label">Severity (optional)</label>
                  <select className="form-select" name="severity" value={formData.severity} onChange={handleChange}>
                    <option value="">Auto-assign</option>
                    <option>High</option>
                    <option>Medium</option>
                    <option>Low</option>
                  </select>
                </div>

                <div className="d-grid">
                  <button type="submit" className="btn btn-danger" disabled={loading}>{loading ? 'Submitting...' : 'Submit Report'}</button>
                </div>

                <div className="mt-3 text-muted small">Fields marked * are required. Allow location access to auto-fill GPS coordinates.</div>
              </form>
            </div>
          </div>
        </div>
      </div>
    </div>
  );
};

export default CitizenReport;
