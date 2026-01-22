import { useEffect, useState } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import {
    Box,
    Typography,
    Alert,
    Chip,
    IconButton,
    Skeleton,
    MobileStepper,
    Button,
    Divider,
    Paper
} from '@mui/material';
import ArrowBackIcon from '@mui/icons-material/ArrowBack';
import ShareIcon from '@mui/icons-material/Share';
import KeyboardArrowLeft from '@mui/icons-material/KeyboardArrowLeft';
import KeyboardArrowRight from '@mui/icons-material/KeyboardArrowRight';
import LocationOnIcon from '@mui/icons-material/LocationOn';
import CalendarTodayIcon from '@mui/icons-material/CalendarToday';
import AccessTimeIcon from '@mui/icons-material/AccessTime';
import RouteIcon from '@mui/icons-material/Route';
import PhoneIcon from '@mui/icons-material/Phone';
import EmailIcon from '@mui/icons-material/Email';
import LanguageIcon from '@mui/icons-material/Language';
import FacebookIcon from '@mui/icons-material/Facebook';
import InstagramIcon from '@mui/icons-material/Instagram';
import HikingIcon from '@mui/icons-material/Hiking';
import TerrainIcon from '@mui/icons-material/Terrain';
import GroupIcon from '@mui/icons-material/Group';
import CategoryIcon from '@mui/icons-material/Category';
import instance from '../utilities/Utilities.js';
import { useTranslation } from '../hooks/useTranslation.js';
import './styles/ContentDetailPage.css';

const MEDIA_BASE = 'https://eppoi.io';

function buildMediaUrl(path) {
    if (!path) return '';
    if (path.startsWith('http')) return path;
    return `${MEDIA_BASE}${path}`;
}

