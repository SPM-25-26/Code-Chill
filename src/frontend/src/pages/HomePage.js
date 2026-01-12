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
    const [recommendationFilter, setRecommendationFilter] = useState('all'); // 'all', 'recommended', 'mightLike'

    const authHeaders = useMemo(() => ({
        Authorization: token ? `Bearer ${token}` : undefined,
    }), [token]);

    /** Carica contenuti da tutte le città */
    useEffect(() => {
        const loadData = async () => {
            try {
                const municipalitiesRes = await instance.get('/api/content/municipalities');
                const municipalities = municipalitiesRes.data;
                setAllMunicipalities(municipalities);

                const contentPromises = municipalities.map(muni =>
                    instance.get(`/api/content/municipalities/${muni}?language=${language}`)
                );
                const contentResponses = await Promise.all(contentPromises);

                const combinedContent = [];
                contentResponses.forEach(res => {
                    const data = res.data;
                    const municipality = data.municipality;
                    data.articles?.forEach(article => combinedContent.push({ ...article, contentType: 'article', municipality }));
                    data.events?.forEach(event => combinedContent.push({ ...event, contentType: 'event', municipality }));
                    data.routes?.forEach(route => combinedContent.push({ ...route, contentType: 'route', municipality }));
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
    }, [language]);

    /** Carica preferenze salvate localmente */
    useEffect(() => {
        try {
            const saved = localStorage.getItem('preference-wizard');
            if (saved) setPreferences(JSON.parse(saved));
        } catch (e) {
            console.warn('Failed to load saved preferences', e);
        }
    }, []);

    /** Aggiorna feed se il questionario modifica le preferenze */
    useEffect(() => {
        const handlePreferencesChange = (event) => {
            const detail = event?.detail;
            if (!detail) return;
            const derived = detail.derivedPreferences;
            const skipped = detail.skipped ?? false;

            if (derived && Object.keys(derived).length > 0 && !skipped) {
                setPreferences(derived);
                localStorage.setItem('preference-wizard', JSON.stringify(derived));
                localStorage.setItem('has-preferences', 'true');
            } else {
                console.log('Preferences empty or skipped, keeping previous values.');
            }
        };

        window.addEventListener('preferencesChanged', handlePreferencesChange);
        return () => window.removeEventListener('preferencesChanged', handlePreferencesChange);
    }, []);

    /** Carica preview con score dal backend */
    useEffect(() => {
        const loadPreview = async () => {
            if (!token) return;
            try {
                const res = await instance.get('/api/preferences/preview', { headers: authHeaders });
                const map = {};
                res.data?.articles?.forEach(a => map[a.id] = a.score);
                res.data?.events?.forEach(e => map[e.id] = e.score);
                res.data?.routes?.forEach(r => map[r.id] = r.score);
                setScoreMap(map);
            } catch (err) {
                console.error('Failed to load preview', err);
            }
        };
        loadPreview();
    }, [token, authHeaders]);

    /** Scroll collapse filter */
    useEffect(() => {
        const scrollContainer = document.querySelector('.scrollable-content');
        if (!scrollContainer) return;

        const handleScroll = () => {
            const scrollTop = scrollContainer.scrollTop;

            if (scrollTop > 0) {
                setFilterCollapsed(true);
            } else {
                setFilterCollapsed(false);
            }
        };

        scrollContainer.addEventListener('scroll', handleScroll);
        return () => scrollContainer.removeEventListener('scroll', handleScroll);
    }, []);


    const filteredContent = useMemo(() => {
        let filtered = allContent;

        if (selectedTypes.length > 0) {
            filtered = filtered.filter(item => selectedTypes.includes(item.contentType));
        }

        if (selectedCities.length > 0) {
            filtered = filtered.filter(item => selectedCities.includes(item.municipality));
        }

        if (preferences) {
            filtered = filtered.filter(item => {
                if (preferences.activity_type && item.type && !preferences.activity_type.includes(item.type)) return false;
                if (preferences.transport && item.travellingMethod && item.travellingMethod !== preferences.transport) return false;
                if (preferences.difficulty && item.difficulty && item.difficulty !== preferences.difficulty) return false;
                return true;
            });
        }

        filtered = filtered.filter(item => {
            const score = scoreMap[item.id ?? item.Id ?? item.identifier] ?? 0;
            if (recommendationFilter === 'recommended') return score >= 0.5;
            if (recommendationFilter === 'mightLike') return score < 0.5;
            return true; // 'all'
        });

        return filtered;
    }, [allContent, selectedTypes, selectedCities, preferences, scoreMap, recommendationFilter]);


    const handleRecommendationToggle = (_, newValue) => newValue && setRecommendationFilter(newValue);

    const sortedContent = useMemo(() => {
        const withScore = [...filteredContent];
        const getScore = (item) => {
            const id = item.id ?? item.Id ?? item.identifier;
            return id && scoreMap[id] != null ? scoreMap[id] : 0;
        };
        withScore.sort((a, b) => getScore(b) - getScore(a));
        return withScore;
    }, [filteredContent, scoreMap]);

    const handleLogout = () => { localStorage.clear(); navigate('/login'); };
    const handleCardClick = (item) => { navigate(`/login/content-detail/${item.id ?? item.Id}`); };
    const handleTypeToggle = (event, newTypes) => setSelectedTypes(newTypes);
    const handleCityChange = (event, newValue) => setSelectedCities(newValue);
    const handleLanguageToggle = () => changeLanguage(language === 'en' ? 'it' : 'en');

    const getContentIcon = (type) => {
        switch (type) {
            case 'article': return <ArticleIcon fontSize="small" />;
            case 'event': return <EventIcon fontSize="small" />;
            case 'route': return <MapIcon fontSize="small" />;
            default: return null;
        }
    };

    const getContentLabel = (type) => {
        switch (type) {
            case 'article': return t('contentTypes.article');
            case 'event': return t('contentTypes.event');
            case 'route': return t('contentTypes.route');
            default: return '';
        }
    };

    const formatDate = (dateString) => {
        if (!dateString) return '';
        const date = new Date(dateString);
        return date.toLocaleDateString(language === 'en' ? 'en-US' : 'it-IT', { day: 'numeric', month: 'short', year: 'numeric' });
    };

    const getCardDetails = (item) => {
        switch (item.contentType) {
            case 'article':
                return (
                    <>
                        {item.subtitle && <Typography variant="caption" sx={{ display: 'block', mb: 0.5, color: 'text.secondary' }}>{item.subtitle}</Typography>}
                        {item.region && <Box sx={{ display: 'flex', alignItems: 'center', gap: 0.5, mt: 0.5 }}><LocationOnIcon sx={{ fontSize: 14, color: 'var(--teal)' }} /><Typography variant="caption" sx={{ color: 'text.secondary' }}>{item.region}</Typography></Box>}
                    </>
                );
            case 'event':
                return (
                    <>
                        {item.startDate && <Box sx={{ display: 'flex', alignItems: 'center', gap: 0.5, mt: 0.5 }}><CalendarTodayIcon sx={{ fontSize: 14, color: 'var(--teal)' }} /><Typography variant="caption" sx={{ color: 'text.secondary' }}>{formatDate(item.startDate)}</Typography></Box>}
                        {item.address && <Box sx={{ display: 'flex', alignItems: 'center', gap: 0.5, mt: 0.5 }}><LocationOnIcon sx={{ fontSize: 14, color: 'var(--teal)' }} /><Typography variant="caption" sx={{ color: 'text.secondary', overflow: 'hidden', textOverflow: 'ellipsis', whiteSpace: 'nowrap' }}>{item.address}</Typography></Box>}
                    </>
                );
            case 'route':
                return (
                    <>
                        {item.duration && <Box sx={{ display: 'flex', alignItems: 'center', gap: 0.5, mt: 0.5 }}><AccessTimeIcon sx={{ fontSize: 14, color: 'var(--teal)' }} /><Typography variant="caption" sx={{ color: 'text.secondary' }}>{item.duration}</Typography></Box>}
                        {item.routeLength && <Box sx={{ display: 'flex', alignItems: 'center', gap: 0.5, mt: 0.5 }}><RouteIcon sx={{ fontSize: 14, color: 'var(--teal)' }} /><Typography variant="caption" sx={{ color: 'text.secondary' }}>{item.routeLength}</Typography></Box>}
                    </>
                );
            default: return null;
        }
    };

    const clearFilters = () => {
        setSelectedTypes([]);
        setSelectedCities([]);
        setRecommendationFilter('all');
    };

    /** Rendering */
    return (
        <Box className="page">

            {/* Drawer */}
            <Drawer anchor="left" open={open} onClose={() => setOpen(false)}>
                <Box className="drawer-container">
                    <Box className="drawer-header">
                        <Box className="drawer-header-icon"><AccountCircleIcon fontSize="small" /></Box>
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

            {/* Header */}
            <Box className="page-card-home">
                <Box className="header">
                    <IconButton onClick={() => setOpen(true)}><MenuIcon /></IconButton>
                    <img src={logoPng} alt="SPM Logo" className="logo" />
                    <Typography variant="h6">{t('header.welcome')}, {username}</Typography>
                    <Box sx={{ marginLeft: 'auto' }}>
                        <IconButton onClick={handleLanguageToggle} title={`Switch to ${language === 'en' ? 'Italian' : 'English'}`}>
                            <LanguageIcon />
                            <Typography variant="caption" sx={{ ml: 0.5, fontWeight: 600 }}>{language.toUpperCase()}</Typography>
                        </IconButton>
                    </Box>
                </Box>

                {/* Filter Toolbar */}
                <Box className={`filter-toolbar ${filterCollapsed ? 'collapsed' : ''}`} onClick={() => filterCollapsed && setFilterCollapsed(false)}>
                    <Box className="filter-header">
                        <FilterListIcon fontSize="small" />
                        <Typography variant="subtitle2">{t('filters.title')}</Typography>
                        <Typography variant="caption" sx={{ ml: 'auto', fontStyle: 'italic', opacity: 0.7 }}>
                            {selectedTypes.length === 0 && selectedCities.length === 0 ? t('filters.showingAll') : t('filters.filtered')}
                        </Typography>
                        {/* Clear All Button */}
                        <IconButton
                            size="small"
                            onClick={clearFilters}
                            title={t('filters.clearAll')}
                            sx={{ ml: 1 }}
                        >
                            <Typography variant="caption" sx={{ fontWeight: 400, fontStyle: 'Italic' }}>{t('filters.clearAll')}</Typography>
                        </IconButton>
                    </Box>

                    {/* Type Filter */}
                    <Box>
                        <Typography variant="caption" sx={{ fontWeight: 600, mb: 0.5, display: 'block' }}>{t('filters.contentType')}</Typography>
                        <ToggleButtonGroup value={selectedTypes} onChange={handleTypeToggle} aria-label="content type filter" size="small" className="filter-toggle-group">
                            <ToggleButton value="article" aria-label="articles" className="filter-toggle"><ArticleIcon fontSize="small" /><span>{t('contentTypes.articles')}</span></ToggleButton>
                            <ToggleButton value="event" aria-label="events" className="filter-toggle"><EventIcon fontSize="small" /><span>{t('contentTypes.events')}</span></ToggleButton>
                            <ToggleButton value="route" aria-label="routes" className="filter-toggle"><MapIcon fontSize="small" /><span>{t('contentTypes.routes')}</span></ToggleButton>
                        </ToggleButtonGroup>
                    </Box>

                    {/* City Filter */}
                    {allMunicipalities.length > 0 && (
                        <Box>
                            <Typography variant="caption" sx={{ fontWeight: 600, mb: 0.5, display: 'block' }}>{t('filters.municipality')}</Typography>
                            <Autocomplete
                                multiple
                                options={allMunicipalities}
                                value={selectedCities}
                                onChange={handleCityChange}
                                renderInput={(params) => <TextField {...params} placeholder={selectedCities.length === 0 ? t('filters.allMunicipalities') : ""} size="small" className="city-autocomplete" />}
                            />
                        </Box>
                    )}

                    <Box sx={{ mt: 1 }}>
                        <Typography variant="caption" sx={{ fontWeight: 600, mb: 0.5, display: 'block' }}>Consigli</Typography>
                        <ToggleButtonGroup
                            value={recommendationFilter}
                            exclusive
                            onChange={handleRecommendationToggle}
                            size="small"
                            className="filter-toggle-group"
                        >
                            <ToggleButton value="all" className="filter-toggle">{t('home.AllActivities')}</ToggleButton>
                            <ToggleButton value="recommended" className="filter-toggle">{t('home.recommendedForYou')}</ToggleButton>
                            <ToggleButton value="mightLike" className="filter-toggle">{t('home.mightLike')}</ToggleButton>
                        </ToggleButtonGroup>
                    </Box>
                </Box>

                {/* Feed */}
                <Box className="scrollable-content">
                    {loading && <CircularProgress />}
                    {error && <Alert severity="error">{error}</Alert>}
                    {!loading && !error && (
                        <>
                            {sortedContent.some(item => (scoreMap[item.id ?? item.Id ?? item.identifier] ?? 0) >= 0.5) && (
                                <>
                                    <Typography variant="h6" className="recommended-title">{t('home.recommendedForYou')}</Typography>
                                    <Box className="content-grid">
                                        {sortedContent.filter(item => (scoreMap[item.id ?? item.Id ?? item.identifier] ?? 0) >= 0.5).map(item => (
                                            <Card key={item.id ?? item.Id} className="content-card" onClick={() => handleCardClick(item)}>
                                                <Box className="card-type-badge">{getContentIcon(item.contentType)}<Typography variant="caption">{getContentLabel(item.contentType)}</Typography></Box>
                                                <CardMedia component="img" image={buildMediaUrl(item.imagePath ?? item.ImagePath ?? item.primaryImage ?? item.PrimaryImage)} alt={item.title ?? item.name} className="card-image" />
                                                <CardContent className="card-content">
                                                    <Typography variant="body2" className="card-title">{item.title ?? item.name}</Typography>
                                                    {getCardDetails(item)}
                                                </CardContent>
                                            </Card>
                                        ))}
                                    </Box>
                                </>
                            )}

                            {sortedContent.some(item => (scoreMap[item.id ?? item.Id ?? item.identifier] ?? 0) < 0.5) && (
                                <>
                                    <Typography variant="h6" className="recommended-title">{t('home.mightLike')}</Typography>
                                    <Box className="content-grid">
                                        {sortedContent.filter(item => (scoreMap[item.id ?? item.Id ?? item.identifier] ?? 0) < 0.5).map(item => (
                                            <Card key={item.id ?? item.Id} className="content-card" onClick={() => handleCardClick(item)}>
                                                <Box className="card-type-badge">{getContentIcon(item.contentType)}<Typography variant="caption">{getContentLabel(item.contentType)}</Typography></Box>
                                                <CardMedia component="img" image={buildMediaUrl(item.imagePath ?? item.ImagePath ?? item.primaryImage ?? item.PrimaryImage)} alt={item.title ?? item.name} className="card-image" />
                                                <CardContent className="card-content">
                                                    <Typography variant="body2" className="card-title">{item.title ?? item.name}</Typography>
                                                    {getCardDetails(item)}
                                                </CardContent>
                                            </Card>
                                        ))}
                                    </Box>
                                </>
                            )}

                            {sortedContent.length === 0 && <Alert severity="info">{t('messages.noActivities')}</Alert>}
                        </>
                    )}
                </Box>
            </Box>
        </Box>
    );
}