import { useEffect, useState } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import {
    Box,
    Typography,
    Alert,
    Chip,
    Card,
    CardMedia,
    CardContent,
    IconButton,
    Skeleton
} from '@mui/material';
import ArrowBackIcon from '@mui/icons-material/ArrowBack';
import ShareIcon from '@mui/icons-material/Share';
import instance from '../utilities/Utilities.js';
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

    const [content, setContent] = useState(null);
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState('');

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

    const TopBar = (
        <Box className="cdp-topbar" mb={1}>
            <IconButton onClick={() => navigate(-1)}>
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
                >
                    <ShareIcon />
                </IconButton>
            )}
        </Box>
    );

    return (
        <Box className="cdp-page">
            <Box className="cdp-card">
                {TopBar}

                {/* Loading Skeleton */}
                {loading && (
                    <Box>
                        <Skeleton variant="rectangular" height={300} />
                        <Skeleton variant="text" height={60} />
                        <Skeleton variant="text" width="60%" />
                        <Skeleton variant="rectangular" height={150} />
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
                        <Card className="cdp-main-card">
                            {content.image && (
                                <CardMedia
                                    component="img"
                                    className="cdp-main-image"
                                    image={buildMediaUrl(content.image)}
                                    alt={content.title}
                                />
                            )}
                            <CardContent>
                                {/* Badges */}
                                <Box mb={1} display="flex" gap={1} flexWrap="wrap">
                                    {content.category && <Chip label={content.category} size="small" />}
                                    {content.subcategory && <Chip label={content.subcategory} size="small" />}
                                    {content.type && <Chip label={content.type.toUpperCase()} size="small" color="primary" />}
                                </Box>

                                {/* Title & Subtitle */}
                                <Typography variant="h4" fontWeight={600} mb={1}>
                                    {content.title}
                                </Typography>
                                {content.subtitle && (
                                    <Typography variant="subtitle1" color="text.secondary" mb={1}>
                                        {content.subtitle}
                                    </Typography>
                                )}

                                {/* Short Description */}
                                {content.shortDescription && (
                                    <Typography variant="body1" mb={2} lineHeight={1.6}>
                                        {content.shortDescription}
                                    </Typography>
                                )}

                                {/* Full Description */}
                                {content.description && (
                                    <Typography variant="body1" mb={2} lineHeight={1.6}>
                                        {content.description}
                                    </Typography>
                                )}

                                {/* Address */}
                                {content.address && (
                                    <Typography variant="body2" color="text.secondary">
                                        <strong>Address:</strong> {content.address}
                                    </Typography>
                                )}

                                {/* Price & Rating */}
                                {content.priceLevel && <Typography variant="body2">Price: {content.priceLevel}</Typography>}
                                {content.rating && (
                                    <Typography variant="body2">
                                        Rating: {content.rating} ({content.reviewCount || 0} reviews)
                                    </Typography>
                                )}

                                {/* Dates */}
                                {content.startDate && (
                                    <Typography variant="body2" color="text.secondary">
                                        <strong>Start Date:</strong> {new Date(content.startDate).toLocaleDateString()}
                                    </Typography>
                                )}
                                {content.endDate && (
                                    <Typography variant="body2" color="text.secondary">
                                        <strong>End Date:</strong> {new Date(content.endDate).toLocaleDateString()}
                                    </Typography>
                                )}

                                {/* Contact Info */}
                                {(content.website || content.phone || content.email) && (
                                    <Box mt={2}>
                                        {content.website && (
                                            <Typography variant="body2" color="primary">
                                                <strong>Website:</strong>{' '}
                                                <a href={content.website} target="_blank" rel="noopener noreferrer">{content.website}</a>
                                            </Typography>
                                        )}
                                        {content.phone && (
                                            <Typography variant="body2">
                                                <strong>Phone:</strong>{' '}
                                                <a href={`tel:${content.phone}`}>{content.phone}</a>
                                            </Typography>
                                        )}
                                        {content.email && (
                                            <Typography variant="body2">
                                                <strong>Email:</strong>{' '}
                                                <a href={`mailto:${content.email}`}>{content.email}</a>
                                            </Typography>
                                        )}
                                    </Box>
                                )}
                            </CardContent>
                        </Card>

                        {/* Gallery */}
                        {content.gallery?.length > 0 && (
                            <Box mt={2}>
                                <Typography variant="h6" mb={1}>
                                    Gallery
                                </Typography>
                                <Box className="cdp-gallery-scroll">
                                    {content.gallery.map((img, idx) => (
                                        <Card key={idx} className="cdp-gallery-card">
                                            <CardMedia
                                                component="img"
                                                height="130"
                                                image={buildMediaUrl(img)}
                                                alt={`Gallery image ${idx + 1}`}
                                            />
                                        </Card>
                                    ))}
                                </Box>
                            </Box>
                        )}
                    </>
                )}
            </Box>
        </Box>
    );
}
