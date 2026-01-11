import { useEffect, useMemo, useState } from 'react';
import { useNavigate } from 'react-router-dom';
import {
    Alert,
    Box,
    Card,
    CardContent,
    CardMedia,
    CircularProgress,
    Typography,
    Drawer,
    List,
    ListItemButton,
    ListItemIcon,
    ListItemText,
    IconButton,
    ToggleButtonGroup,
    ToggleButton,
    Autocomplete,
    TextField
} from '@mui/material';
import SettingsIcon from '@mui/icons-material/Settings';
import LogoutIcon from '@mui/icons-material/Logout';
import ArticleIcon from '@mui/icons-material/Article';
import EventIcon from '@mui/icons-material/Event';
import MapIcon from '@mui/icons-material/Map';
import MenuIcon from '@mui/icons-material/Menu';
import AccountCircleIcon from '@mui/icons-material/AccountCircle';
import FilterListIcon from '@mui/icons-material/FilterList';
import AdminPanelSettingsIcon from '@mui/icons-material/AdminPanelSettings';
import LanguageIcon from '@mui/icons-material/Language';
import LocationOnIcon from '@mui/icons-material/LocationOn';
import AccessTimeIcon from '@mui/icons-material/AccessTime';
import RouteIcon from '@mui/icons-material/Route';
import CalendarTodayIcon from '@mui/icons-material/CalendarToday';

