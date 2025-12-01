import { BrowserRouter as Router, Routes, Route } from 'react-router-dom';
import LoginPage from './pages/authentication/LoginPage.js';
import RegistrationPage from './pages/authentication/RegistrationPage.js';
import './App.css'; 

function App() {
  return (
    <Router>
      <Routes>
        <Route path="/" element={<LoginPage />} />
        <Route path="/register" element={<RegistrationPage />} />
      </Routes>
    </Router>
  );
}

export default App;
