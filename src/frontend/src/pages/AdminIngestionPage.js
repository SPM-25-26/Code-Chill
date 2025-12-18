import { useEffect, useMemo, useState } from 'react';
import { useNavigate } from 'react-router-dom';
import {
  Alert,
  Box,
  Button,
  Card,
  CardContent,
  CircularProgress,
  Divider,
  MenuItem,
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableRow,
  TextField,
  Typography,
} from '@mui/material';
import instance from '../utilities/Utilities.js';
import './styles/AdminIngestionPage.css';

function formatDateTime(value) {
  if (!value) return '-';
  const date = new Date(value);
  if (Number.isNaN(date.getTime())) return String(value);
  return date.toLocaleString();
}

function AdminIngestionPage() {
  const navigate = useNavigate();
  const [municipalities, setMunicipalities] = useState([]);
  const [municipality, setMunicipality] = useState('');
  const [language, setLanguage] = useState('it');

  const [loadingMunicipalities, setLoadingMunicipalities] = useState(false);
  const [loadingSummary, setLoadingSummary] = useState(false);
  const [loadingIngest, setLoadingIngest] = useState(false);

  const [error, setError] = useState('');
  const [result, setResult] = useState(null);

  useEffect(() => {
    const loadMunicipalities = async () => {
      setLoadingMunicipalities(true);
      try {
        const response = await instance.get('/api/admin/ingestion/municipalities');
        setMunicipalities(Array.isArray(response.data) ? response.data : []);
      } catch {
        // Non-blocking: municipality can still be typed manually.
      } finally {
        setLoadingMunicipalities(false);
      }
    };

    loadMunicipalities();
  }, []);

  const summary = useMemo(() => {
    if (!result) return null;
    return {
      municipality: result.municipality ?? result.Municipality ?? '',
      articleCount: result.articleCount ?? result.ArticleCount ?? 0,
      eventCount: result.eventCount ?? result.EventCount ?? 0,
      routeCount: result.routeCount ?? result.RouteCount ?? 0,
      unassignedArticleCount: result.unassignedArticleCount ?? result.UnassignedArticleCount ?? 0,
      unassignedEventCount: result.unassignedEventCount ?? result.UnassignedEventCount ?? 0,
      unassignedRouteCount: result.unassignedRouteCount ?? result.UnassignedRouteCount ?? 0,
      municipalityProfileExists:
        result.municipalityProfileExists ?? result.MunicipalityProfileExists ?? false,
      runs: result.runs ?? result.Runs ?? [],
    };
  }, [result]);

  const isBusy = loadingMunicipalities || loadingSummary || loadingIngest;

  const loadSummary = async () => {
    setError('');

    if (!municipality.trim()) {
      setError('Municipality is required.');
      return;
    }

    setLoadingSummary(true);
    try {
      const response = await instance.get(
        `/api/admin/ingestion/municipalities/${encodeURIComponent(municipality.trim())}/summary`
      );
      setResult(response.data);
    } catch (e) {
      const msg =
        e?.response?.data?.message ||
        (typeof e?.response?.data === 'string' ? e.response.data : '') ||
        e?.message ||
        'Failed to load summary.';
      setError(msg);
    } finally {
      setLoadingSummary(false);
    }
  };

  const triggerIngestion = async () => {
    setError('');

    if (!municipality.trim()) {
      setError('Municipality is required.');
      return;
    }

    setLoadingIngest(true);
    try {
      await instance.post('/api/admin/ingest', {
        municipality: municipality.trim(),
        language,
      });
      await loadSummary();
    } catch (e) {
      const msg =
        e?.response?.data?.message ||
        (typeof e?.response?.data === 'string' ? e.response.data : '') ||
        e?.message ||
        'Failed to ingest data.';
      setError(msg);
    } finally {
      setLoadingIngest(false);
    }
  };

  return (
    <div className="ingestion-page">
      <div className="ingestion-header">
        <Button variant="text" onClick={() => navigate('/login/dashboard')}>
          Back to Dashboard
        </Button>
        <Typography variant="h4" sx={{ mt: 1 }}>
          Ingestion Verification
        </Typography>
        <Typography variant="body2" color="text.secondary" sx={{ mt: 0.5 }}>
          Choose a municipality and verify how many entities are stored in the database. Each
          ingestion run is recorded (municipality + language).
        </Typography>
      </div>

      <Card variant="outlined" className="ingestion-controls">
        <CardContent>
          <Box className="ingestion-controls-grid">
            {municipalities.length > 0 ? (
              <TextField
                label="Municipality"
                value={municipality}
                onChange={(e) => setMunicipality(e.target.value)}
                select
                fullWidth
              >
                {municipalities.map((m) => (
                  <MenuItem key={m} value={m}>
                    {m}
                  </MenuItem>
                ))}
              </TextField>
            ) : (
              <TextField
                label="Municipality"
                value={municipality}
                onChange={(e) => setMunicipality(e.target.value)}
                placeholder="e.g. Ascoli Piceno"
                fullWidth
              />
            )}

            <TextField
              label="Language"
              value={language}
              onChange={(e) => setLanguage(e.target.value)}
              select
              fullWidth
              helperText="Used when triggering a new ingestion"
            >
              <MenuItem value="it">it</MenuItem>
              <MenuItem value="en">en</MenuItem>
            </TextField>

            <Box className="ingestion-actions">
              <Button
                variant="outlined"
                onClick={loadSummary}
                disabled={isBusy}
                className="ingestion-action-btn"
              >
                {loadingSummary ? 'Loading…' : 'Load Summary'}
              </Button>
              <Button
                variant="contained"
                onClick={triggerIngestion}
                disabled={isBusy}
                className="ingestion-action-btn"
              >
                {loadingIngest ? 'Ingesting…' : 'Trigger Ingestion'}
              </Button>
            </Box>
          </Box>

          {isBusy && (
            <Box sx={{ display: 'flex', alignItems: 'center', gap: 1, mt: 2 }}>
              <CircularProgress size={20} />
              <Typography variant="body2">
                {loadingMunicipalities
                  ? 'Loading municipalities…'
                  : loadingIngest
                    ? 'Running ingestion…'
                    : 'Loading summary…'}
              </Typography>
            </Box>
          )}

          {error && (
            <Alert severity="error" sx={{ mt: 2 }}>
              {error}
            </Alert>
          )}
        </CardContent>
      </Card>

      {summary && (
        <Card variant="outlined" className="ingestion-summary">
          <CardContent>
            <Typography variant="h6" sx={{ mb: 1 }}>
              {summary.municipality}
            </Typography>
            <div className="ingestion-summary-grid">
              <div>
                <span className="ingestion-label">Profile in DB:</span>{' '}
                {summary.municipalityProfileExists ? 'Yes' : 'No'}
              </div>
              <div>
                <span className="ingestion-label">Articles (DB):</span> {summary.articleCount}
              </div>
              <div>
                <span className="ingestion-label">Events (DB):</span> {summary.eventCount}
              </div>
              <div>
                <span className="ingestion-label">Routes (DB):</span> {summary.routeCount}
              </div>
            </div>

            {(summary.unassignedArticleCount > 0 ||
              summary.unassignedEventCount > 0 ||
              summary.unassignedRouteCount > 0) && (
              <Alert severity="warning" sx={{ mt: 2 }}>
                Some stored records are missing municipality linkage (older ingestions). Trigger
                ingestion again for this municipality to update them.
                <div style={{ marginTop: 8 }}>
                  Unassigned — Articles: {summary.unassignedArticleCount}, Events:{' '}
                  {summary.unassignedEventCount}, Routes: {summary.unassignedRouteCount}
                </div>
              </Alert>
            )}

            <Divider sx={{ my: 3 }} />

            <Typography variant="h6" sx={{ mb: 1 }}>
              Ingestion Runs
            </Typography>

            {Array.isArray(summary.runs) && summary.runs.length > 0 ? (
              <Table size="small">
                <TableHead>
                  <TableRow>
                    <TableCell>Started</TableCell>
                    <TableCell>Completed</TableCell>
                    <TableCell>Language</TableCell>
                    <TableCell align="right">Articles</TableCell>
                    <TableCell align="right">Events</TableCell>
                    <TableCell align="right">Routes</TableCell>
                    <TableCell align="right">Errors</TableCell>
                  </TableRow>
                </TableHead>
                <TableBody>
                  {summary.runs.map((r) => (
                    <TableRow key={r.id ?? r.Id}>
                      <TableCell>{formatDateTime(r.startedAt ?? r.StartedAt)}</TableCell>
                      <TableCell>{formatDateTime(r.completedAt ?? r.CompletedAt)}</TableCell>
                      <TableCell>{r.language ?? r.Language ?? '-'}</TableCell>
                      <TableCell align="right">{r.articleCount ?? r.ArticleCount ?? 0}</TableCell>
                      <TableCell align="right">{r.eventCount ?? r.EventCount ?? 0}</TableCell>
                      <TableCell align="right">{r.routeCount ?? r.RouteCount ?? 0}</TableCell>
                      <TableCell align="right">
                        {Array.isArray(r.errors ?? r.Errors) ? (r.errors ?? r.Errors).length : 0}
                      </TableCell>
                    </TableRow>
                  ))}
                </TableBody>
              </Table>
            ) : (
              <Typography variant="body2" color="text.secondary">
                No ingestion runs recorded yet for this municipality.
              </Typography>
            )}
          </CardContent>
        </Card>
      )}
    </div>
  );
}

export default AdminIngestionPage;
