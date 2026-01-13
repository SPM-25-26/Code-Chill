import { useEffect, useRef, useState } from 'react';
import { Box, Typography, IconButton, TextField, Button } from '@mui/material';
import CloseIcon from '@mui/icons-material/Close';
import { useTranslation } from '../hooks/useTranslation.js';
import './styles/ChatBotPage.css';

const INITIAL_MESSAGE = {
    from: 'bot',
    text: 'Ciao! Come posso aiutarti oggi?'
};

export default function ChatBotPage({ open, onClose }) {
    const { t } = useTranslation();

    const [messages, setMessages] = useState([INITIAL_MESSAGE]);
    const [input, setInput] = useState('');
    const [isTyping, setIsTyping] = useState(false);

    const messagesEndRef = useRef(null);

    useEffect(() => {
        messagesEndRef.current?.scrollIntoView({ behavior: 'smooth' });
    }, [messages, isTyping]);

    if (!open) return null;

    const startNewChat = () => {
        setMessages([INITIAL_MESSAGE]);
        setInput('');
        setIsTyping(false);
    };

    const sendMessage = () => {
        if (!input.trim() || isTyping) return;

        const userMessage = { from: 'user', text: input };

        setMessages(prev => [...prev, userMessage]);
        setInput('');
        setIsTyping(true);

        setTimeout(() => {
            setIsTyping(false);
            setMessages(prev => [
                ...prev,
                {
                    from: 'bot',
                    text: 'Sto imparando! Presto saprò rispondere meglio 😊'
                }
            ]);
        }, 1200);
    };

    return (
        <Box className="chatbot-container slide-up">
            {/* HEADER */}
            <Box className="chatbot-header">
                <Box className="chatbot-header-top">
                    <Typography className="chatbot-title">
                        {t('chatBot.AIAssistant')}
                    </Typography>

                    <Box>
                        <IconButton size="small" onClick={onClose}>
                            <CloseIcon sx={{ color: 'white' }} />
                        </IconButton>
                    </Box>
                </Box>

                <Typography className="chatbot-subtitle">
                    {t('chatBot.chatWithBot')}
                </Typography>
            </Box>

            {/* MESSAGES */}
            <Box className="chatbot-messages">

                {/* System action */}
                <Box className="chatbot-system-wrapper">
                    <Box
                        className="chatbot-system-action"
                        onClick={startNewChat}
                    >
                        {t('chatBot.newChat')}
                    </Box>
                </Box>
                {messages.map((m, i) => (
                    <Box
                        key={i}
                        className={`chatbot-message ${m.from}`}
                    >
                        {m.text}
                    </Box>
                ))}

                {/* Typing indicator */}
                {isTyping && (
                    <Box className="chatbot-message bot typing">
                        <span className="dot" />
                        <span className="dot" />
                        <span className="dot" />
                    </Box>
                )}

                <div ref={messagesEndRef} />
            </Box>

            {/* INPUT */}
            <Box className="chatbot-input">
                <TextField
                    value={input}
                    onChange={(e) => setInput(e.target.value)}
                    placeholder={t('chatBot.inputPlaceholder')}
                    fullWidth
                    size="small"
                    disabled={isTyping}
                    onKeyDown={(e) => e.key === 'Enter' && sendMessage()}
                />

                <Button
                    onClick={sendMessage}
                    disabled={isTyping}
                    className="chatbot-send-btn"
                    variant="contained"
                >
                    {t('chatBot.sendButton')}
                </Button>
            </Box>
        </Box>
    );
}
