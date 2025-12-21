import { useNavigate } from 'react-router-dom';
import { Button } from '@mui/material';
import logoPng from '../assets/SPM-Logo.png';
import logoWebp from '../assets/SPM-Logo.webp';
import './styles/HomePage.css';

function HomePage() {
    const navigate = useNavigate();
    const username = localStorage.getItem('username') || 'User';

    const handleLogout = () => {
        localStorage.clear();
        navigate('/login');
    };

    return (
        <div className="page-container d-flex justify-content-center align-items-center min-vh-100">
            <div className="page-card text-center shadow-sm">
                {/* Logo */}
                <div className="logo-row text-center w-100 mb-3">
                    <picture>
                        <source srcSet={logoWebp} type="image/webp" />
                        <img src={logoPng} alt="SPM Logo" className="logo-image" />
                    </picture>
                </div>

                {/* Welcome message */}
                <h1 className="welcome-title">
                    Welcome {username}!
                </h1>
                <p className="welcome-subtitle">
                    You are now logged in.
                </p>

                {/* Navigation */}
                <div className="mt-3">
                    <Button
                        variant="outlined"
                        fullWidth
                        onClick={() => navigate('/login/admin/ingestion')}
                    >
                        Admin: Ingestion Verification
                    </Button>
                </div>
                <div className="mt-3">
                    <Button
                        variant="outlined"
                        fullWidth
                        onClick={() => navigate('/login/content')}
                    >
                        View Ingested Content
                    </Button>
                </div>
                <div className="mt-3">
                    <Button
                        variant="outlined"
                        fullWidth
                        onClick={() => navigate('/login/preferences')}
                    >
                        Update Preferences
                    </Button>
                </div>

                {/* Logout button */}
                <div className="mt-4">
                    <Button
                        variant="contained"
                        className='logout-btn'
                        fullWidth
                        onClick={handleLogout}
                    >
                        Logout
                    </Button>
                </div>
            </div>
        </div>
    );
}

export default HomePage;
