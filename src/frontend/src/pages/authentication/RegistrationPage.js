import { useState, useMemo } from 'react';
import { useNavigate } from 'react-router-dom';
import logoPng from '../../assets/SPM-Logo.png';
import logoWebp from '../../assets/SPM-Logo.webp';
import '../styles/AuthenticationPage.css';
import {
    TextField,
    IconButton,
    InputAdornment,
    Button,
    LinearProgress,
    Typography,
    Box,
    Alert
} from '@mui/material';
import { Visibility, VisibilityOff } from '@mui/icons-material';
import instance from '../../utilities/Utilities.js';

function RegistrationPage() {
    const navigate = useNavigate();

    // Form fields
    const [username, setUsername] = useState('');
    const [email, setEmail] = useState('');
    const [password, setPassword] = useState('');
    const [confirmPassword, setConfirmPassword] = useState('');

    // Show/hide password
    const [showPassword, setShowPassword] = useState(false);
    const [showConfirmPassword, setShowConfirmPassword] = useState(false);

    const [errorMessage, setErrorMessage] = useState('');
    const [successMessage, setSuccessMessage] = useState('');
    const [loading, setLoading] = useState(false);

    // Email validation
    const emailValid = /^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(email);

    // Password requirements
    const passwordRequirements = useMemo(() => ({
        length: password.length >= 8,
        uppercase: /[A-Z]/.test(password),
        lowercase: /[a-z]/.test(password),
        number: /[0-9]/.test(password),
        special: /[!@#$%^&*(),.?":{}|<>]/.test(password)
    }), [password]);

    const passwordValid = Object.values(passwordRequirements).every(Boolean);
    const confirmValid = password === confirmPassword && passwordValid;

    // Password strength 0-100
    const passwordStrength = useMemo(() => {
        const passed = Object.values(passwordRequirements).filter(Boolean).length;
        return (passed / Object.keys(passwordRequirements).length) * 100;
    }, [passwordRequirements]);

    const getStrengthColor = () => {
        if (passwordStrength < 40) return 'error';
        if (passwordStrength < 80) return 'warning';
        return 'success';
    };

    const isFormValid = emailValid && passwordValid && confirmValid;

    /**
     * Handle registration form submission
     * Sends registration request to backend and redirects to login with a verification prompt
     * @returns 
     */
    const handleRegister = async () => {
        if (!isFormValid) return;
        setLoading(true);
        setErrorMessage('');
        // Ensure no auth tokens are present after registration
        localStorage.removeItem('token');
        localStorage.removeItem('refreshToken');
        localStorage.removeItem('refreshTokenExpiresAt');
        try {
            const payload = {
                email,
                password,
            };
            if (username.trim() !== '') {
                payload.userName = username;
            }

            const response = await instance.post('/api/Authentication/register', payload);

            if (response.status === 201) {
                setSuccessMessage('Registration successful! Please verify your email before logging in.');
                // Redirect to login with a clear message to verify email
                setTimeout(() => {
                    navigate('/login', {
                        state: {
                            registrationSuccess: true,
                            message: 'Registration successful! Please check your email to verify your account before logging in.'
                        }
                    });
                }, 800);
            } else {
                setErrorMessage(response.data.message || 'Registration failed');
            }
        } catch (err) {
            const msg = err.response?.data?.message || 'Something went wrong. Please try again.';
            setErrorMessage(msg);
        }
        setLoading(false);
    };

    return (
        <div className="page-container">
                {/* Logo */}
                <div className="logo-row text-center w-100 mb-3">
                    <picture>
                        <source srcSet={logoWebp} type="image/webp" />
                        <img src={logoPng} alt="SPM Logo" className="logo-image" />
                    </picture>
                </div>

                <h1 className="welcome-title">Create Account</h1>
                <p className="welcome-subtitle">Fill in your details to get started!</p>

                {/* Alerts */}
                {errorMessage && <Alert severity="error" sx={{ mb: 2 }}>{errorMessage}</Alert>}
                {successMessage && <Alert severity="success" sx={{ mb: 2 }}>{successMessage}</Alert>}

                <form className="login-form w-100" noValidate>
                    {/* Username */}
                    <TextField
                        variant="outlined"
                        label="Username (optional)"
                        fullWidth
                        className="mb-3"
                        value={username}
                        onChange={(e) => setUsername(e.target.value)}
                        InputProps={{ style: { fontStyle: 'italic', fontWeight: 500 } }}
                    />

                    {/* Email */}
                    <TextField
                        variant="outlined"
                        label="Email"
                        fullWidth
                        className="mb-3"
                        value={email}
                        onChange={(e) => setEmail(e.target.value)}
                        error={email !== '' && !emailValid}
                        helperText={email !== '' && !emailValid ? 'Enter a valid email' : ''}
                        InputProps={{ style: { fontStyle: 'italic', fontWeight: 500 } }}
                    />

                    {/* Password */}
                    <TextField
                        variant="outlined"
                        label="Password"
                        type={showPassword ? 'text' : 'password'}
                        fullWidth
                        className="mb-3"
                        value={password}
                        onChange={(e) => setPassword(e.target.value)}
                        error={password !== '' && !passwordValid}
                        helperText={password !== '' && !passwordValid ? 'Password does not meet requirements' : ''}
                        InputProps={{
                            endAdornment: (
                                <InputAdornment position="end">
                                    <IconButton onClick={() => setShowPassword(!showPassword)} edge="end">
                                        {showPassword ? <VisibilityOff /> : <Visibility />}
                                    </IconButton>
                                </InputAdornment>
                            ),
                            style: { fontStyle: 'italic', fontWeight: 500 }
                        }}
                    />
                    {password && (
                        <Box sx={{ mt: 1, mb: 2 }}>
                            <LinearProgress
                                variant="determinate"
                                value={passwordStrength}
                                color={getStrengthColor()}
                                sx={{ height: 8, borderRadius: 4 }}
                            />
                            <Typography variant="caption" color="textSecondary">
                                Password strength: {Math.round(passwordStrength)}%
                            </Typography>
                        </Box>
                    )}

                    {/* Confirm Password */}
                    <TextField
                        variant="outlined"
                        label="Confirm Password"
                        type={showConfirmPassword ? 'text' : 'password'}
                        fullWidth
                        className="mb-3"
                        value={confirmPassword}
                        onChange={(e) => setConfirmPassword(e.target.value)}
                        error={confirmPassword !== '' && !confirmValid}
                        helperText={confirmPassword !== '' && !confirmValid ? 'Passwords do not match' : ''}
                        InputProps={{
                            endAdornment: (
                                <InputAdornment position="end">
                                    <IconButton
                                        onClick={() => setShowConfirmPassword(!showConfirmPassword)}
                                        edge="end"
                                    >
                                        {showConfirmPassword ? <VisibilityOff /> : <Visibility />}
                                    </IconButton>
                                </InputAdornment>
                            ),
                            style: { fontStyle: 'italic', fontWeight: 500 }
                        }}
                    />

                    {/* Register Button */}
                    <Button
                        variant="contained"
                        fullWidth
                        className="login-btn"
                        disabled={!isFormValid || loading}
                        onClick={handleRegister}
                    >
                        {loading ? 'Registering...' : 'Register'}
                    </Button>
                </form>

                {/* Back to Login */}
                <div className="register-section w-100 mt-3">
                    <small className="text-muted d-block mb-2">Already have an account?</small>
                    <Button
                        variant="outlined"
                        fullWidth
                        className="register-btn"
                        onClick={() => navigate('/login')}
                    >
                        Login
                    </Button>
                </div>
            </div>
    );
}

export default RegistrationPage;
