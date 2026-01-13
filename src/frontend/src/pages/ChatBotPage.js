import { useState } from 'react';
import { Box, Typography, IconButton, TextField, Button } from '@mui/material';
import { useTranslation } from '../hooks/useTranslation.js';
import CloseIcon from '@mui/icons-material/Close';
import './styles/ChatBotPage.css';

export default function ChatBotPage({ open, onClose }) {
    const [messages, setMessages] = useState([
        { from: 'bot', text: 'Ciao! Come posso aiutarti oggi?' }
    ]);
    const [input, setInput] = useState('');
    const { t } = useTranslation();

    if (!open) return null;

    const sendMessage = () => {
        if (!input.trim()) return;
        setMessages([...messages, { from: 'user', text: input }]);
        setInput('');
        setTimeout(() => {
            setMessages(prev => [...prev, { from: 'bot', text: 'Mi dispiace, non posso ancora rispondere a domande complesse 😅' }]);
        }, 800);
    };

    return (
        <Box className="chatbot-container slide-up">
            {/* Header */}
            <Box className="chatbot-header">
                <Box className="chatbot-header-top">
                    <Typography variant="subtitle1">{t('chatBot.AIAssistant')}</Typography>
                    <IconButton size="small" onClick={onClose}>
                        <CloseIcon sx={{ color: 'white' }} />
                    </IconButton>
                </Box>
                <Typography className="chatbot-subtitle">
                    {t('chatBot.chatWithBot')}
                </Typography>
            </Box>

            {/* Messages */}
            <Box className="chatbot-messages">
                {messages.map((m, i) => (
                    <Box
                        key={i}
                        className={`chatbot-message ${m.from === 'user' ? 'user' : 'bot'}`}
                    >
                        {m.text}
                    </Box>
                ))}
            </Box>

            {/* Input */}
            <Box className="chatbot-input">
                <TextField
                    value={input}
                    onChange={(e) => setInput(e.target.value)}
                    placeholder={t('chatBot.inputPlaceholder')}
                    size="small"
                    fullWidth
                    onKeyDown={(e) => e.key === 'Enter' && sendMessage()}
                />
                <Button
                    onClick={sendMessage}
                    className="chatbot-send-btn"
                    variant="contained"
                >
                    {t('chatBot.sendButton')}
                </Button>

            </Box>
        </Box>
    );
}
