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
    TextField,
    Tab,
    Tabs,
    Pagination
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
import PlaceIcon from '@mui/icons-material/Place';
import RestaurantIcon from '@mui/icons-material/Restaurant';
import MuseumIcon from '@mui/icons-material/Museum';
import ParkIcon from '@mui/icons-material/Park';
import ShoppingBagIcon from '@mui/icons-material/ShoppingBag';
import HotelIcon from '@mui/icons-material/Hotel';
import LocalOfferIcon from '@mui/icons-material/LocalOffer';
import BuildIcon from '@mui/icons-material/Build';
import TheaterComedyIcon from '@mui/icons-material/TheaterComedy';
import BusinessIcon from '@mui/icons-material/Business';
import SupportAgentIcon from '@mui/icons-material/SupportAgent';
import SearchIcon from '@mui/icons-material/Search';
import CloseIcon from '@mui/icons-material/Close';
import InputAdornment from '@mui/material/InputAdornment';

import logoPng from '../assets/SPM-Logo.png';
import instance from '../utilities/Utilities.js';
import { useTranslation } from '../hooks/useTranslation.js';
import ChatbotOverlay from '../components/ChatbotOverlay.js';
import './styles/HomePage.css';

const MEDIA_BASE = 'https://eppoi.io';
const DISTANCE_THRESHOLD_KM = 1000;

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
    const [sortNearMe, setSortNearMe] = useState(false);
    const [chatOpen, setChatOpen] = useState(false);
    const [searchQuery, setSearchQuery] = useState('');

    const [tabValue, setTabValue] = useState('all');

    const ITEMS_PER_PAGE = 18;
    const [page, setPage] = useState(1);
    const handleTabChange = (_, newValue) => {
        setTabValue(newValue);
        setPage(1);
    };

    const [userLocation, setUserLocation] = useState(null);
    const [locationDenied, setLocationDenied] = useState(false);

    const authHeaders = useMemo(() => ({
        Authorization: token ? `Bearer ${token}` : undefined,
    }), [token]);

    const getItemCoords = (item) => {
        const lat = item.latitude ?? item.Latitude;
        const lng = item.longitude ?? item.Longitude;
        return typeof lat === 'number' && typeof lng === 'number'
            ? { lat, lng }
            : null;
    };

    const distanceKm = (a, b) => {
        const R = 6371;
        const toRad = (v) => (v * Math.PI) / 180;
        const dLat = toRad(b.lat - a.lat);
        const dLng = toRad(b.lng - a.lng);
        const lat1 = toRad(a.lat);
        const lat2 = toRad(b.lat);
        const h =
            Math.sin(dLat / 2) * Math.sin(dLat / 2)
            + Math.cos(lat1) * Math.cos(lat2) * Math.sin(dLng / 2) * Math.sin(dLng / 2);
        return 2 * R * Math.asin(Math.min(1, Math.sqrt(h)));
    };

    const getDistanceFromUser = (item) => {
        if (!userLocation) return null;
        const coords = getItemCoords(item);
        if (!coords) return null;
        return distanceKm(userLocation, coords);
    };

    const getRegion = (item) => item.region ?? item.Region ?? 'Marche';

    const renderDistanceOrRegion = (item) => {
        const distance = getDistanceFromUser(item);
        const region = getRegion(item);

        if (distance != null && distance <= DISTANCE_THRESHOLD_KM) {
            return (
                <Box sx={{ display: 'flex', alignItems: 'center', gap: 0.5, mt: 0.5 }}>
                    <LocationOnIcon sx={{ fontSize: 14, color: 'var(--teal)' }} />
                    <Typography variant="caption" sx={{ color: 'text.secondary' }}>
                        {distance.toFixed(1)} km
                    </Typography>
                </Box>
            );
        }
        else if (region) {
            return (
                <Box sx={{ display: 'flex', alignItems: 'center', gap: 0.5, mt: 0.5 }}>
                    <LocationOnIcon sx={{ fontSize: 14, color: 'var(--teal)' }} />
                    <Typography variant="caption" sx={{ color: 'text.secondary' }}>
                        {region}
                    </Typography>
                </Box>
            );
        }
        return null;
    };

    const getEventBoost = (item) => {
        if (item.contentType !== 'event') return 0;
        const start = item.startDate ?? item.StartDate;
        const end = item.endDate ?? item.EndDate;
        if (!start || !end) return 0;
        const startDate = new Date(start);
        const endDate = new Date(end);
        if (Number.isNaN(startDate.getTime()) || Number.isNaN(endDate.getTime())) return 0;
        const now = new Date();
        if (now >= startDate && now <= endDate) return 0.2;
        const upcomingWindowMs = 7 * 24 * 60 * 60 * 1000;
        if (startDate > now && startDate.getTime() - now.getTime() <= upcomingWindowMs) return 0.2;
        return 0;
    };

    const getEventStatusLabel = (item) => {
        if (item.contentType !== 'event') return null;
        const start = item.startDate ?? item.StartDate;
        const end = item.endDate ?? item.EndDate;
        if (!start || !end) return null;
        const startDate = new Date(start);
        const endDate = new Date(end);
        if (Number.isNaN(startDate.getTime()) || Number.isNaN(endDate.getTime())) return null;
        const now = new Date();
        if (now >= startDate && now <= endDate) return t('home.eventLive');
        const upcomingWindowMs = 7 * 24 * 60 * 60 * 1000;
        if (startDate > now && startDate.getTime() - now.getTime() <= upcomingWindowMs) {
            return t('home.eventUpcoming');
        }
        return null;
    };

    /** Load all content for all municipalities */
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
                    data.pois?.forEach(poi => combinedContent.push({ ...poi, contentType: poi.category, municipality }));
                    data.organizations?.forEach(org => combinedContent.push({ ...org, contentType: 'organization', municipality }));
                    data.services?.forEach(service => combinedContent.push({ ...service, contentType: 'service', municipality }));
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

    /** Get user location */
    useEffect(() => {
        if (!navigator.geolocation || userLocation) return;

        navigator.geolocation.getCurrentPosition(
            (pos) => {
                setUserLocation({
                    lat: pos.coords.latitude,
                    lng: pos.coords.longitude
                });
            },
            () => setLocationDenied(true),
            { enableHighAccuracy: true, timeout: 5000 }
        );
    }, []);

    /** Load saved preferences from localStorage */
    useEffect(() => {
        try {
            const saved = localStorage.getItem('preference-wizard');
            if (saved) setPreferences(JSON.parse(saved));
        } catch (e) {
            console.warn('Failed to load saved preferences', e);
        }
    }, []);

    /** Listen for preference changes */
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

    /** Load preview scores */
    useEffect(() => {
        const loadPreview = async () => {
            if (!token) return;
            try {
                const res = await instance.get('/api/preferences/preview', { headers: authHeaders });
                const map = {};
                res.data?.articles?.forEach(a => map[a.id] = a.score);
                res.data?.events?.forEach(e => map[e.id] = e.score);
                res.data?.routes?.forEach(r => map[r.id] = r.score);
                res.data?.pois?.forEach(p => map[p.id] = p.score);
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

    useEffect(() => {
        const scrollContainer = document.querySelector('.scrollable-content');
        if (!scrollContainer) return;

        scrollContainer.scrollTo({ top: 0, behavior: 'smooth' });
    }, [page, tabValue]);

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

        if (searchQuery.trim().length > 0) {
            const q = searchQuery.trim().toLowerCase();
            filtered = filtered.filter(item => {
                const title = (item.title ?? item.name ?? item.legalName ?? '').toString().toLowerCase();
                return title.includes(q);
            });
        }

        return filtered;
    }, [allContent, selectedTypes, selectedCities, preferences, searchQuery]);

    const getTabContent = useMemo(() => {
        const score = (item) => scoreMap[item.id ?? item.Id ?? item.identifier ?? item.taxCode] ?? 0;

        if (tabValue === 'recommended') {
            return filteredContent.filter(item => score(item) >= 0.5);
        }

        if (tabValue === 'mightLike') {
            return filteredContent.filter(item => score(item) < 0.5);
        }

        return filteredContent;
    }, [filteredContent, scoreMap, tabValue]);

    const sortedContent = useMemo(() => {
        const withScore = [...getTabContent];
        const getScore = (item) => {
            const id = item.id ?? item.Id ?? item.identifier ?? item.taxCode;
            return id && scoreMap[id] != null ? scoreMap[id] : 0;
        };
        const getRankingScore = (item) => getScore(item) + getEventBoost(item);

        if (sortNearMe && userLocation) {
            withScore.sort((a, b) => {
                const coordsA = getItemCoords(a);
                const coordsB = getItemCoords(b);
                if (!coordsA && !coordsB) return getRankingScore(b) - getRankingScore(a);
                if (!coordsA) return 1;
                if (!coordsB) return -1;
                return distanceKm(userLocation, coordsA) - distanceKm(userLocation, coordsB);
            });
        } else {
            withScore.sort((a, b) => getRankingScore(b) - getRankingScore(a));
        }
        return withScore;
    }, [getTabContent, scoreMap, sortNearMe, userLocation]);

    // Paginated content
    const paginatedContent = useMemo(() => {
        const start = (page - 1) * ITEMS_PER_PAGE;
        const end = start + ITEMS_PER_PAGE;
        return sortedContent.slice(start, end);
    }, [sortedContent, page]);

    const handleLogout = () => {
        try {
            const storageKey = username ? `chatbotMessages:${username}` : 'chatbotMessages';
            sessionStorage.removeItem(storageKey);
            sessionStorage.removeItem('chatbotSessionUser');
        } catch {
            // ignore storage errors
        }
        localStorage.clear();
        navigate('/login');
    };
    const handleCardClick = (item) => {
        const id = item.id ?? item.Id ?? item.identifier ?? item.taxCode;
        if (!id) return;
        navigate(`/login/content-detail/${id}`);
    };
    const handleTypeToggle = (_, newTypes) => setSelectedTypes(newTypes);
    const handleCityChange = (_, newValue) => setSelectedCities(newValue);
    const handleLanguageToggle = () => changeLanguage(language === 'en' ? 'it' : 'en');
    const toggleNearMe = () => setSortNearMe((prev) => !prev);
    const toggleChat = () => setChatOpen((prev) => !prev);

    const getContentIcon = (type) => {
        switch (type) {
            case 'article': return <ArticleIcon fontSize="small" />;
            case 'event': return <EventIcon fontSize="small" />;
            case 'route': return <MapIcon fontSize="small" />;
            case 'organization': return <BusinessIcon fontSize="small" />;
            case 'service': return <BuildIcon fontSize="small" />;
            case 'ArtCulture': return <MuseumIcon fontSize="small" />;
            case 'EatAndDrink': return <RestaurantIcon fontSize="small" />;
            case 'EntertainmentLeisure': return <TheaterComedyIcon fontSize="small" />;
            case 'Nature': return <ParkIcon fontSize="small" />;
            case 'Services': return <BuildIcon fontSize="small" />;
            case 'Shopping': return <ShoppingBagIcon fontSize="small" />;
            case 'Sleep': return <HotelIcon fontSize="small" />;
            case 'TypicalProducts': return <LocalOfferIcon fontSize="small" />;
            default: return <PlaceIcon fontSize="small" />;
        }
    };

    const getContentLabel = (type) => {
        switch (type) {
            case 'article': return t('contentTypes.article');
            case 'event': return t('contentTypes.event');
            case 'route': return t('contentTypes.route');
            case 'organization': return t('contentTypes.organization');
            case 'service': return t('contentTypes.service');
            case 'ArtCulture': return t('poiCategories.artCulture');
            case 'EatAndDrink': return t('poiCategories.eatAndDrink');
            case 'EntertainmentLeisure': return t('poiCategories.entertainmentLeisure');
            case 'Nature': return t('poiCategories.nature');
            case 'Services': return t('poiCategories.services');
            case 'Shopping': return t('poiCategories.shopping');
            case 'Sleep': return t('poiCategories.sleep');
            case 'TypicalProducts': return t('poiCategories.typicalProducts');
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
                        {renderDistanceOrRegion(item)}
                        {item.subtitle && <Typography variant="caption" sx={{ display: 'block', mb: 0.5, color: 'text.secondary' }}>{item.subtitle}</Typography>}
                        {item.region && <Box sx={{ display: 'flex', alignItems: 'center', gap: 0.5, mt: 0.5 }}><LocationOnIcon sx={{ fontSize: 14, color: 'var(--teal)' }} /><Typography variant="caption" sx={{ color: 'text.secondary' }}>{item.region}</Typography></Box>}
                    </>
                );
            case 'event':
                return (
                    <>
                        {getEventStatusLabel(item) && (
                            <Box sx={{ display: 'inline-flex', alignItems: 'center', mb: 0.5, px: 1, py: 0.25, borderRadius: 999, backgroundColor: 'rgba(0, 150, 136, 0.12)' }}>
                                <Typography variant="caption" sx={{ color: 'var(--teal)', fontWeight: 600 }}>
                                    {getEventStatusLabel(item)}
                                </Typography>
                            </Box>
                        )}
                        {renderDistanceOrRegion(item)}
                        {item.startDate && <Box sx={{ display: 'flex', alignItems: 'center', gap: 0.5, mt: 0.5 }}><CalendarTodayIcon sx={{ fontSize: 14, color: 'var(--teal)' }} /><Typography variant="caption" sx={{ color: 'text.secondary' }}>{formatDate(item.startDate)}</Typography></Box>}
                        {item.address && <Box sx={{ display: 'flex', alignItems: 'center', gap: 0.5, mt: 0.5 }}><LocationOnIcon sx={{ fontSize: 14, color: 'var(--teal)' }} /><Typography variant="caption" sx={{ color: 'text.secondary', overflow: 'hidden', textOverflow: 'ellipsis', whiteSpace: 'nowrap' }}>{item.address}</Typography></Box>}
                    </>
                );
            case 'route':
                return (
                    <>
                        {renderDistanceOrRegion(item)}
                        {item.duration && <Box sx={{ display: 'flex', alignItems: 'center', gap: 0.5, mt: 0.5 }}><AccessTimeIcon sx={{ fontSize: 14, color: 'var(--teal)' }} /><Typography variant="caption" sx={{ color: 'text.secondary' }}>{item.duration}</Typography></Box>}
                        {item.routeLength && <Box sx={{ display: 'flex', alignItems: 'center', gap: 0.5, mt: 0.5 }}><RouteIcon sx={{ fontSize: 14, color: 'var(--teal)' }} /><Typography variant="caption" sx={{ color: 'text.secondary' }}>{item.routeLength}</Typography></Box>}
                    </>
                );
            case 'organization':
                return (
                    <>
                        {renderDistanceOrRegion(item)}
                        {item.type && <Typography variant="caption" sx={{ display: 'block', mb: 0.5, color: 'text.secondary' }}>{item.type}</Typography>}
                        {item.address && <Box sx={{ display: 'flex', alignItems: 'center', gap: 0.5, mt: 0.5 }}><LocationOnIcon sx={{ fontSize: 14, color: 'var(--teal)' }} /><Typography variant="caption" sx={{ color: 'text.secondary', overflow: 'hidden', textOverflow: 'ellipsis', whiteSpace: 'nowrap' }}>{item.address}</Typography></Box>}
                    </>
                );
            default:
                return (
                    <>
                        {renderDistanceOrRegion(item)}
                        {item.typology && <Typography variant="caption" sx={{ display: 'block', mb: 0.5, color: 'text.secondary' }}>{item.typology}</Typography>}
                        {item.address && <Box sx={{ display: 'flex', alignItems: 'center', gap: 0.5, mt: 0.5 }}><LocationOnIcon sx={{ fontSize: 14, color: 'var(--teal)' }} /><Typography variant="caption" sx={{ color: 'text.secondary', overflow: 'hidden', textOverflow: 'ellipsis', whiteSpace: 'nowrap' }}>{item.address}</Typography></Box>}
                    </>
                );
        }
    };

    const clearFilters = () => {
        setSelectedTypes([]);
        setSelectedCities([]);
        setTabValue('all');
        setSortNearMe(false);
    };

    /** Rendering */
    return (
        <Box className="page">
            <ChatbotOverlay
                open={chatOpen}
                onClose={() => setChatOpen(false)}
                location={userLocation}
                municipality={selectedCities.length === 1 ? selectedCities[0] : null}
                language={language}
                userName={username}
            />
            <Box className="chatbot-launcher">
                <IconButton color="primary" onClick={toggleChat} className="chatbot-launcher-btn">
                    <SupportAgentIcon />
                </IconButton>
            </Box>

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
                        <ListItemButton onClick={() => { setOpen(false); navigate('/login/admin/llm-health'); }}>
                            <ListItemIcon><AdminPanelSettingsIcon /></ListItemIcon>
                            <ListItemText primary={t('menu.adminLlmHealth')} />
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
                    <IconButton onClick={() => {
                        setOpen(true)
                        setChatOpen(false)
                    }}><MenuIcon /></IconButton>
                    <img src={logoPng} alt="SPM Logo" className="logo" />
                    <Typography variant="h6">{t('header.welcome')}, {username}</Typography>
                    <Box sx={{ marginLeft: 'auto' }}>
                        <IconButton onClick={handleLanguageToggle} title={`Switch to ${language === 'en' ? 'Italian' : 'English'}`}>
                            <LanguageIcon />
                            <Typography variant="caption" sx={{ ml: 0.5, fontWeight: 600 }}>{language.toUpperCase()}</Typography>
                        </IconButton>
                    </Box>
                </Box>

                <Box className="search-bar-wrapper">
                    <TextField
                        fullWidth
                        size="small"
                        placeholder={t('home.searchPlaceholder') ?? "Search activities..."}
                        value={searchQuery}
                        onChange={(e) => setSearchQuery(e.target.value)}
                        InputProps={{
                            startAdornment: (
                                <InputAdornment position="start">
                                    <SearchIcon />
                                </InputAdornment>
                            ),
                            endAdornment: searchQuery.length > 0 && (
                                <InputAdornment position="end">
                                    <IconButton
                                        size="small"
                                        onClick={() => setSearchQuery('')}
                                    >
                                        <CloseIcon />
                                    </IconButton>
                                </InputAdornment>
                            )
                        }}
                    />
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
                            <ToggleButton value="organization" aria-label="organizations" className="filter-toggle"><BusinessIcon fontSize="small" /><span>{t('contentTypes.organizations')}</span></ToggleButton>
                            <ToggleButton value="service" aria-label="services" className="filter-toggle"><BuildIcon fontSize="small" /><span>{t('contentTypes.services')}</span></ToggleButton>
                            <ToggleButton value="ArtCulture" aria-label="art-culture" className="filter-toggle"><MuseumIcon fontSize="small" /><span>{t('poiCategories.artCulture')}</span></ToggleButton>
                            <ToggleButton value="EatAndDrink" aria-label="eat-drink" className="filter-toggle"><RestaurantIcon fontSize="small" /><span>{t('poiCategories.eatAndDrink')}</span></ToggleButton>
                            <ToggleButton value="EntertainmentLeisure" aria-label="entertainment" className="filter-toggle"><TheaterComedyIcon fontSize="small" /><span>{t('poiCategories.entertainmentLeisure')}</span></ToggleButton>
                            <ToggleButton value="Nature" aria-label="nature" className="filter-toggle"><ParkIcon fontSize="small" /><span>{t('poiCategories.nature')}</span></ToggleButton>
                            <ToggleButton value="Shopping" aria-label="shopping" className="filter-toggle"><ShoppingBagIcon fontSize="small" /><span>{t('poiCategories.shopping')}</span></ToggleButton>
                            <ToggleButton value="Sleep" aria-label="sleep" className="filter-toggle"><HotelIcon fontSize="small" /><span>{t('poiCategories.sleep')}</span></ToggleButton>
                            <ToggleButton value="TypicalProducts" aria-label="typical-products" className="filter-toggle"><LocalOfferIcon fontSize="small" /><span>{t('poiCategories.typicalProducts')}</span></ToggleButton>
                        </ToggleButtonGroup>
                    </Box>

                    <Box>
                        <Typography variant="caption" sx={{ fontWeight: 600, mb: 0.5, display: 'block' }}>{t('filters.sorting')}</Typography>
                        <ToggleButton
                            value="near-me"
                            selected={sortNearMe}
                            onChange={toggleNearMe}
                            aria-label="near me"
                            className="filter-toggle"
                            disabled={!userLocation}
                        >
                            <LocationOnIcon fontSize="small" />
                            <span>{t('filters.nearMe')}</span>
                        </ToggleButton>
                        {!userLocation && locationDenied && (
                            <Typography variant="caption" color="text.secondary" sx={{ display: 'block', mt: 0.5 }}>
                                {t('filters.locationDenied')}
                            </Typography>
                        )}
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
                </Box>

                {/* Tabs */}
                <Box className="tabs-toolbar">
                    <Tabs
                        value={tabValue}
                        onChange={handleTabChange}
                        variant="fullWidth"
                        textColor="primary"
                        indicatorColor="primary"
                    >
                        <Tab value="all" label={t('home.AllActivities')} />
                        <Tab value="recommended" label={t('home.recommendedForYou')} />
                        <Tab value="mightLike" label={t('home.mightLike')} />
                    </Tabs>
                </Box>

                {/* Feed */}
                <Box className="scrollable-content">
                    {loading && <CircularProgress />}
                    {error && <Alert severity="error">{error}</Alert>}

                    {!loading && !error && (
                        <>
                            <Typography variant="h6" className="recommended-title">
                                {tabValue === 'recommended' ? t('home.recommendedForYou')
                                    : tabValue === 'mightLike' ? t('home.mightLike')
                                        : t('home.AllActivities')}
                            </Typography>

                            <Box className="content-grid">
                                {paginatedContent.map(item => (
                                    <Card key={item.id ?? item.Id ?? item.identifier ?? item.taxCode} className="content-card" onClick={() => handleCardClick(item)}>
                                        <Box className="card-type-badge">
                                            {getContentIcon(item.contentType)}
                                            <Typography variant="caption">{getContentLabel(item.contentType)}</Typography>
                                        </Box>
                                        <CardMedia component="img" image={buildMediaUrl(item.imagePath ?? item.ImagePath ?? item.primaryImage ?? item.PrimaryImage ?? item.primaryImagePath ?? item.PrimaryImagePath)} alt={item.title ?? item.name ?? item.legalName} className="card-image" />
                                        <CardContent className="card-content">
                                            <Typography variant="body2" className="card-title">{item.title ?? item.name ?? item.legalName}</Typography>
                                            {getCardDetails(item)}
                                        </CardContent>
                                    </Card>
                                ))}
                            </Box>

                            {sortedContent.length === 0 && <Alert severity="info">{t('messages.noActivities')}</Alert>}
                        </>
                    )}

                    {/* Pagination */}
                    <Box className="custom-pagination">
                        <Pagination
                            count={Math.ceil(sortedContent.length / ITEMS_PER_PAGE)}
                            page={page}
                            onChange={(_, value) => setPage(value)}
                            color="primary"
                            shape="rounded"
                            size="medium"
                        />
                    </Box>
                </Box>
            </Box>
        </Box>
    );
}
