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
import './AuthenticationPage.css';

// Schema di validazione
const schema = yup.object({
  username: yup.string().required('Username is required'),
  password: yup.string().required('Password is required').min(8, 'Password must be at least 8 characters'),
}).required();

export default function LoginPage() {
  const navigate = useNavigate();
  const [showPassword, setShowPassword] = useState(false);

  const { control, handleSubmit, formState: { errors, isValid } } = useForm({
    mode: 'onChange',
    resolver: yupResolver(schema),
  });

  const onSubmit = (data) => {
    console.log('Login data:', data);
    // qui chiamerai la tua API login
  };

  return (
    <div className="login-page d-flex justify-content-center align-items-center min-vh-100">
      <div className="login-card text-center shadow-sm">

        {/* Logo */}
        <div className="logo-row text-center w-100 mb-3">
          <picture>
            <source srcSet={logoWebp} type="image/webp" />
            <img src={logoPng} alt="SPM Logo" className="logo-image" />
          </picture>
        </div>

        <h1 className="welcome-title">Welcome!</h1>
        <p className="welcome-subtitle">
          Tell us your preferences and get the best recommendations!
        </p>

        <form onSubmit={handleSubmit(onSubmit)} className="login-form w-100">
          <Controller
            name="username"
            control={control}
            render={({ field }) => (
              <TextField
                {...field}
                label="Username"
                variant="outlined"
                fullWidth
                className="mb-3"
                error={!!errors.username}
                helperText={errors.username?.message}
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
            color="primary"
            fullWidth
            disabled={!isValid}
            className="login-btn"
          >
            Login
          </Button>
        </form>

        <div className="register-section w-100 mt-3">
          <small className="text-muted d-block mb-2">
            Don’t have an account yet?
          </small>
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