export default function ContentDetailPage() {
    const { id } = useParams();
    const navigate = useNavigate();
    const { t, language } = useTranslation();

    const [content, setContent] = useState(null);
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState('');
    const [activeStep, setActiveStep] = useState(0);
    const [isPaused, setIsPaused] = useState(false);

    // Combine main image with gallery for carousel
    const images = content ? [
        content.image,
        ...(content.gallery || [])
    ].filter(img => img) : [];

    const maxSteps = images.length;

    useEffect(() => {
        const fetchContent = async () => {
            try {
                const res = await instance.get(`/api/pois/${id}`);
                setContent(res.data);
            } catch (err) {
                console.error(err);
                setError('Failed to load content details.');
            } finally {
                setLoading(false);
            }
        };
        fetchContent();
    }, [id]);

    // Auto-play carousel
    useEffect(() => {
        if (maxSteps <= 1 || isPaused) return;

        const interval = setInterval(() => {
            setActiveStep((prevStep) =>
                prevStep === maxSteps - 1 ? 0 : prevStep + 1
            );
        }, 3000); // Change slide every 3 seconds

        return () => clearInterval(interval);
    }, [maxSteps, isPaused]);

    const handleNext = () => {
        setIsPaused(true);
        setActiveStep((prevActiveStep) => prevActiveStep + 1);
    };

    const handleBack = () => {
        setIsPaused(true);
        setActiveStep((prevActiveStep) => prevActiveStep - 1);
    };

    const formatDate = (dateString) => {
        if (!dateString) return '';
        const date = new Date(dateString);
        return date.toLocaleDateString(language === 'en' ? 'en-US' : 'it-IT', {
            day: 'numeric',
            month: 'long',
            year: 'numeric'
        });
    };

    const TopBar = (
        <Box className="cdp-topbar" mb={2}>
            <IconButton onClick={() => navigate(-1)} sx={{ bgcolor: 'background.paper', boxShadow: 2 }}>
                <ArrowBackIcon />
            </IconButton>
            {!loading && content && (
                <IconButton
                    onClick={() => {
                        if (navigator.share) {
                            navigator.share({ title: content.title, url: window.location.href })
                                .catch(err => { if (err.name !== 'AbortError') console.error(err); });
                        } else {
                            alert('Share not supported');
                        }
                    }}
                    sx={{ bgcolor: 'background.paper', boxShadow: 2 }}
                >
                    <ShareIcon />
                </IconButton>
            )}
        </Box>
    );

    function buildGoogleMapsUrl(address) {
        if (!address) return '#';
        return `https://www.google.com/maps/search/?api=1&query=${encodeURIComponent(address)}`;
    }

    return (
        <Box className="cdp-page">
            <Box className="cdp-card">
                {TopBar}

                {/* Loading Skeleton */}
                {loading && (
                    <Box>
                        <Skeleton variant="rectangular" height={300} sx={{ borderRadius: 2 }} />
                        <Skeleton variant="text" height={60} sx={{ mt: 2 }} />
                        <Skeleton variant="text" width="60%" />
                        <Skeleton variant="rectangular" height={150} sx={{ mt: 2, borderRadius: 2 }} />
                    </Box>
                )}

                {/* Error */}
                {!loading && error && (
                    <Alert severity="error">{error}</Alert>
                )}

                {/* Content not found */}
                {!loading && !error && !content && (
                    <Alert severity="warning">Content not found.</Alert>
                )}

                {/* Main Content */}
                {!loading && content && (
                    <>
                        {/* Image Carousel */}
                        {images.length > 0 && (
                            <Box
                                sx={{ position: 'relative', borderRadius: 2, overflow: 'hidden', mb: 3 }}
                                onMouseEnter={() => setIsPaused(true)}
                                onMouseLeave={() => setIsPaused(false)}
                            >
                                <Box
                                    component="img"
                                    sx={{
                                        width: '100%',
                                        height: 400,
                                        objectFit: 'cover',
                                        display: 'block',
                                    }}
                                    src={buildMediaUrl(images[activeStep])}
                                    alt={content.title}
                                />
                                {maxSteps > 1 && (
                                    <MobileStepper
                                        steps={maxSteps}
                                        position="static"
                                        activeStep={activeStep}
                                        sx={{ bgcolor: 'transparent', position: 'absolute', bottom: 0, width: '100%', background: 'linear-gradient(to top, rgba(0,0,0,0.7), transparent)' }}
                                        nextButton={
                                            <Button size="small" onClick={handleNext} disabled={activeStep === maxSteps - 1} sx={{ color: 'white' }}>
                                                <KeyboardArrowRight />
                                            </Button>
                                        }
                                        backButton={
                                            <Button size="small" onClick={handleBack} disabled={activeStep === 0} sx={{ color: 'white' }}>
                                                <KeyboardArrowLeft />
                                            </Button>
                                        }
                                    />
                                )}
                            </Box>
                        )}

                        {/* Header Section */}
                        <Box mb={3}>
                            {/* Type Badge */}
                            {content.type && (
                                <Chip
                                    label={content.type.toUpperCase()}
                                    size="small"
                                    sx={{
                                        mb: 1,
                                        background: 'linear-gradient(135deg, var(--teal) 0%, var(--teal-dark) 100%)',
                                        color: 'white',
                                        fontWeight: 600
                                    }}
                                />
                            )}

                            {/* Title */}
                            <Typography variant="h4" fontWeight={700} mb={1} color="var(--dark)">
                                {content.title}
                            </Typography>

                            {/* Subtitle */}
                            {content.subtitle && (
                                <Typography variant="h6" color="text.secondary" mb={2}>
                                    {content.subtitle}
                                </Typography>
                            )}

                            {/* Quick Info Chips */}
                            <Box display="flex" gap={1} flexWrap="wrap" mb={2}>
                                {content.region && (
                                    <Chip icon={<LocationOnIcon />} label={content.region} size="small" variant="outlined" />
                                )}
                                {content.timeToRead && (
                                    <Chip icon={<AccessTimeIcon />} label={content.timeToRead} size="small" variant="outlined" />
                                )}
                                {content.typology && (
                                    <Chip icon={<CategoryIcon />} label={content.typology} size="small" variant="outlined" />
                                )}
                                {content.audience && (
                                    <Chip icon={<GroupIcon />} label={content.audience} size="small" variant="outlined" />
                                )}
                                {content.duration && (
                                    <Chip icon={<AccessTimeIcon />} label={content.duration} size="small" variant="outlined" />
                                )}
                                {content.routeLength && (
                                    <Chip icon={<RouteIcon />} label={content.routeLength} size="small" variant="outlined" />
                                )}
                            </Box>
                        </Box>

                        {/* Description */}
                        {content.description && (
                            <Paper elevation={0} sx={{ p: 2, mb: 3, bgcolor: 'rgba(46, 196, 182, 0.05)', borderLeft: '4px solid var(--teal)' }}>
                                <Typography variant="body1" lineHeight={1.8} sx={{ whiteSpace: 'pre-line' }}>
                                    {content.description}
                                </Typography>
                            </Paper>
                        )}

                        {/* Article Paragraphs */}
                        {content.paragraphs && content.paragraphs.length > 0 && (
                            <Box mb={3}>
                                {content.paragraphs.map((para, idx) => (
                                    <Box key={idx} mb={3}>
                                        {para.title && (
                                            <Typography variant="h5" fontWeight={600} mb={1} color="var(--dark)">
                                                {para.title}
                                            </Typography>
                                        )}
                                        {para.subtitle && (
                                            <Typography variant="subtitle1" color="text.secondary" mb={1}>
                                                {para.subtitle}
                                            </Typography>
                                        )}
                                        {para.script && (
                                            <Typography variant="body1" lineHeight={1.8} sx={{ whiteSpace: 'pre-line' }}>
                                                {para.script}
                                            </Typography>
                                        )}
                                        {idx < content.paragraphs.length - 1 && <Divider sx={{ mt: 2 }} />}
                                    </Box>
                                ))}
                            </Box>
                        )}

                        {/* Themes */}
                        {content.themes && content.themes.length > 0 && (
                            <Box mb={3}>
                                <Typography variant="h6" fontWeight={600} mb={1}>
                                    {t('home.themes')}
                                </Typography>
                                <Box display="flex" gap={1} flexWrap="wrap">
                                    {content.themes.map((theme, idx) => (
                                        <Chip key={idx} label={theme} size="small" sx={{ bgcolor: 'var(--teal)', color: 'white' }} />
                                    ))}
                                </Box>
                            </Box>
                        )}

                        {/* Route Details */}
                        {(content.pathTheme || content.travellingMethod || content.numberOfStages) && (
                            <Paper elevation={1} sx={{ p: 2, mb: 3 }}>
                                <Typography variant="h6" fontWeight={600} mb={2}>
                                    {t('home.routeDetails')}
                                </Typography>
                                <Box display="flex" flexDirection="column" gap={1}>
                                    {content.pathTheme && (
                                        <Box display="flex" alignItems="center" gap={1}>
                                            <TerrainIcon sx={{ color: 'var(--teal)' }} />
                                            <Typography><strong>
                                                {t('home.themes')}:</strong> {content.pathTheme}</Typography>
                                        </Box>
                                    )}
                                    {content.travellingMethod && (
                                        <Box display="flex" alignItems="center" gap={1}>
                                            <HikingIcon sx={{ color: 'var(--teal)' }} />
                                            <Typography><strong>
                                                {t('home.methodsOfTravel')}:</strong> {content.travellingMethod}</Typography>
                                        </Box>
                                    )}
                                    {content.numberOfStages !== 0 && (
                                        <Box display="flex" alignItems="center" gap={1}>
                                            <RouteIcon sx={{ color: 'var(--teal)' }} />
                                            <Typography><strong>
                                                {t('home.stages')}:</strong> {content.numberOfStages}</Typography>
                                        </Box>
                                    )}
                                </Box>
                            </Paper>
                        )}

                        {/* Best When (Route) */}
                        {content.bestWhen && content.bestWhen.length > 0 && (
                            <Box mb={3}>
                                <Typography variant="h6" fontWeight={600} mb={1}>
                                    {t('home.bestTimeToVisit')}
                                </Typography>
                                <Box display="flex" gap={1} flexWrap="wrap">
                                    {content.bestWhen.map((season, idx) => (
                                        <Chip key={idx} label={season} size="small" color="primary" />
                                    ))}
                                </Box>
                            </Box>
                        )}

                        {/* Event Details */}
                        {(content.startDate || content.endDate) && (
                            <Paper elevation={1} sx={{ p: 2, mb: 3 }}>
                                <Typography variant="h6" fontWeight={600} mb={2}>
                                    {t('home.eventInformation')}
                                </Typography>
                                <Box display="flex" flexDirection="column" gap={1}>
                                    {content.startDate && (
                                        <Box display="flex" alignItems="center" gap={1}>
                                            <CalendarTodayIcon sx={{ color: 'var(--teal)' }} />
                                            <Typography><strong>
                                                {t('home.startDate')}:</strong> {formatDate(content.startDate)}</Typography>
                                        </Box>
                                    )}
                                    {content.endDate && (
                                        <Box display="flex" alignItems="center" gap={1}>
                                            <CalendarTodayIcon sx={{ color: 'var(--teal)' }} />
                                            <Typography><strong>
                                                {t('home.endDate')}:</strong> {formatDate(content.endDate)}</Typography>
                                        </Box>
                                    )}
                                </Box>
                            </Paper>
                        )}

                        {/* Location (Restaurants / POIs) */}
                        {content.address && (
                            <Paper elevation={1} sx={{ p: 2, mb: 3 }}>
                                <Typography variant="h6" fontWeight={600} mb={2}>
                                    {t('home.Location')}
                                </Typography>

                                <Box display="flex" alignItems="center" gap={1}>
                                    <LocationOnIcon sx={{ color: 'var(--teal)' }} />

                                    <Typography>
                                        {content.address}
                                    </Typography>
                                </Box>

                                <Button
                                    variant="outlined"
                                    startIcon={<RouteIcon />}
                                    fullWidth
                                    href={buildGoogleMapsUrl(content.address)}
                                    target="_blank"
                                    rel="noopener noreferrer"
                                    sx={{
                                        borderColor: 'var(--teal)',
                                        color: 'var(--teal)',
                                        fontWeight: 600,
                                        textTransform: 'none',
                                        marginTop: 2,
                                        '&:hover': {
                                            bgcolor: 'rgba(46, 196, 182, 0.08)',
                                            borderColor: 'var(--teal-dark)'
                                        }
                                    }}
                                >
                                    {t('home.OpenGoogleMaps')}
                                </Button>
                            </Paper>
                        )}


                        {/* Contact Information */}
                        {(content.website || content.phone || content.email || content.facebook || content.instagram) && (
                            <Paper elevation={1} sx={{ p: 2, mb: 3 }}>
                                <Typography variant="h6" fontWeight={600} mb={2}>
                                    {t('home.contant&social')}
                                </Typography>
                                <Box display="flex" flexDirection="column" gap={1.5}>
                                    {content.website && (
                                        <Box display="flex" alignItems="center" gap={1}>
                                            <LanguageIcon sx={{ color: 'var(--teal)' }} />
                                            <a href={content.website} target="_blank" rel="noopener noreferrer" style={{ color: 'var(--teal)', textDecoration: 'none' }}>
                                                {content.website}
                                            </a>
                                        </Box>
                                    )}
                                    {content.phone && (
                                        <Box display="flex" alignItems="center" gap={1}>
                                            <PhoneIcon sx={{ color: 'var(--teal)' }} />
                                            <a href={`tel:${content.phone}`} style={{ color: 'var(--teal)', textDecoration: 'none' }}>
                                                {content.phone}
                                            </a>
                                        </Box>
                                    )}
                                    {content.email && (
                                        <Box display="flex" alignItems="center" gap={1}>
                                            <EmailIcon sx={{ color: 'var(--teal)' }} />
                                            <a href={`mailto:${content.email}`} style={{ color: 'var(--teal)', textDecoration: 'none' }}>
                                                {content.email}
                                            </a>
                                        </Box>
                                    )}
                                    {content.facebook && (
                                        <Box display="flex" alignItems="center" gap={1}>
                                            <FacebookIcon sx={{ color: 'var(--teal)' }} />
                                            <a href={content.facebook} target="_blank" rel="noopener noreferrer" style={{ color: 'var(--teal)', textDecoration: 'none' }}>
                                                Facebook
                                            </a>
                                        </Box>
                                    )}
                                    {content.instagram && (
                                        <Box display="flex" alignItems="center" gap={1}>
                                            <InstagramIcon sx={{ color: 'var(--teal)' }} />
                                            <a href={content.instagram} target="_blank" rel="noopener noreferrer" style={{ color: 'var(--teal)', textDecoration: 'none' }}>
                                                Instagram
                                            </a>
                                        </Box>
                                    )}
                                </Box>
                            </Paper>
                        )}

                        {/* Virtual Tours */}
                        {content.virtualTours && content.virtualTours.length > 0 && (
                            <Box mb={3}>
                                <Typography variant="h6" fontWeight={600} mb={2}>
                                    {t('home.virtualTour')}
                                </Typography>
                                {content.virtualTours.map((tour, idx) => (
                                    <Button
                                        key={idx}
                                        variant="outlined"
                                        fullWidth
                                        sx={{ mb: 1 }}
                                        href={tour}
                                        target="_blank"
                                    >
                                        {t('home.viewVirtualTour')} {idx + 1}
                                    </Button>
                                ))}
                            </Box>
                        )}

                        {/* Municipality Badge */}
                        {content.municipalityName && (
                            <Box mt={4} pt={3} borderTop="1px solid #e0e0e0">
                                <Typography variant="caption" color="text.secondary">
                                    Part of {content.municipalityName}
                                </Typography>
                            </Box>
                        )}
                    </>
                )}
            </Box>
        </Box>
    );
}
