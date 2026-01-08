import { useState, useMemo } from 'react';
import { useNavigate } from 'react-router-dom';
import { TextField, Button, IconButton, InputAdornment, LinearProgress, Box, Typography } from '@mui/material';
import Visibility from '@mui/icons-material/Visibility';
import VisibilityOff from '@mui/icons-material/VisibilityOff';
import instance from '../../utilities/Utilities.js';
import logoPng from '../../assets/SPM-Logo.png';
import logoWebp from '../../assets/SPM-Logo.webp';

export default function ResetPasswordPage() {
    const navigate = useNavigate();

    const [email, setEmail] = useState('');
    const [password, setPassword] = useState('');
    const [confirmPassword, setConfirmPassword] = useState('');
    const [errorMessage, setErrorMessage] = useState('');
    const [successMessage, setSuccessMessage] = useState('');
    const [loading, setLoading] = useState(false);

    // Show/hide password
    const [showPassword, setShowPassword] = useState(false);
    const [showConfirmPassword, setShowConfirmPassword] = useState(false);

    // Email validation
    const emailValid = /^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(email);

    // Password complexity
    const passwordRequirements = useMemo(() => ({
        length: password.length >= 8,
        uppercase: /[A-Z]/.test(password),
        lowercase: /[a-z]/.test(password),
        number: /[0-9]/.test(password),
        special: /[!@#$%^&*(),.?":{}|<>]/.test(password),
    }), [password]);

    const passwordValid = Object.values(passwordRequirements).every(Boolean);
    const confirmValid = password === confirmPassword;

    // Strength calculation (0–100)
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

    const handleReset = async () => {
        if (!isFormValid) return;
        setLoading(true);
        setErrorMessage('');

        try {
            const response = await instance.post('/api/Authentication/reset-password', {
                email: email,
                newPassword: password
            });

            if (response.status === 200) {
                setSuccessMessage('Password updated successfully! Redirecting to login...');
                setTimeout(() => navigate('/login'), 1000);
            } else {
                setErrorMessage('Something went wrong. Try again.');
            }
        } catch (error) {
            const msg = error.response?.data || 'Server error. Try again later.';
            setErrorMessage(msg);
        }
        setLoading(false);
    };

    const passwordErrors = [];
    if (password.length > 0 && password.length < 8) passwordErrors.push("At least 8 characters");
    if (password.length > 0 && !/[A-Z]/.test(password)) passwordErrors.push("At least one uppercase letter");
    if (password.length > 0 && !/[a-z]/.test(password)) passwordErrors.push("At least one lowercase letter");
    if (password.length > 0 && !/[0-9]/.test(password)) passwordErrors.push("At least one number");
    if (password.length > 0 && !/[!@#$%^&*(),.?":{}|<>]/.test(password)) passwordErrors.push("At least one special character");


    return (
        <div className="page-container">

                {/* Logo */}
                <div className="logo-row text-center w-100 mb-3">
                    <picture>
                        <source srcSet={logoWebp} type="image/webp" />
                        <img src={logoPng} alt="SPM Logo" className="logo-image" />
                    </picture>
                </div>

                <h1 className="welcome-title">Reset Password</h1>
                <p className="welcome-subtitle">Enter your email and your new password.</p>

                {errorMessage && <p style={{ color: 'red', marginBottom: '1rem' }}>{errorMessage}</p>}
                {successMessage && <p style={{ color: 'green' }}>{successMessage}</p>}

                <form className="login-form w-100" noValidate>
                    {/* Email */}
                    <TextField
                        label="Email"
                        type="email"
                        fullWidth
                        className="mb-3"
                        value={email}
                        error={email.length > 0 && !emailValid}
                        helperText={email.length > 0 && !emailValid ? "Invalid email format." : ""}
                        onChange={(e) => setEmail(e.target.value)}
                    />

                    {/* New Password */}
                    <TextField
                        variant="outlined"
                        label="New Password"
                        type={showPassword ? "text" : "password"}
                        fullWidth
                        className="mb-3"
                        value={password}
                        onChange={(e) => setPassword(e.target.value)}
                        error={password !== '' && !passwordValid}
                        helperText={passwordErrors.join(", ")}
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

                    {/* Confirm password */}
                    <TextField
                        variant="outlined"
                        label="Confirm Password"
                        type={showConfirmPassword ? "text" : "password"}
                        fullWidth
                        className="mb-3"
                        value={confirmPassword}
                        onChange={(e) => setConfirmPassword(e.target.value)}
                        error={confirmPassword.length > 0 && !confirmValid}
                        helperText={
                            confirmPassword.length > 0 && !confirmValid
                                ? "Passwords do not match."
                                : ""
                        }
                        InputProps={{
                            endAdornment: (
                                <InputAdornment position="end">
                                    <IconButton onClick={() => setShowConfirmPassword(!showConfirmPassword)} edge="end">
                                        {showConfirmPassword ? <VisibilityOff /> : <Visibility />}
                                    </IconButton>
                                </InputAdornment>
                            ),
                            style: { fontStyle: 'italic', fontWeight: 500 }
                        }}
                    />

                    <Button
                        variant="contained"
                        fullWidth
                        className="reset-btn"
                        onClick={handleReset}
                        disabled={!isFormValid || loading}
                    >
                        {loading ? 'Saving...' : 'Save New Password'}
                    </Button>
                </form>

                <Button
                    variant="text"
                    fullWidth
                    className="mt-2"
                    onClick={() => navigate('/login')}
                >
                    Back to Login
                </Button>
            </div>
    );
}
