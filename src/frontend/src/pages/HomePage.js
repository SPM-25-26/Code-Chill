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
    Typography,
    Divider,
    Drawer,
    List,
    ListItemButton,
    ListItemIcon,
    ListItemText
} from '@mui/material';
import SettingsIcon from '@mui/icons-material/Settings';
import LogoutIcon from '@mui/icons-material/Logout';
import logoPng from '../assets/SPM-Logo.png';
import instance from '../utilities/Utilities.js';
import './styles/HomePage.css';

import ArticleIcon from '@mui/icons-material/Article';
import EventIcon from '@mui/icons-material/Event';
import MapIcon from '@mui/icons-material/Map';
import MenuIcon from '@mui/icons-material/Menu';
import IconButton from '@mui/material/IconButton';
import AccountCircleIcon from '@mui/icons-material/AccountCircle';


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
    const [open, setOpen] = useState(false);
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState('');

    useEffect(() => {
        const loadContent = async () => {
            try {
                const res = await instance.get(
                    '/api/content/municipalities/Comune di Castignano'
                );
                setContent(res.data);
            } catch {
                setError('Failed to load content.');
            } finally {
                setLoading(false);
            }
        };
        loadContent();
    }, []);

    const articles = useMemo(() => content?.articles ?? [], [content]);
    const events = useMemo(() => content?.events ?? [], [content]);
    const routes = useMemo(() => content?.routes ?? [], [content]);

    const handleLogout = () => {
        localStorage.clear();
        navigate('/login');
    };

    const handleCardClick = (item, type) => {
        localStorage.setItem(
            'selectedContent',
            JSON.stringify({ ...item, type })
        );
        navigate('/login/content-detail');
    };

    const renderSection = (title, items, type) => (
        <Box className="section">
            <Box className="section-header">
                <Typography variant="subtitle1">{title}</Typography>
                <Chip label={items.length} size="small" />
            </Box>

            {items.length === 0 ? (
                <Typography variant="body2" color="text.secondary">
                    No {title.toLowerCase()} available for this municipality.
                </Typography>
            ) : (
                <Box className="horizontal-scroll">
                    {items.map((item) => (
                        <Card
                            key={item.id ?? item.Id}
                            className="content-card"
                            onClick={() => handleCardClick(item, type)}
                        >
                            <CardMedia
                                component="img"
                                height="120"
                                image={buildMediaUrl(
                                    item.imagePath ??
                                    item.ImagePath ??
                                    item.primaryImage ??
                                    item.PrimaryImage
                                )}
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

                <Chip
                    label="Comune di Castignano"
                    size="small"
                    variant="outlined"
                />

                <Box className="scrollable-content">
                    {loading && (
                        <Box className="loading">
                            <CircularProgress size={24} />
                            <Typography variant="body2">Loading content…</Typography>
                        </Box>
                    )}

                    {error && <Alert severity="error">{error}</Alert>}

                    {!loading && content && (
                        <>
                            {renderSection(<Typography variant="subtitle1">
                                <ArticleIcon fontSize="small" /> Articles
                            </Typography>, articles, 'article')}

                            <Divider />
                            {renderSection(<Typography variant="subtitle1">
                                <EventIcon fontSize="small" /> Events
                            </Typography>, events, 'event')}
                            <Divider />
                            {renderSection(<Typography variant="subtitle1">
                                <MapIcon fontSize="small" /> Routes
                            </Typography>, routes, 'route')}
                        </>
                    )}

                    {/* <Box className="actions">
                    <Button
                        variant="outlined"
                        fullWidth
                        onClick={() => navigate('/login/preferences')}
                    >
                        Update Preferences
                    </Button>
                    <Button
                        variant="contained"
                        fullWidth
                        onClick={handleLogout}
                    >
                        Logout
                    </Button>
                </Box> */}
                </Box>
            </Box>
        </Box>
    );
}
