import React, { useState } from "react";
import 'bootstrap/dist/css/bootstrap.min.css';

/*
Create a responsive login and registration page for Thika ResQNet.
Fields: Full Name, Phone Number, Role (Dropdown: Public, Responder, Dispatcher, HospitalAdmin), Password
Include client-side validation for required fields and phone number format.
Include 'Submit' button that triggers POST request to /api/auth/register or /api/auth/login.
*/

const LoginRegister = () => {
  const [isRegister, setIsRegister] = useState(true);
  const [formData, setFormData] = useState({
    fullName: "",
    phoneNumber: "",
    role: "Public",
    password: "",
  });
  const [errors, setErrors] = useState({});
  const [loading, setLoading] = useState(false);
  const [message, setMessage] = useState(null);

  const phoneRegex = /^\+?\d{7,15}$/;

  const handleChange = (e) => {
    setFormData({...formData, [e.target.name]: e.target.value});
  };

  const validate = () => {
    const e = {};
    if (isRegister) {
      if (!formData.fullName || formData.fullName.trim().length < 2) e.fullName = 'Full name is required';
    }
    if (!formData.phoneNumber || !phoneRegex.test(formData.phoneNumber)) e.phoneNumber = 'Enter a valid phone number (7-15 digits, optional +)';
    if (!formData.password || formData.password.length < 6) e.password = 'Password must be at least 6 characters';
    setErrors(e);
    return Object.keys(e).length === 0;
  };

  const handleSubmit = async (e) => {
    e.preventDefault();
    setMessage(null);
    if (!validate()) return;
    setLoading(true);

    try {
      const payload = isRegister
        ? { fullName: formData.fullName, phoneNumber: formData.phoneNumber, password: formData.password, role: formData.role }
        : { phoneNumber: formData.phoneNumber, password: formData.password };

      const url = isRegister ? '/api/auth/register' : '/api/auth/login';
      const res = await fetch(url, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify(payload),
      });

      const data = await res.json().catch(() => ({}));
      if (!res.ok) {
        setMessage({ type: 'danger', text: data?.message || `Request failed (${res.status})` });
      } else {
        // store token if provided
        if (data?.token) localStorage.setItem('thika_token', data.token);
        setMessage({ type: 'success', text: isRegister ? 'Registration successful' : 'Login successful' });
        if (!isRegister) {
          // optional: redirect or refresh
        }
      }
    } catch (err) {
      setMessage({ type: 'danger', text: 'Network error' });
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="container mt-5">
      <div className="row justify-content-center">
        <div className="col-12 col-md-8 col-lg-5">
          <div className="card shadow-sm">
            <div className="card-body">
              <div className="d-flex justify-content-between align-items-center mb-3">
                <h4 className="mb-0">{isRegister ? 'Register' : 'Login'} — Thika ResQNet</h4>
                <button className="btn btn-sm btn-outline-secondary" onClick={() => { setIsRegister(!isRegister); setErrors({}); setMessage(null); }}>
                  {isRegister ? 'Switch to Login' : 'Switch to Register'}
                </button>
              </div>

              {message && (
                <div className={`alert alert-${message.type}`} role="alert">{message.text}</div>
              )}

              <form onSubmit={handleSubmit} noValidate>
                {isRegister && (
                  <div className="mb-3">
                    <label className="form-label">Full Name</label>
                    <input type="text" className={`form-control ${errors.fullName ? 'is-invalid' : ''}`} name="fullName" value={formData.fullName} onChange={handleChange} />
                    {errors.fullName && <div className="invalid-feedback">{errors.fullName}</div>}
                  </div>
                )}

                <div className="mb-3">
                  <label className="form-label">Phone Number</label>
                  <input type="tel" className={`form-control ${errors.phoneNumber ? 'is-invalid' : ''}`} name="phoneNumber" value={formData.phoneNumber} onChange={handleChange} placeholder="e.g. +254700000000" />
                  {errors.phoneNumber && <div className="invalid-feedback">{errors.phoneNumber}</div>}
                </div>

                {isRegister && (
                  <div className="mb-3">
                    <label className="form-label">Role</label>
                    <select className="form-select" name="role" value={formData.role} onChange={handleChange}>
                      <option>Public</option>
                      <option>Responder</option>
                      <option>Dispatcher</option>
                      <option>HospitalAdmin</option>
                    </select>
                  </div>
                )}

                <div className="mb-3">
                  <label className="form-label">Password</label>
                  <input type="password" className={`form-control ${errors.password ? 'is-invalid' : ''}`} name="password" value={formData.password} onChange={handleChange} />
                  {errors.password && <div className="invalid-feedback">{errors.password}</div>}
                </div>

                <div className="d-grid">
                  <button type="submit" className="btn btn-primary" disabled={loading}>
                    {loading ? 'Please wait...' : (isRegister ? 'Register' : 'Login')}
                  </button>
                </div>
              </form>

              <div className="text-center mt-3 small text-muted">By continuing you agree to the platform terms.</div>
            </div>
          </div>
          <div className="text-center mt-3 text-muted small">Need help? Contact support.</div>
        </div>
      </div>
    </div>
  );
};

export default LoginRegister;
