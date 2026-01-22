import { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import {
  Alert,
  Box,
  Button,
  Card,
  CardContent,
  CircularProgress,
  Typography,
} from '@mui/material';
import instance from '../utilities/Utilities.js';
import './styles/AdminLlmHealthPage.css';

function AdminLlmHealthPage() {
  const navigate = useNavigate();
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState('');
  const [reply, setReply] = useState('');

  const runHealthCheck = async () => {
    setError('');
    setReply('');
    setLoading(true);
    try {
      const token = localStorage.getItem('token');
      const response = await instance.get('/api/chat/health', {
        headers: token ? { Authorization: `Bearer ${token}` } : undefined,
      });
      setReply(response.data?.reply ?? '');
    } catch (e) {
      const msg =
        e?.response?.data?.message ||
        (typeof e?.response?.data === 'string' ? e.response.data : '') ||
        e?.message ||
        'Health check failed.';
      setError(msg);
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="llm-health-page">
      <div className="llm-health-header">
        <Button variant="text" onClick={() => navigate('/login/dashboard')}>
          Back to Dashboard
        </Button>
        <Typography variant="h4" sx={{ mt: 1 }}>
          LLM Health Check
        </Typography>
        <Typography variant="body2" color="text.secondary" sx={{ mt: 0.5 }}>
          Verify the LLM connection and prompt pipeline.
        </Typography>
      </div>

      <Card variant="outlined" className="llm-health-card">
        <CardContent>
          <Box className="llm-health-actions">
            <Button variant="contained" onClick={runHealthCheck} disabled={loading}>
              {loading ? 'Checking…' : 'Run Health Check'}
            </Button>
          </Box>

          {loading && (
            <Box sx={{ display: 'flex', alignItems: 'center', gap: 1, mt: 2 }}>
              <CircularProgress size={20} />
              <Typography variant="body2">Contacting LLM…</Typography>
            </Box>
          )}

          {error && (
            <Alert severity="error" sx={{ mt: 2 }}>
              {error}
            </Alert>
          )}

          {reply && (
            <Alert severity="success" sx={{ mt: 2 }}>
              <Typography variant="subtitle2" sx={{ fontWeight: 'bold', mb: 1 }}>
                LLM Reply
              </Typography>
              <Typography variant="body2">{reply}</Typography>
            </Alert>
          )}
        </CardContent>
      </Card>
    </div>
  );
}

export default AdminLlmHealthPage;
