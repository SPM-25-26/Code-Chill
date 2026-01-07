import { useEffect, useMemo, useState } from 'react';
import { useNavigate } from 'react-router-dom';
import {
    Alert,
    Box,
    Card,
    CardContent,
    CardMedia,
    Chip,
    CircularProgress,
    Typography,
    Divider,
    Drawer,
    List,
    ListItemButton,
    ListItemIcon,
    ListItemText,
    IconButton
} from '@mui/material';
import SettingsIcon from '@mui/icons-material/Settings';
import LogoutIcon from '@mui/icons-material/Logout';
import ArticleIcon from '@mui/icons-material/Article';
import EventIcon from '@mui/icons-material/Event';
import MapIcon from '@mui/icons-material/Map';
import MenuIcon from '@mui/icons-material/Menu';
import AccountCircleIcon from '@mui/icons-material/AccountCircle';

import logoPng from '../assets/SPM-Logo.png';
import instance from '../utilities/Utilities.js';
import './styles/HomePage.css';

const MEDIA_BASE = 'https://eppoi.io';
function buildMediaUrl(path) {
    if (!path) return '';
    if (path.startsWith('http')) return path;
    return `${MEDIA_BASE}${path}`;
}

export default function HomePage() {
    const navigate = useNavigate();
    const username = localStorage.getItem('username') || 'User';

    const [content, setContent] = useState(null);
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState('');
    const [open, setOpen] = useState(false);
    const [preferences, setPreferences] = useState(null);

    // Carica contenuti
    useEffect(() => {
        const loadContent = async () => {
            try {
                const res = await instance.get('/api/content/municipalities/Comune di Castignano');
                setContent(res.data);
            } catch {
                setError('Failed to load content.');
            } finally {
                setLoading(false);
            }
        };
        loadContent();
    }, []);

    // Carica preferenze utente
    useEffect(() => {
        try {
            const saved = localStorage.getItem('preference-wizard');
            if (saved) setPreferences(JSON.parse(saved));
        } catch (e) {
            console.warn('Failed to load saved preferences', e);
        }
    }, []);

    const articles = useMemo(() => content?.articles ?? [], [content]);
    const events = useMemo(() => content?.events ?? [], [content]);
    const routes = useMemo(() => content?.routes ?? [], [content]);

    // Filtra contenuti secondo preferenze
    const filterByPreferences = (items) => {
        if (!preferences) return items; // nessun filtro
        return items.filter(item => {
            // Esempi di filtri: type, pathTheme, travellingMethod ecc.
            if (preferences.activity_type && item.type && !preferences.activity_type.includes(item.type)) return false;
            if (preferences.transport && item.travellingMethod && item.travellingMethod !== preferences.transport) return false;
            if (preferences.difficulty && item.difficulty && item.difficulty !== preferences.difficulty) return false;
            return true;
        });
    };

    const filteredArticles = useMemo(() => filterByPreferences(articles), [articles, preferences]);
    const filteredEvents = useMemo(() => filterByPreferences(events), [events, preferences]);
    const filteredRoutes = useMemo(() => filterByPreferences(routes), [routes, preferences]);

    const handleLogout = () => {
        localStorage.clear();
        navigate('/login');
    };

    const handleCardClick = (item) => {
        if (!item.id) {
            console.error('Missing POI id', item);
            return;
        }
        navigate(`/login/content-detail/${item.id}`);
    };

    const renderSection = (title, items, type) => (
        <Box className="section">
            <Box className="section-header">
                <Typography variant="subtitle1">{title}</Typography>
                <Chip label={items.length} size="small" />
            </Box>

            {items.length === 0 ? (
                <Typography variant="body2" color="text.secondary">
                    No {title.toLowerCase()} match your preferences.
                </Typography>
            ) : (
                <Box className="horizontal-scroll">
                    {items.map((item) => (
                        <Card
                            key={item.id ?? item.Id}
                            className="content-card"
                            onClick={() => handleCardClick(item)}
                        >
                            <CardMedia
                                component="img"
                                height="120"
                                image={buildMediaUrl(item.imagePath ?? item.ImagePath ?? item.primaryImage ?? item.PrimaryImage)}
                                alt={item.title ?? item.name}
                            />
                            <CardContent>
                                <Typography variant="body2" noWrap>
                                    {item.title ?? item.name}
                                </Typography>
                            </CardContent>
                        </Card>
                    ))}
                </Box>
            )}
        </Box>
    );

    return (
        <Box className="page">
            {/* Drawer Menu */}
            <Drawer anchor="left" open={open} onClose={() => setOpen(false)}>
                <Box className="drawer-container">
                    <Box className="drawer-header">
                        <Box className="drawer-header-icon">
                            <AccountCircleIcon fontSize="small" />
                        </Box>
                        <Typography variant="subtitle1">{username}</Typography>
                    </Box>

                    <List className="drawer-list">
                        <ListItemButton onClick={() => { setOpen(false); navigate('/login/preferences'); }}>
                            <ListItemIcon><SettingsIcon /></ListItemIcon>
                            <ListItemText primary="Preferences" />
                        </ListItemButton>
                        <ListItemButton onClick={handleLogout}>
                            <ListItemIcon><LogoutIcon /></ListItemIcon>
                            <ListItemText primary="Logout" />
                        </ListItemButton>
                    </List>
                </Box>
            </Drawer>

            <Box className="page-card-home" >
                <Box className="header">
                    <IconButton onClick={() => setOpen(true)}>
                        <MenuIcon />
                    </IconButton>
                    <img src={logoPng} alt="SPM Logo" className="logo" />
                    <Typography variant="h6">
                        Welcome, {username}
                    </Typography>
                </Box>

                <Chip label="Comune di Castignano" size="small" variant="outlined" />

                <Box className="scrollable-content">
                    {loading && (
                        <Box className="loading">
                            <CircularProgress size={24} />
                            <Typography variant="body2">Loading content…</Typography>
                        </Box>
                    )}

                    {error && <Alert severity="error">{error}</Alert>}

                    {!loading && !error && (
                        <>
                            {(filteredArticles.length || filteredEvents.length || filteredRoutes.length) ? (
                                <>
                                    {renderSection(<><ArticleIcon fontSize="small" /> Articles</>, filteredArticles, 'article')}
                                    <Divider />
                                    {renderSection(<><EventIcon fontSize="small" /> Events</>, filteredEvents, 'event')}
                                    <Divider />
                                    {renderSection(<><MapIcon fontSize="small" /> Routes</>, filteredRoutes, 'route')}
                                </>
                            ) : (
                                <Alert severity="info">No activities match your preferences for this municipality.</Alert>
                            )}
                        </>
                    )}
                </Box>
            </Box>
        </Box>
    );
}
