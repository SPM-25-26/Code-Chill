import { useState } from 'react';
import { useNavigate, useLocation } from 'react-router-dom';
import { useForm, Controller } from 'react-hook-form';
import { yupResolver } from '@hookform/resolvers/yup';
import * as yup from 'yup';

import TextField from '@mui/material/TextField';
import Button from '@mui/material/Button';
import IconButton from '@mui/material/IconButton';
import InputAdornment from '@mui/material/InputAdornment';
import Visibility from '@mui/icons-material/Visibility';
import VisibilityOff from '@mui/icons-material/VisibilityOff';
import Alert from '@mui/material/Alert';

import logoPng from '../../assets/SPM-Logo.png';
import logoWebp from '../../assets/SPM-Logo.webp';
import instance from '../../utilities/Utilities.js';
import '../styles/AuthenticationPage.css';

const schema = yup.object({
  email: yup.string().email('Enter a valid email').required('Email is required'),
  password: yup.string().required('Password is required')
}).required();

export default function LoginPage() {
  const navigate = useNavigate();
  const location = useLocation();
  const [showPassword, setShowPassword] = useState(false);
  const [errorMessage, setErrorMessage] = useState('');
  const registrationMessage = location.state?.message || '';
  const [loading, setLoading] = useState(false);

  const { control, handleSubmit, formState: { errors, isValid } } = useForm({
    mode: 'onChange',
    resolver: yupResolver(schema),
    defaultValues: { email: '', password: '' },
  });

  const checkPreferencesAndNavigate = async (token) => {
    try {
      const res = await instance.get('/api/preferences/response', {
        headers: { Authorization: `Bearer ${token}` }
      });

      const answers = res.data?.answers ?? {};
      const skipped = res.data?.skipped ?? false;

      if (!answers || Object.keys(answers).length === 0 || skipped) {
        navigate('/login/preferences-intro');
      } else {
        navigate('/login/dashboard');
      }
    } catch (err) {
      if (err.response && err.response.status === 404) {
        navigate('/login/preferences-intro');
      } else {
        navigate('/login/dashboard');
      }
    }
  };

  /**
   * Load user preferences and store them in localStorage
   * @param {*} token - JWT token for authentication
   */
  const loadPreferences = async (token) => {
    try {
      const res = await instance.get('/api/preferences/response', {
        headers: { Authorization: `Bearer ${token}` }
      });
      console.log('Preferences response:', res.data);

      const derived = res.data?.derivedPreferences ?? {};
      const skipped = res.data?.skipped ?? false;

      if (Object.keys(derived).length > 0 && !skipped) {
        localStorage.setItem('preference-wizard', JSON.stringify(derived));
        localStorage.setItem('has-preferences', 'true');
        console.log('Loaded preferences:', derived);
      } else {
        localStorage.removeItem('preference-wizard');
        localStorage.setItem('has-preferences', 'false');
      }
    } catch (err) {
      localStorage.removeItem('preference-wizard');
      localStorage.setItem('has-preferences', 'false');
    }
  };


  const onSubmit = async (data) => {
    setLoading(true);
    setErrorMessage('');
    try {
      const response = await instance.post('/api/Authentication/login', {
        email: data.email,
        password: data.password
      });

      if (response.status === 201 || response.data.token) {
        const { token, refreshToken, refreshTokenExpiresAt, userName } = response.data;
        localStorage.setItem('token', token);
        localStorage.setItem('refreshToken', refreshToken);
        localStorage.setItem('refreshTokenExpiresAt', refreshTokenExpiresAt);
        if (userName) localStorage.setItem('username', userName);
        try {
          const previousUser = sessionStorage.getItem('chatbotSessionUser');
          if (previousUser) {
            sessionStorage.removeItem(`chatbotMessages:${previousUser}`);
          }
          sessionStorage.removeItem('chatbotMessages');
          if (userName) {
            sessionStorage.removeItem(`chatbotMessages:${userName}`);
            sessionStorage.setItem('chatbotSessionUser', userName);
          } else {
            sessionStorage.removeItem('chatbotSessionUser');
          }
        } catch {
          // ignore storage errors
        }

        // Load user preferences
        await loadPreferences(token);

        // Check preferences and navigate accordingly
        await checkPreferencesAndNavigate(token);
      } else {
        setErrorMessage(response.data.message || 'Login failed');
      }
    } catch (error) {
      let msg = 'Something went wrong. Please try again.';
      if (error.response) {
        switch (error.response.status) {
          case 401: msg = 'Invalid credentials. Please try again.'; break;
          case 403: msg = 'Your account is not verified.'; break;
          case 500: msg = 'Server error. Please try again later.'; break;
          default: msg = error.response.data?.message || msg;
        }
      }
      setErrorMessage(msg);
    } finally {
      setLoading(false);
    }
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

      <h1 className="welcome-title">Welcome!</h1>
      <p className="welcome-subtitle">Tell us your preferences and get the best recommendations!</p>

      {registrationMessage && (
        <Alert severity="success" sx={{ mb: 2 }}>
          {registrationMessage}
        </Alert>
      )}

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

        <Button
          variant="text"
          fullWidth
          className="mt-2"
          onClick={() => navigate('/reset-password')}
        >
          Forgot your password?
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
  );
}
