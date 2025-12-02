import { BrowserRouter as Router, Routes, Route } from 'react-router-dom';
import LoginPage from './pages/authentication/LoginPage.js';
import RegistrationPage from './pages/authentication/RegistrationPage.js';
import HomePage from './pages/HomePage.js';
import ProtectedRoute from './utilities/ProtectedRoute.js';
import './App.css';

function App() {
  return (
    <Router>
      <Routes>
        <Route path="/" element={<LoginPage />} />
        <Route path="/register" element={<RegistrationPage />} />

        {/* Route protetta */}
        <Route
          path="/home"
          element={
            <ProtectedRoute>
              <HomePage />
            </ProtectedRoute>
          }
        />
      </Routes>
    </Router>
  );
}

export default App;
