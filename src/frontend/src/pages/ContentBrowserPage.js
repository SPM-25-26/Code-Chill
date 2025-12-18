import { useEffect, useMemo, useState } from 'react';
import { useNavigate } from 'react-router-dom';
import {
  Alert,
  Box,
  Button,
  Card,
  CardContent,
  CardMedia,
  Chip,
  CircularProgress,
  MenuItem,
  TextField,
  Typography,
} from '@mui/material';
import instance from '../utilities/Utilities.js';
import './styles/ContentBrowserPage.css';

const MEDIA_BASE = 'https://eppoi.io';

function buildMediaUrl(path) {
  if (!path) return '';
  if (path.startsWith('http://') || path.startsWith('https://')) return path;
  return `${MEDIA_BASE}${path}`;
}

function ContentSection({ title, items, renderItem, emptyText }) {
  return (
    <Card variant="outlined" className="content-section">
      <CardContent>
        <Typography variant="h6" sx={{ mb: 1 }}>
          {title} <Chip label={items.length} size="small" sx={{ ml: 1 }} />
        </Typography>
        {items.length === 0 ? (
          <Typography variant="body2" color="text.secondary">
            {emptyText}
          </Typography>
        ) : (
          <div className="content-grid">{items.map(renderItem)}</div>
        )}
      </CardContent>
    </Card>
  );
}

