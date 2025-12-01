import logoPng from './assets/SPM-Logo.png';
import logoWebp from './assets/SPM-Logo.webp';
import './App.css';

function App() {
  return (
    <div className="login-page d-flex justify-content-center align-items-center">
      <div className="login-card text-center shadow-sm">
        <div className="logo-row text-start w-100">
          <picture>
            <source srcSet={logoWebp} type="image/webp" />
            <img src={logoPng} alt="SPM Logo" className="logo-image" />
          </picture>
        </div>

        <h1 className="welcome-title">Welcome!</h1>
        <p className="welcome-subtitle">
          Tell us your preferences and get the best recommendations!
        </p>

        <form className="login-form w-100">
          <div className="mb-3">
            <input
              type="text"
              className="form-control pill-input"
              placeholder="Username"
              aria-label="Username"
            />
          </div>
          <div className="mb-4">
            <input
              type="password"
              className="form-control pill-input"
              placeholder="Password"
              aria-label="Password"
            />
          </div>
          <button type="button" className="btn login-btn w-100 mb-4">
            Login
          </button>
        </form>

        <div className="register-section w-100">
          <small className="text-muted d-block mb-2">Noch keinen Account?</small>
          <button type="button" className="btn register-btn">
            Register
          </button>
        </div>
      </div>
    </div>
  );
}

export default App;
