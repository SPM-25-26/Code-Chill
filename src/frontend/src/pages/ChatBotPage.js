import { useEffect, useRef, useState } from 'react';
import { Box, Typography, IconButton, TextField, Button } from '@mui/material';
import CloseIcon from '@mui/icons-material/Close';
import WarningAmberIcon from '@mui/icons-material/WarningAmber';
import ReplayIcon from '@mui/icons-material/Replay';
import { useTranslation } from '../hooks/useTranslation.js';
import './styles/ChatBotPage.css';
import DeleteOutlineIcon from '@mui/icons-material/DeleteOutline';


export default function ChatBotPage({ open, onClose }) {
    const { t } = useTranslation();

    const INITIAL_MESSAGE = {
        from: 'bot',
        text: t('chatBot.welcomeMessage')
    };

    const [messages, setMessages] = useState([INITIAL_MESSAGE]);
    const [input, setInput] = useState('');
    const [isTyping, setIsTyping] = useState(false);
    const [isOnline, setIsOnline] = useState(navigator.onLine);
    const [hasError, setHasError] = useState(false);

    const messagesEndRef = useRef(null);
    const inputRef = useRef(null);

    /* Auto scroll */
    useEffect(() => {
        if (open) {
            messagesEndRef.current?.scrollIntoView({ behavior: 'smooth' });
        }
    }, [messages, isTyping, open]);

    /* Focus input */
    useEffect(() => {
        if (open && inputRef.current) {
            inputRef.current.focus();
        }
    }, [open, messages]);

    /* Online / Offline detection */
    useEffect(() => {
        const handleOnline = () => setIsOnline(true);
        const handleOffline = () => setIsOnline(false);

        window.addEventListener('online', handleOnline);
        window.addEventListener('offline', handleOffline);

        return () => {
            window.removeEventListener('online', handleOnline);
            window.removeEventListener('offline', handleOffline);
        };
    }, []);

    if (!open) return null;

    const startNewChat = () => {
        setMessages([INITIAL_MESSAGE]);
        setInput('');
        setIsTyping(false);
        setHasError(false);
    };

    /* Fake API */
    const fakeChatbotApi = (text) =>
        new Promise((resolve, reject) => {
            setTimeout(() => {
                Math.random() < 0.3
                    ? reject(new Error('API error'))
                    : resolve('Sto imparando! Presto saprò rispondere meglio 😊');
            }, 1200);
        });

    const sendMessage = async (text) => {
        if (!text.trim() || isTyping || !isOnline) return;

        const userMessage = {
            from: 'user',
            text,
            failed: false
        };

        setMessages(prev => [...prev, userMessage]);
        setInput('');
        setIsTyping(true);
        setHasError(false);

        try {
            const response = await fakeChatbotApi(text);
            setMessages(prev => [...prev, { from: 'bot', text: response }]);
        } catch {
            setMessages(prev => {
                const updated = [...prev];
                updated[updated.length - 1] = {
                    ...updated[updated.length - 1],
                    failed: true
                };
                return updated;
            });

            setMessages(prev => [
                ...prev,
                {
                    from: 'error',
                    text: t('chatBot.errorMessage') || 'Something went wrong. Please try again.'
                }
            ]);
        } finally {
            setIsTyping(false);
        }
    };

    const retryMessage = async (index) => {
        if (isTyping || !isOnline) return;

        const msg = messages[index];
        if (!msg) return;

        setMessages(prev => {
            const updated = [...prev];
            if (updated[updated.length - 1]?.from === 'error') {
                updated.pop();
            }
            updated[index] = { ...updated[index], failed: false };
            return updated;
        });

        setIsTyping(true);

        try {
            const response = await fakeChatbotApi(msg.text);
            setMessages(prev => [...prev, { from: 'bot', text: response }]);
        } catch {
            setMessages(prev => [
                ...prev,
                { ...prev[index], failed: true },
                {
                    from: 'error',
                    text: t('chatBot.errorMessage') || 'Something went wrong. Please try again.'
                }
            ]);
        } finally {
            setIsTyping(false);
        }
    };


    return (
        <Box className="chatbot-container slide-up">

            {/* HEADER */}
            <Box className="chatbot-header">
                <Box className="chatbot-header-top">
                    <Typography className="chatbot-title">
                        {t('chatBot.AIAssistant')}
                    </Typography>

                    <Box className="chatbot-header-actions">
                        <IconButton size="small" onClick={startNewChat} title={t('chatBot.newChat')}>
                            <DeleteOutlineIcon />
                        </IconButton>

                        <IconButton size="small" onClick={onClose}>
                            <CloseIcon />
                        </IconButton>
                    </Box>
                </Box>

                <Typography className="chatbot-subtitle">
                    {t('chatBot.chatWithBot')}
                </Typography>
            </Box>

            {/* MESSAGES */}
            <Box className="chatbot-messages">

                {messages.map((m, i) => (
                    <Box key={i} className={`chatbot-message-wrapper ${m.from}`}>

                        <Box className={`chatbot-message ${m.from} fade-in`}>
                            {m.text}
                        </Box>

                        {m.from === 'user' && m.failed && (
                            <Box
                                className="chatbot-retry"
                                onClick={() => retryMessage(i)}
                            >
                                <ReplayIcon fontSize="small" />
                            </Box>
                        )}
                    </Box>
                ))}


                {/* TYPING INDICATOR */}
                {isTyping && (
                    <Box className="chatbot-message bot typing fade-in">
                        <span className="dot" />
                        <span className="dot" />
                        <span className="dot" />
                    </Box>
                )}

                <div ref={messagesEndRef} />
            </Box>

            {/* OFFLINE WARNING */}
            {!isOnline && (
                <Box className="chatbot-offline-alert">
                    <WarningAmberIcon fontSize="small" />
                    <Typography variant="body2">
                        {t('chatBot.offlineMessage') || 'Chat requires an internet connection'}
                    </Typography>
                </Box>
            )}

            {/* INPUT */}
            <Box className="chatbot-input">
                <TextField
                    value={input}
                    onChange={(e) => setInput(e.target.value)}
                    placeholder={t('chatBot.inputPlaceholder')}
                    fullWidth
                    size="small"
                    inputRef={inputRef}
                    disabled={isTyping}
                    onKeyDown={(e) => e.key === 'Enter' && sendMessage(input)}
                />

                <Button
                    onClick={() => sendMessage(input)}
                    disabled={isTyping || !isOnline}
                    className="chatbot-send-btn"
                    variant="contained"
                >
                    {t('chatBot.sendButton')}
                </Button>
            </Box>
        </Box>
    );
}