function ContentBrowserPage() {
  const navigate = useNavigate();
  const [municipalities, setMunicipalities] = useState([]);
  const [selectedMunicipality, setSelectedMunicipality] = useState('');
  const [loadingMunicipalities, setLoadingMunicipalities] = useState(false);
  const [loadingContent, setLoadingContent] = useState(false);
  const [error, setError] = useState('');
  const [content, setContent] = useState(null);

  useEffect(() => {
    const loadMunicipalities = async () => {
      setLoadingMunicipalities(true);
      try {
        const response = await instance.get('/api/content/municipalities');
        const names = Array.isArray(response.data) ? response.data : [];
        setMunicipalities(names);
        if (names.length > 0) {
          setSelectedMunicipality(names[0]);
        }
      } catch (e) {
        setError('Failed to load municipalities.');
      } finally {
        setLoadingMunicipalities(false);
      }
    };
    loadMunicipalities();
  }, []);

  const loadContent = async () => {
    setError('');
    if (!selectedMunicipality.trim()) {
      setError('Select a municipality first.');
      return;
    }
    setLoadingContent(true);
    try {
      const response = await instance.get(
        `/api/content/municipalities/${encodeURIComponent(selectedMunicipality)}`
      );
      setContent(response.data);
    } catch (e) {
      setError('Failed to load content.');
    } finally {
      setLoadingContent(false);
    }
  };

  const articles = useMemo(() => content?.articles ?? content?.Articles ?? [], [content]);
  const events = useMemo(() => content?.events ?? content?.Events ?? [], [content]);
  const routes = useMemo(() => content?.routes ?? content?.Routes ?? [], [content]);

  return (
    <div className="content-page">
      <div className="content-header">
        <Button variant="text" onClick={() => navigate('/login/dashboard')}>
          Back to Dashboard
        </Button>
        <Typography variant="h4" sx={{ mt: 1 }}>
          Ingested Content
        </Typography>
        <Typography variant="body2" color="text.secondary" sx={{ mt: 0.5 }}>
          Browse stored Articles, Events, and Routes per municipality.
        </Typography>
      </div>

      <Card variant="outlined" className="content-controls">
        <CardContent>
          <Box className="content-controls-grid">
            <TextField
              label="Municipality"
              value={selectedMunicipality}
              onChange={(e) => setSelectedMunicipality(e.target.value)}
              select
              fullWidth
              helperText="Select a municipality to view its content"
            >
              {municipalities.map((m) => (
                <MenuItem key={m} value={m}>
                  {m}
                </MenuItem>
              ))}
            </TextField>
            <Button
              variant="contained"
              onClick={loadContent}
              disabled={loadingMunicipalities || loadingContent || !selectedMunicipality}
              className="content-load-btn"
            >
              {loadingContent ? 'Loading…' : 'Load Content'}
            </Button>
          </Box>

          {(loadingMunicipalities || loadingContent) && (
            <Box sx={{ display: 'flex', alignItems: 'center', gap: 1, mt: 2 }}>
              <CircularProgress size={20} />
              <Typography variant="body2">
                {loadingMunicipalities ? 'Loading municipalities…' : 'Loading content…'}
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

      {content && (
        <div className="content-sections">
          <ContentSection
            title="Articles"
            items={articles}
            emptyText="No articles for this municipality."
            renderItem={(item) => (
              <Card
                key={item.id ?? item.Id}
                variant="outlined"
                className="content-card"
              >
                {buildMediaUrl(item.imagePath ?? item.ImagePath) && (
                  <CardMedia
                    component="img"
                    height="140"
                    image={buildMediaUrl(item.imagePath ?? item.ImagePath)}
                    alt={item.title ?? item.Title}
                  />
                )}
                <CardContent>
                  <Typography variant="subtitle1" sx={{ mb: 0.5 }}>
                    {item.title ?? item.Title}
                  </Typography>
                  {(item.subtitle ?? item.Subtitle) && (
                    <Typography variant="body2" color="text.secondary" sx={{ mb: 0.5 }}>
                      {item.subtitle ?? item.Subtitle}
                    </Typography>
                  )}
                  {(item.region ?? item.Region) && (
                    <Typography variant="body2">
                      Region: {item.region ?? item.Region}
                    </Typography>
                  )}
                </CardContent>
              </Card>
            )}
          />

          <ContentSection
            title="Events"
            items={events}
            emptyText="No events for this municipality."
            renderItem={(item) => (
              <Card
                key={item.id ?? item.Id}
                variant="outlined"
                className="content-card"
              >
                {buildMediaUrl(item.primaryImage ?? item.PrimaryImage) && (
                  <CardMedia
                    component="img"
                    height="140"
                    image={buildMediaUrl(item.primaryImage ?? item.PrimaryImage)}
                    alt={item.title ?? item.Title}
                  />
                )}
                <CardContent>
                  <Typography variant="subtitle1" sx={{ mb: 0.5 }}>
                    {item.title ?? item.Title}
                  </Typography>
                  {(item.description ?? item.Description) && (
                    <Typography variant="body2" color="text.secondary" sx={{ mb: 0.5 }}>
                      {(item.description ?? item.Description).slice(0, 140)}
                      {(item.description ?? item.Description).length > 140 ? '…' : ''}
                    </Typography>
                  )}
                  {(item.address ?? item.Address) && (
                    <Typography variant="body2">
                      Address: {item.address ?? item.Address}
                    </Typography>
                  )}
                </CardContent>
              </Card>
            )}
          />

          <ContentSection
            title="Routes"
            items={routes}
            emptyText="No routes for this municipality."
            renderItem={(item) => (
              <Card
                key={item.id ?? item.Id}
                variant="outlined"
                className="content-card"
              >
                {buildMediaUrl(item.imagePath ?? item.ImagePath) && (
                  <CardMedia
                    component="img"
                    height="140"
                    image={buildMediaUrl(item.imagePath ?? item.ImagePath)}
                    alt={item.name ?? item.Name}
                  />
                )}
                <CardContent>
                  <Typography variant="subtitle1" sx={{ mb: 0.5 }}>
                    {item.name ?? item.Name}
                  </Typography>
                  {(item.description ?? item.Description) && (
                    <Typography variant="body2" color="text.secondary" sx={{ mb: 0.5 }}>
                      {(item.description ?? item.Description).slice(0, 140)}
                      {(item.description ?? item.Description).length > 140 ? '…' : ''}
                    </Typography>
                  )}
                  <Typography variant="body2">
                    Theme:{' '}
                    {item.pathTheme ?? item.PathTheme ? item.pathTheme ?? item.PathTheme : '-'} ·
                    Method:{' '}
                    {item.travellingMethod ?? item.TravellingMethod
                      ? item.travellingMethod ?? item.TravellingMethod
                      : '-'}
                  </Typography>
                </CardContent>
              </Card>
            )}
          />
        </div>
      )}
    </div>
  );
}

export default ContentBrowserPage;
