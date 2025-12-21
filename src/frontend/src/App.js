import { BrowserRouter as Router, Routes, Route, Navigate } from 'react-router-dom';
import LoginPage from './pages/authentication/LoginPage.js';
import RegistrationPage from './pages/authentication/RegistrationPage.js';
import HomePage from './pages/HomePage.js';
import ProtectedRoute from './utilities/ProtectedRoute.js';
import './App.css';
import ResetPasswordPage from './pages/authentication/ResetPasswordPage.js';
import AdminIngestionPage from './pages/AdminIngestionPage.js';
import ContentBrowserPage from './pages/ContentBrowserPage.js';
import PreferencesPage from './pages/PreferencesPage.js';

function App() {
  return (
    <Router>
      <Routes>
        <Route path="/" element={<Navigate to="/login" />} /> {/* default route */}
        <Route path="/login" element={<LoginPage />} />
        <Route path="/register" element={<RegistrationPage />} />
        <Route path="/reset-password" element={<ResetPasswordPage />} />

        {/* Route protetta */}
        <Route
          path="/login/dashboard"
          element={
            <ProtectedRoute>
              <HomePage />
            </ProtectedRoute>
          }
        />

        <Route
          path="/login/admin/ingestion"
          element={
            <ProtectedRoute>
              <AdminIngestionPage />
            </ProtectedRoute>
          }
        />
        <Route
          path="/login/content"
          element={
            <ProtectedRoute>
              <ContentBrowserPage />
            </ProtectedRoute>
          }
        />
        <Route
          path="/login/preferences"
          element={
            <ProtectedRoute>
              <PreferencesPage />
            </ProtectedRoute>
          }
        />
      </Routes>
    </Router>
  );
}

export default App;