import logoPng from '../assets/SPM-Logo.png';
import instance from '../utilities/Utilities.js';
import { useTranslation } from '../hooks/useTranslation.js';
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
    const token = localStorage.getItem('token') || '';
    const { t, language, changeLanguage } = useTranslation();

    const [allMunicipalities, setAllMunicipalities] = useState([]);
    const [allContent, setAllContent] = useState([]);
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState('');
    const [open, setOpen] = useState(false);
    const [preferences, setPreferences] = useState(null);
    const [scoreMap, setScoreMap] = useState({});
    const [selectedTypes, setSelectedTypes] = useState([]);
    const [selectedCities, setSelectedCities] = useState([]);
    const [filterCollapsed, setFilterCollapsed] = useState(false);

    const authHeaders = useMemo(
        () => ({
            Authorization: token ? `Bearer ${token}` : undefined,
        }),
        [token],
    );

    // Load municipalities and content from all cities
    useEffect(() => {
        const loadData = async () => {
            try {
                // Fetch list of municipalities
                const municipalitiesRes = await instance.get('/api/content/municipalities');
                const municipalities = municipalitiesRes.data;
                setAllMunicipalities(municipalities);

                // Fetch content from all municipalities
                const contentPromises = municipalities.map(muni =>
                    instance.get(`/api/content/municipalities/${muni}?language=${language}`)
                );
                const contentResponses = await Promise.all(contentPromises);

                // Combine all content
                const combinedContent = [];
                contentResponses.forEach(res => {
                    const data = res.data;
                    const municipality = data.municipality;

                    data.articles?.forEach(article => combinedContent.push({
                        ...article,
                        contentType: 'article',
                        municipality
                    }));
                    data.events?.forEach(event => combinedContent.push({
                        ...event,
                        contentType: 'event',
                        municipality
                    }));
                    data.routes?.forEach(route => combinedContent.push({
                        ...route,
                        contentType: 'route',
                        municipality
                    }));
                });

                setAllContent(combinedContent);
            } catch (err) {
                console.error('Failed to load content:', err);
                setError(t('messages.loadError'));
            } finally {
                setLoading(false);
            }
        };
        loadData();
    }, [language]); // Reload when language changes

    // Load user preferences
    useEffect(() => {
        try {
            const saved = localStorage.getItem('preference-wizard');
            if (saved) setPreferences(JSON.parse(saved));
        } catch (e) {
            console.warn('Failed to load saved preferences', e);
        }
    }, []);

    // Load scored preview from backend to sort feed
    useEffect(() => {
        const loadPreview = async () => {
            if (!token) return;
            try {
                const res = await instance.get('/api/preferences/preview', { headers: authHeaders });
                const preview = res.data;
                const map = {};
                preview?.articles?.forEach((a) => {
                    const id = a.id ?? a.Id ?? a.identifier;
                    if (id) map[id] = a.score ?? a.Score ?? 0;
                });
                preview?.events?.forEach((e) => {
                    const id = e.id ?? e.Id ?? e.identifier;
                    if (id) map[id] = e.score ?? e.Score ?? 0;
                });
                preview?.routes?.forEach((r) => {
                    const id = r.id ?? r.Id ?? r.identifier;
                    if (id) map[id] = r.score ?? r.Score ?? 0;
                });
                setScoreMap(map);
            } catch (err) {
                console.warn('Failed to load preference preview', err);
            }
        };
        loadPreview();
    }, [authHeaders, token]);

    // Handle scroll to collapse filter
    useEffect(() => {
        const scrollContainer = document.querySelector('.scrollable-content');
        if (!scrollContainer) return;

        let lastScrollTop = 0;
        const threshold = 50;
        let ticking = false;

        const handleScroll = () => {
            if (!ticking) {
                window.requestAnimationFrame(() => {
                    const scrollTop = scrollContainer.scrollTop;

                    if (scrollTop - lastScrollTop > threshold) {
                        setFilterCollapsed(true);
                        lastScrollTop = scrollTop;
                    }
                    else if (lastScrollTop - scrollTop > threshold) {
                        setFilterCollapsed(false);
                        lastScrollTop = scrollTop;
                    }

                    ticking = false;
                });
                ticking = true;
            }
        };

        scrollContainer.addEventListener('scroll', handleScroll);
        return () => scrollContainer.removeEventListener('scroll', handleScroll);
    }, []);

    // Filter content based on selections and preferences
    const filteredContent = useMemo(() => {
        let filtered = allContent;

        // Filter by selected content types (inverted: empty = show all, selected = show only selected)
        if (selectedTypes.length > 0) {
            filtered = filtered.filter(item => selectedTypes.includes(item.contentType));
        }

        // Filter by selected cities (inverted: empty = show all, selected = show only selected)
        if (selectedCities.length > 0) {
            filtered = filtered.filter(item => selectedCities.includes(item.municipality));
        }

        // Filter by user preferences
        if (preferences) {
            filtered = filtered.filter(item => {
                if (preferences.activity_type && item.type && !preferences.activity_type.includes(item.type)) return false;
                if (preferences.transport && item.travellingMethod && item.travellingMethod !== preferences.transport) return false;
                if (preferences.difficulty && item.difficulty && item.difficulty !== preferences.difficulty) return false;
                return true;
            });
        }

        return filtered;
    }, [allContent, selectedTypes, selectedCities, preferences]);

    const sortedContent = useMemo(() => {
        const withScore = [...filteredContent];
        const getScore = (item) => {
            const id = item.id ?? item.Id ?? item.identifier;
            return id && scoreMap[id] != null ? scoreMap[id] : 0;
        };
        withScore.sort((a, b) => getScore(b) - getScore(a));
        return withScore;
    }, [filteredContent, scoreMap]);

    const handleLogout = () => {
        localStorage.clear();
        navigate('/login');
    };

    const handleCardClick = (item) => {
        if (!item.id && !item.Id) {
            console.error('Missing POI id', item);
            return;
        }
        navigate(`/login/content-detail/${item.id ?? item.Id}`);
    };

    const handleTypeToggle = (event, newTypes) => {
        setSelectedTypes(newTypes);
    };

    const handleCityChange = (event, newValue) => {
        setSelectedCities(newValue);
    };

    const handleLanguageToggle = () => {
        const newLanguage = language === 'en' ? 'it' : 'en';
        changeLanguage(newLanguage);
        // Reload content will happen automatically via useEffect dependency
    };

    const getContentIcon = (contentType) => {
        switch (contentType) {
            case 'article': return <ArticleIcon fontSize="small" />;
            case 'event': return <EventIcon fontSize="small" />;
            case 'route': return <MapIcon fontSize="small" />;
            default: return null;
        }
    };

    const getContentLabel = (contentType) => {
        switch (contentType) {
            case 'article': return t('contentTypes.article');
            case 'event': return t('contentTypes.event');
            case 'route': return t('contentTypes.route');
            default: return '';
        }
    };

    const formatDate = (dateString) => {
        if (!dateString) return '';
        const date = new Date(dateString);
        return date.toLocaleDateString(language === 'en' ? 'en-US' : 'it-IT', {
            day: 'numeric',
            month: 'short',
            year: 'numeric'
        });
    };

    const getCardDetails = (item) => {
        switch (item.contentType) {
            case 'article':
                return (
                    <>
                        {item.subtitle && (
                            <Typography variant="caption" className="card-subtitle" sx={{ display: 'block', mb: 0.5, color: 'text.secondary' }}>
                                {item.subtitle}
                            </Typography>
                        )}
                        {item.region && (
                            <Box sx={{ display: 'flex', alignItems: 'center', gap: 0.5, mt: 0.5 }}>
                                <LocationOnIcon sx={{ fontSize: 14, color: 'var(--teal)' }} />
                                <Typography variant="caption" sx={{ color: 'text.secondary' }}>
                                    {item.region}
                                </Typography>
                            </Box>
                        )}
                    </>
                );
            case 'event':
                return (
                    <>
                        {item.startDate && (
                            <Box sx={{ display: 'flex', alignItems: 'center', gap: 0.5, mt: 0.5 }}>
                                <CalendarTodayIcon sx={{ fontSize: 14, color: 'var(--teal)' }} />
                                <Typography variant="caption" sx={{ color: 'text.secondary' }}>
                                    {formatDate(item.startDate)}
                                </Typography>
                            </Box>
                        )}
                        {item.address && (
                            <Box sx={{ display: 'flex', alignItems: 'center', gap: 0.5, mt: 0.5 }}>
                                <LocationOnIcon sx={{ fontSize: 14, color: 'var(--teal)' }} />
                                <Typography variant="caption" sx={{ color: 'text.secondary', overflow: 'hidden', textOverflow: 'ellipsis', whiteSpace: 'nowrap' }}>
                                    {item.address}
                                </Typography>
                            </Box>
                        )}
                    </>
                );
            case 'route':
                return (
                    <>
                        {item.duration && (
                            <Box sx={{ display: 'flex', alignItems: 'center', gap: 0.5, mt: 0.5 }}>
                                <AccessTimeIcon sx={{ fontSize: 14, color: 'var(--teal)' }} />
                                <Typography variant="caption" sx={{ color: 'text.secondary' }}>
                                    {item.duration}
                                </Typography>
                            </Box>
                        )}
                        {item.routeLength && (
                            <Box sx={{ display: 'flex', alignItems: 'center', gap: 0.5, mt: 0.5 }}>
                                <RouteIcon sx={{ fontSize: 14, color: 'var(--teal)' }} />
                                <Typography variant="caption" sx={{ color: 'text.secondary' }}>
                                    {item.routeLength}
                                </Typography>
                            </Box>
                        )}
                    </>
                );
            default:
                return null;
        }
    };

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
                            <ListItemText primary={t('menu.preferences')} />
                        </ListItemButton>
                        <ListItemButton onClick={() => { setOpen(false); navigate('/login/admin/ingestion'); }}>
                            <ListItemIcon><AdminPanelSettingsIcon /></ListItemIcon>
                            <ListItemText primary={t('menu.adminIngestion')} />
                        </ListItemButton>
                        <ListItemButton onClick={handleLogout}>
                            <ListItemIcon><LogoutIcon /></ListItemIcon>
                            <ListItemText primary={t('menu.logout')} />
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
                        {t('header.welcome')}, {username}
                    </Typography>
                    <Box sx={{ marginLeft: 'auto' }}>
                        <IconButton
                            onClick={handleLanguageToggle}
                            className="language-button"
                            title={`Switch to ${language === 'en' ? 'Italian' : 'English'}`}
                        >
                            <LanguageIcon />
                            <Typography variant="caption" sx={{ ml: 0.5, fontWeight: 600 }}>
                                {language.toUpperCase()}
                            </Typography>
                        </IconButton>
                    </Box>
                </Box>

                {/* Filter Toolbar */}
                <Box
                    className={`filter-toolbar ${filterCollapsed ? 'collapsed' : ''}`}
                    onClick={() => filterCollapsed && setFilterCollapsed(false)}
                >
                    <Box className="filter-header">
                        <FilterListIcon fontSize="small" />
                        <Typography variant="subtitle2">{t('filters.title')}</Typography>
                        <Typography variant="caption" sx={{ ml: 'auto', fontStyle: 'italic', opacity: 0.7 }}>
                            {selectedTypes.length === 0 && selectedCities.length === 0 ? t('filters.showingAll') : t('filters.filtered')}
                        </Typography>
                    </Box>

                    {/* Content Type Filter */}
                    <Box>
                        <Typography variant="caption" sx={{ fontWeight: 600, mb: 0.5, display: 'block' }}>
                            {t('filters.contentType')}
                        </Typography>
                        <ToggleButtonGroup
                            value={selectedTypes}
                            onChange={handleTypeToggle}
                            aria-label="content type filter"
                            size="small"
                            className="filter-toggle-group"
                        >
                            <ToggleButton value="article" aria-label="articles" className="filter-toggle">
                                <ArticleIcon fontSize="small" />
                                <span>{t('contentTypes.articles')}</span>
                            </ToggleButton>
                            <ToggleButton value="event" aria-label="events" className="filter-toggle">
                                <EventIcon fontSize="small" />
                                <span>{t('contentTypes.events')}</span>
                            </ToggleButton>
                            <ToggleButton value="route" aria-label="routes" className="filter-toggle">
                                <MapIcon fontSize="small" />
                                <span>{t('contentTypes.routes')}</span>
                            </ToggleButton>
                        </ToggleButtonGroup>
                    </Box>

                    {/* City Filter */}
                    {allMunicipalities.length > 0 && (
                        <Box>
                            <Typography variant="caption" sx={{ fontWeight: 600, mb: 0.5, display: 'block' }}>
                                {t('filters.municipality')}
                            </Typography>
                            <Autocomplete
                                multiple
                                options={allMunicipalities}
                                value={selectedCities}
                                onChange={handleCityChange}
                                renderInput={(params) => (
                                    <TextField
                                        {...params}
                                        placeholder={selectedCities.length === 0 ? t('filters.allMunicipalities') : ""}
                                        size="small"
                                        className="city-autocomplete"
                                    />
                                )}
                                sx={{
                                    '& .MuiOutlinedInput-root': {
                                        borderRadius: '999px',
                                        backgroundColor: 'var(--surface)',
                                        border: '2px solid var(--teal)',
                                        '&:hover': {
                                            backgroundColor: 'rgba(46, 196, 182, 0.05)',
                                        },
                                        '&.Mui-focused': {
                                            backgroundColor: 'var(--surface)',
                                            boxShadow: '0 4px 12px rgba(46, 196, 182, 0.2)',
                                        },
                                        '& fieldset': {
                                            border: 'none',
                                        },
                                    },
                                    '& .MuiAutocomplete-tag': {
                                        borderRadius: '999px',
                                        background: 'linear-gradient(135deg, var(--teal) 0%, var(--teal-dark) 100%)',
                                        color: '#ffffff',
                                        border: '2px solid var(--dark)',
                                        fontWeight: 600,
                                    },
                                    '& .MuiChip-deleteIcon': {
                                        color: '#ffffff',
                                        '&:hover': {
                                            color: 'rgba(255, 255, 255, 0.8)',
                                        },
                                    },
                                }}
                            />
                        </Box>
                    )}
                </Box>

                <Box className="scrollable-content">
                    {loading && (
                        <Box className="loading">
                            <CircularProgress size={24} />
                            <Typography variant="body2">{t('messages.loading')}</Typography>
                        </Box>
                    )}

                    {error && <Alert severity="error">{error}</Alert>}

                    {!loading && !error && (
                        <>
                            {sortedContent.length > 0 ? (
                                <Box className="content-grid">
                                    {sortedContent.map((item) => (
                                        <Card
                                            key={item.id ?? item.Id}
                                            className="content-card"
                                            onClick={() => handleCardClick(item)}
                                        >
                                            <Box className="card-type-badge">
                                                {getContentIcon(item.contentType)}
                                                <Typography variant="caption">
                                                    {getContentLabel(item.contentType)}
                                                </Typography>
                                            </Box>
                                            <CardMedia
                                                component="img"
                                                image={buildMediaUrl(item.imagePath ?? item.ImagePath ?? item.primaryImage ?? item.PrimaryImage)}
                                                alt={item.title ?? item.name}
                                                className="card-image"
                                            />
                                            <CardContent className="card-content">
                                                <Typography variant="body2" className="card-title">
                                                    {item.title ?? item.name}
                                                </Typography>
                                                {getCardDetails(item)}
                                            </CardContent>
                                        </Card>
                                    ))}
                                </Box>
                            ) : (
                                <Alert severity="info">{t('messages.noActivities')}</Alert>
                            )}
                        </>
                    )}
                </Box>
            </Box>
        </Box>
    );
}
