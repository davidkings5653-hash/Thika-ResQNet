import React from 'react';
import 'bootstrap/dist/css/bootstrap.min.css';

const MobileNavbar = () => {
  return (
    <nav className="navbar navbar-expand-lg navbar-dark bg-dark mb-3">
      <div className="container-fluid">
        <a className="navbar-brand" href="#">Thika ResQNet</a>
        <button className="navbar-toggler" type="button" data-bs-toggle="collapse" data-bs-target="#thikaNav" aria-controls="thikaNav" aria-expanded="false" aria-label="Toggle navigation">
          <span className="navbar-toggler-icon"></span>
        </button>
        <div className="collapse navbar-collapse" id="thikaNav">
          <ul className="navbar-nav me-auto mb-2 mb-lg-0">
            <li className="nav-item"><a className="nav-link" href="#dashboard">Dashboard</a></li>
            <li className="nav-item"><a className="nav-link" href="#incidents">Incidents</a></li>
            <li className="nav-item"><a className="nav-link" href="#dispatch">Dispatch</a></li>
          </ul>
          <ul className="navbar-nav ms-auto">
            <li className="nav-item"><a className="nav-link" href="#profile">Profile</a></li>
            <li className="nav-item"><a className="nav-link" href="#logout">Logout</a></li>
          </ul>
        </div>
      </div>
    </nav>
  );
};

export default MobileNavbar;
