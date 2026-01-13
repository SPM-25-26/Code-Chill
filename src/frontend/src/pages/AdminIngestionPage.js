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
  const [selectedMunicipality, setSelectedMunicipality] = useState('');
  const [customMunicipality, setCustomMunicipality] = useState('');
  const [language, setLanguage] = useState('it');

  const [loadingMunicipalities, setLoadingMunicipalities] = useState(false);
  const [loadingSummary, setLoadingSummary] = useState(false);
  const [loadingIngest, setLoadingIngest] = useState(false);

  const [error, setError] = useState('');
  const [result, setResult] = useState(null);
  const [lastIngestionResult, setLastIngestionResult] = useState(null);

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

    const muni = (customMunicipality || selectedMunicipality).trim();
    if (!muni) {
      setError('Select a municipality or enter a new one.');
      return;
    }

    setLoadingSummary(true);
    try {
      const response = await instance.get(
        `/api/admin/ingestion/municipalities/${encodeURIComponent(muni)}/summary`
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
    setLastIngestionResult(null);

    const muni = (customMunicipality || selectedMunicipality).trim();
    if (!muni) {
      setError('Select a municipality or enter a new one.');
      return;
    }

    setLoadingIngest(true);
    try {
      const response = await instance.post('/api/admin/ingest', {
        municipality: muni,
        language,
      });
      setLastIngestionResult(response.data);
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
            <TextField
              label="Ingested municipalities"
              value={selectedMunicipality}
              onChange={(e) => setSelectedMunicipality(e.target.value)}
              select
              fullWidth
              helperText="Pick an already ingested city (optional)"
            >
              <MenuItem value="">(none)</MenuItem>
              {municipalities.map((m) => (
                <MenuItem key={m} value={m}>
                  {m}
                </MenuItem>
              ))}
            </TextField>

            <TextField
              label="Or enter a new municipality"
              value={customMunicipality}
              onChange={(e) => setCustomMunicipality(e.target.value)}
              placeholder="e.g. Ascoli Piceno"
              fullWidth
              helperText="Used for ingestion if provided"
            />

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

          {lastIngestionResult && (
            <Alert
              severity={lastIngestionResult.errors?.length > 0 ? 'warning' : 'success'}
              sx={{ mt: 2 }}
            >
              <Typography variant="subtitle2" sx={{ fontWeight: 'bold', mb: 1 }}>
                Ingestion completed for {lastIngestionResult.municipality}
              </Typography>
              <Box sx={{ display: 'grid', gridTemplateColumns: 'repeat(2, 1fr)', gap: 1, mb: 1 }}>
                <div>📄 Articles: {lastIngestionResult.articleCount || 0}</div>
                <div>🎉 Events: {lastIngestionResult.eventCount || 0}</div>
                <div>🚶 Routes: {lastIngestionResult.routeCount || 0}</div>
                <div>📍 Total POIs: {lastIngestionResult.totalPoiCount || 0}</div>
              </Box>
              {lastIngestionResult.totalPoiCount > 0 && lastIngestionResult.poiCounts && (
                <Box sx={{ mt: 1 }}>
                  <Typography variant="caption" sx={{ fontWeight: 'bold' }}>POIs by Category:</Typography>
                  <Box sx={{ display: 'grid', gridTemplateColumns: 'repeat(2, 1fr)', gap: 0.5, mt: 0.5, fontSize: '0.875rem' }}>
                    {Object.entries(lastIngestionResult.poiCounts).map(([category, count]) => (
                      <div key={category}>• {category}: {count}</div>
                    ))}
                  </Box>
                </Box>
              )}
              {lastIngestionResult.errors?.length > 0 && (
                <Box sx={{ mt: 1, pt: 1, borderTop: '1px solid rgba(0,0,0,0.1)' }}>
                  <Typography variant="caption" sx={{ fontWeight: 'bold' }}>
                    Errors ({lastIngestionResult.errors.length}):
                  </Typography>
                  <ul style={{ margin: '4px 0 0 0', paddingLeft: '20px', fontSize: '0.75rem' }}>
                    {lastIngestionResult.errors.map((err, idx) => (
                      <li key={idx}>{err}</li>
                    ))}
                  </ul>
                </Box>
              )}
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
                    <TableCell align="right">POIs</TableCell>
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
                      <TableCell align="right">{r.totalPoiCount ?? r.TotalPoiCount ?? 0}</TableCell>
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
