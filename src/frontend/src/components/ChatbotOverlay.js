import { useMemo, useState } from 'react';
import {
  Box,
  Button,
  CircularProgress,
  IconButton,
  Paper,
  TextField,
  Typography,
} from '@mui/material';
import CloseIcon from '@mui/icons-material/Close';
import SendIcon from '@mui/icons-material/Send';
import instance from '../utilities/Utilities.js';
import { useTranslation } from '../hooks/useTranslation.js';
import './styles/ChatbotOverlay.css';

function ChatbotOverlay({ open, onClose, location, municipality, language }) {
  const { t } = useTranslation();
  const [messages, setMessages] = useState([]);
  const [input, setInput] = useState('');
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState('');

  const conversationPayload = useMemo(
    () =>
      messages.map((m) => ({
        role: m.role,
        content: m.content,
      })),
    [messages]
  );

  const handleSend = async () => {
    const trimmed = input.trim();
    if (!trimmed || loading) return;

    setError('');
    setInput('');
    const nextMessages = [...messages, { role: 'user', content: trimmed }];
    setMessages(nextMessages);
    setLoading(true);

    try {
      const token = localStorage.getItem('token');
      const response = await instance.post(
        '/api/chat',
        {
          message: trimmed,
          conversation: conversationPayload,
          location: location
            ? { latitude: location.lat, longitude: location.lng }
            : null,
          municipality: municipality || null,
          language: language || 'it',
          maxContextItems: 40,
        },
        {
          headers: token ? { Authorization: `Bearer ${token}` } : undefined,
        }
      );

      const reply = response.data?.reply ?? '';
      setMessages((prev) => [...prev, { role: 'assistant', content: reply }]);
    } catch (e) {
      const msg =
        e?.response?.data?.message ||
        (typeof e?.response?.data === 'string' ? e.response.data : '') ||
        e?.message ||
        t('chat.error');
      setError(msg);
    } finally {
      setLoading(false);
    }
  };

  if (!open) return null;

  return (
    <div className="chatbot-overlay">
      <Paper elevation={4} className="chatbot-panel">
        <Box className="chatbot-header">
          <Typography variant="subtitle1">{t('chat.title')}</Typography>
          <IconButton size="small" onClick={onClose} aria-label="close chat">
            <CloseIcon fontSize="small" />
          </IconButton>
        </Box>

        <Box className="chatbot-messages">
          {messages.length === 0 && (
            <Typography variant="body2" color="text.secondary">
              {t('chat.empty')}
            </Typography>
          )}
          {messages.map((m, idx) => (
            <Box
              key={`${m.role}-${idx}`}
              className={`chatbot-message ${m.role === 'user' ? 'user' : 'assistant'}`}
            >
              <Typography variant="body2">{m.content}</Typography>
            </Box>
          ))}
          {loading && (
            <Box className="chatbot-loading">
              <CircularProgress size={16} />
              <Typography variant="caption">{t('chat.thinking')}</Typography>
            </Box>
          )}
        </Box>

        {error && (
          <Typography variant="caption" className="chatbot-error">
            {error}
          </Typography>
        )}

        <Box className="chatbot-input">
          <TextField
            value={input}
            onChange={(e) => setInput(e.target.value)}
            placeholder={t('chat.placeholder')}
            size="small"
            fullWidth
            onKeyDown={(e) => {
              if (e.key === 'Enter' && !e.shiftKey) {
                e.preventDefault();
                handleSend();
              }
            }}
          />
          <Button
            variant="contained"
            onClick={handleSend}
            disabled={loading || !input.trim()}
            className="chatbot-send"
          >
            <SendIcon fontSize="small" />
          </Button>
        </Box>
      </Paper>
    </div>
  );
}

export default ChatbotOverlay;
