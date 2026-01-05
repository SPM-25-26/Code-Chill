import { useNavigate } from 'react-router-dom';
import { Button, Typography, Card, CardContent } from '@mui/material';
import './styles/ContentDetailPage.css';

function ContentDetailPage() {
  const navigate = useNavigate();

  return (
    <div className="page-container d-flex justify-content-center align-items-center min-vh-100">
      <div className="page-card shadow-sm">
        <CardContent>
          <Typography variant="h4" sx={{ mb: 2 }}>
            Content Details
          </Typography>
          <Typography variant="body1">
            Details will be displayed here.
          </Typography>

          <div className="mt-3">
            <Button
              variant="outlined"
              sx={{ mt: 3 }}
              onClick={() => navigate(-1)}
            >
              Back
            </Button>
          </div>
        </CardContent>
      </div>
    </div>
  );
}

export default ContentDetailPage;
