import { BrowserRouter as Router, Routes, Route, Navigate } from 'react-router-dom';
import LoginPage from './pages/authentication/LoginPage.js';
import RegistrationPage from './pages/authentication/RegistrationPage.js';
import HomePage from './pages/HomePage.js';
import ProtectedRoute from './utilities/ProtectedRoute.js';
import './App.css';
import ResetPasswordPage from './pages/authentication/ResetPasswordPage.js';
import ContentDetailPage from './pages/ContentDetailPage.js';
import PreferencesPage from './pages/PreferencesPage.js';
import AdminIngestionPage from './pages/AdminIngestionPage.js';
import PreferencesIntroPage from './pages/PreferencesIntroPage.js';

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
          path="/login/content-detail/:id"
          element={
            <ProtectedRoute>
              <ContentDetailPage />
            </ProtectedRoute>
          } />
        <Route
          path="/login/preferences-intro"
          element={
            <ProtectedRoute>
              <PreferencesIntroPage />
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
        <Route
          path="/login/admin/ingestion"
          element={
            <ProtectedRoute>
              <AdminIngestionPage />
            </ProtectedRoute>
          }
        />
      </Routes>
    </Router>
  );
}

export default App;
