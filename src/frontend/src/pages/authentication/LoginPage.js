import { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { useForm, Controller } from 'react-hook-form';
import { yupResolver } from '@hookform/resolvers/yup';
import * as yup from 'yup';

import TextField from '@mui/material/TextField';
import Button from '@mui/material/Button';
import IconButton from '@mui/material/IconButton';
import InputAdornment from '@mui/material/InputAdornment';
import Visibility from '@mui/icons-material/Visibility';
import VisibilityOff from '@mui/icons-material/VisibilityOff';

import logoPng from '../../assets/SPM-Logo.png';
import logoWebp from '../../assets/SPM-Logo.webp';
import instance from '../../utilities/Utilities.js';
import '../styles/AuthenticationPage.css';


/**
 * Validation schema for the login form
 * - Ensures the email is a valid format and required
 * - Ensures password is required
 */
const schema = yup.object({
  email: yup.string().email('Enter a valid email').required('Email is required'),
  password: yup.string().required('Password is required')
}).required();

export default function LoginPage() {
  const navigate = useNavigate();
  const [showPassword, setShowPassword] = useState(false);
  const [errorMessage, setErrorMessage] = useState('');
  const [loading, setLoading] = useState(false);

  const { control, handleSubmit, formState: { errors, isValid } } = useForm({
    mode: 'onChange',
    resolver: yupResolver(schema),
  });

  /**
     * Handle form submission
     * Sends login request to backend and stores tokens on success
     * Redirects user to the login/dashboard page
     */
  const onSubmit = async (data) => {
    setLoading(true);
    setErrorMessage('');
    try {
      const response = await instance.post('/api/Authentication/login', {
        email: data.email,
        password: data.password
      });

      if (response.status === 201 || response.data.token) {
        localStorage.setItem('token', response.data.token);
        localStorage.setItem('refreshToken', response.data.refreshToken);
        localStorage.setItem('refreshTokenExpiresAt', response.data.refreshTokenExpiresAt);
        if (response.data.userName) {
          localStorage.setItem('username', response.data.userName);
        }

        navigate('/login/dashboard'); // redirect to the home page after login
      } else {
        setErrorMessage(response.data.message || 'Login failed');
      }
    } catch (error) {
      let msg = 'Something went wrong. Please try again.';
      if (error.response) {
        switch (error.response.status) {
          case 401:
            msg = 'Invalid credentials. Please try again.';
            break;
          case 403:
            msg = 'Your account is not verified.';
            break;
          case 500:
            msg = 'Server error. Please try again later.';
            break;
          default:
            msg = error.response.data?.message || msg;
        }
      }
      setErrorMessage(msg);
    }
    setLoading(false);
  };

  return (
    <div className="-page-container d-flex justify-content-center align-items-center min-vh-100">
      <div className="page-card text-center shadow-sm">

        {/* Logo */}
        <div className="logo-row text-center w-100 mb-3">
          <picture>
            <source srcSet={logoWebp} type="image/webp" />
            <img src={logoPng} alt="SPM Logo" className="logo-image" />
          </picture>
        </div>

        <h1 className="welcome-title">Welcome!</h1>
        <p className="welcome-subtitle">Tell us your preferences and get the best recommendations!</p>

        {errorMessage && <div style={{ color: 'red', marginBottom: '1rem' }}>{errorMessage}</div>}

        <form onSubmit={handleSubmit(onSubmit)} className="login-form w-100">
          <Controller
            name="email"
            control={control}
            render={({ field }) => (
              <TextField
                {...field}
                label="Email"
                variant="outlined"
                fullWidth
                className="mb-3"
                error={!!errors.email}
                helperText={errors.email?.message}
              />
            )}
          />

          <Controller
            name="password"
            control={control}
            render={({ field }) => (
              <TextField
                {...field}
                type={showPassword ? 'text' : 'password'}
                label="Password"
                variant="outlined"
                fullWidth
                className="mb-3"
                error={!!errors.password}
                helperText={errors.password?.message}
                InputProps={{
                  endAdornment: (
                    <InputAdornment position="end">
                      <IconButton
                        onClick={() => setShowPassword(!showPassword)}
                        edge="end"
                      >
                        {showPassword ? <VisibilityOff /> : <Visibility />}
                      </IconButton>
                    </InputAdornment>
                  ),
                }}
              />
            )}
          />

          <Button
            type="submit"
            variant="contained"
            fullWidth
            className="login-btn"
            disabled={!isValid || loading}
          >
            {loading ? 'Logging in...' : 'Login'}
          </Button>
        </form>

        <div className="register-section w-100 mt-3">
          <small className="text-muted d-block mb-2">Don’t have an account yet?</small>
          <Button
            variant="outlined"
            fullWidth
            className="register-btn"
            onClick={() => navigate('/register')}
          >
            Register
          </Button>
        </div>
      </div>
    </div>
  );
}
