import { useEffect, useMemo, useRef, useState } from 'react';
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
import DeleteOutlineIcon from '@mui/icons-material/DeleteOutline';
import SendIcon from '@mui/icons-material/Send';
import instance from '../utilities/Utilities.js';
import { useTranslation } from '../hooks/useTranslation.js';
import './styles/ChatbotOverlay.css';

function ChatbotOverlay({ open, onClose, location, municipality, language }) {
  const { t } = useTranslation();
  const [messages, setMessages] = useState(() => {
    try {
      const raw = localStorage.getItem('chatbotMessages');
      const parsed = raw ? JSON.parse(raw) : [];
      return Array.isArray(parsed) ? parsed : [];
    } catch {
      return [];
    }
  });
  const [input, setInput] = useState('');
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState('');
  const messagesEndRef = useRef(null);

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

  const handleReset = () => {
    if (loading) return;
    setMessages([]);
    setError('');
    setInput('');
    localStorage.removeItem('chatbotMessages');
  };

  useEffect(() => {
    try {
      localStorage.setItem('chatbotMessages', JSON.stringify(messages));
    } catch {
      // ignore persistence errors
    }
  }, [messages]);

  useEffect(() => {
    if (!messagesEndRef.current) return;
    messagesEndRef.current.scrollIntoView({ behavior: 'smooth', block: 'end' });
  }, [messages, loading]);

  const renderInlineText = (text, keyPrefix) => {
    const parts = [];
    const regex = /(\*\*[^*]+\*\*|\*[^*]+\*)/g;
    let lastIndex = 0;
    let match;
    let idx = 0;
    while ((match = regex.exec(text)) !== null) {
      if (match.index > lastIndex) {
        parts.push(text.slice(lastIndex, match.index));
      }
      const token = match[0];
      if (token.startsWith('**')) {
        parts.push(
          <strong key={`${keyPrefix}-b-${idx++}`}>{token.slice(2, -2)}</strong>
        );
      } else {
        parts.push(
          <em key={`${keyPrefix}-i-${idx++}`}>{token.slice(1, -1)}</em>
        );
      }
      lastIndex = match.index + token.length;
    }
    if (lastIndex < text.length) {
      parts.push(text.slice(lastIndex));
    }
    return parts;
  };

  const renderInline = (text) => {
    const parts = [];
    const linkRegex = /\*\*\[([^\]]+)\]\(([^)]+)\)\*\*|\[([^\]]+)\]\(([^)]+)\)/g;
    let lastIndex = 0;
    let match;
    let idx = 0;
    while ((match = linkRegex.exec(text)) !== null) {
      if (match.index > lastIndex) {
        parts.push(...renderInlineText(text.slice(lastIndex, match.index), `t-${idx}`));
      }
      const isBoldLink = Boolean(match[1]);
      const label = isBoldLink ? match[1] : match[3];
      let href = isBoldLink ? match[2] : match[4];
      if (href.startsWith('http')) {
        try {
          const url = new URL(href);
          href = url.pathname;
        } catch {
          // keep as-is if parsing fails
        }
      }
      if (href.startsWith('login/')) {
        href = `/${href}`;
      } else if (href.startsWith('/content-detail/')) {
        href = `/login${href}`;
      } else if (href.startsWith('content-detail/')) {
        href = `/login/${href}`;
      }
      parts.push(
        <a
          key={`l-${idx++}`}
          href={href}
          className="chatbot-md-link"
          rel={href.startsWith('http') ? 'noreferrer' : undefined}
          target={href.startsWith('http') ? '_blank' : undefined}
        >
          {isBoldLink ? <strong>{renderInlineText(label, `l-${idx}`)}</strong> : renderInlineText(label, `l-${idx}`)}
        </a>
      );
      lastIndex = match.index + match[0].length;
    }
    if (lastIndex < text.length) {
      parts.push(...renderInlineText(text.slice(lastIndex), `t-${idx}`));
    }
    return parts;
  };

  const renderMarkdown = (text) => {
    if (!text) return null;
    const lines = text.split(/\r?\n/);
    const blocks = [];
    let listItems = [];
    let listType = null;
    let blockIndex = 0;

    const flushList = () => {
      if (listItems.length === 0) return;
      const items = listItems;
      listItems = [];
      const ListTag = listType === 'ul' ? 'ul' : 'ol';
      blocks.push(
        <ListTag
          className={listType === 'ul' ? 'chatbot-md-ul' : 'chatbot-md-ol'}
          key={`list-${blockIndex++}`}
        >
          {items.map((item, i) => (
            <li className="chatbot-md-li" key={`li-${i}`}>
              {renderInline(item)}
            </li>
          ))}
        </ListTag>
      );
      listType = null;
    };

    for (const line of lines) {
      const trimmed = line.trim();
      if (!trimmed) {
        continue;
      }
      const headingMatch = trimmed.match(/^###\s+(.*)$/);
      if (headingMatch) {
        flushList();
        blocks.push(
          <h4 className="chatbot-md-h4" key={`h4-${blockIndex++}`}>
            {renderInline(headingMatch[1])}
          </h4>
        );
        continue;
      }
      const olMatch = trimmed.match(/^\d+\.\s+(.*)$/);
      if (olMatch) {
        if (listType && listType !== 'ol') {
          flushList();
        }
        listType = 'ol';
        listItems.push(olMatch[1]);
        continue;
      }
      const ulMatch = trimmed.match(/^[-*]\s+(.*)$/);
      if (ulMatch) {
        if (listType && listType !== 'ul') {
          flushList();
        }
        listType = 'ul';
        listItems.push(ulMatch[1]);
        continue;
      }
      flushList();
      blocks.push(
        <p className="chatbot-md-p" key={`p-${blockIndex++}`}>
          {renderInline(trimmed)}
        </p>
      );
    }
    flushList();
    return blocks;
  };

  if (!open) return null;

  return (
    <div className="chatbot-overlay">
      <Paper elevation={4} className="chatbot-panel">
        <Box className="chatbot-header">
          <Typography variant="subtitle1">{t('chat.title')}</Typography>
          <Box className="chatbot-header-actions">
            <IconButton
              size="small"
              onClick={handleReset}
              aria-label={t('chat.newChat')}
              title={t('chat.newChat')}
              disabled={loading}
            >
              <DeleteOutlineIcon fontSize="small" />
            </IconButton>
            <IconButton size="small" onClick={onClose} aria-label="close chat">
              <CloseIcon fontSize="small" />
            </IconButton>
          </Box>
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
              <Box className="chatbot-message-text">{renderMarkdown(m.content)}</Box>
            </Box>
          ))}
          {loading && (
            <Box className="chatbot-loading">
              <CircularProgress size={16} />
              <Typography variant="caption">{t('chat.thinking')}</Typography>
            </Box>
          )}
          <div ref={messagesEndRef} />
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
