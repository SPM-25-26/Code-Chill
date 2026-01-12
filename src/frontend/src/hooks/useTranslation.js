import { useState, useEffect } from 'react';
import en from '../locales/en.json';
import it from '../locales/it.json';

const translations = {
    en,
    it
};

export const useTranslation = () => {
    const [language, setLanguageState] = useState(localStorage.getItem('language') || 'en');

    useEffect(() => {
        const handleStorageChange = () => {
            setLanguageState(localStorage.getItem('language') || 'it');
        };

        window.addEventListener('storage', handleStorageChange);

        // Custom event for same-tab language changes
        window.addEventListener('languageChange', handleStorageChange);

        return () => {
            window.removeEventListener('storage', handleStorageChange);
            window.removeEventListener('languageChange', handleStorageChange);
        };
    }, []);

    const t = (key) => {
        const keys = key.split('.');
        let value = translations[language];

        for (const k of keys) {
            if (value && typeof value === 'object') {
                value = value[k];
            } else {
                return key; // Return key if translation not found
            }
        }

        return value || key;
    };

    const changeLanguage = (newLanguage) => {
        localStorage.setItem('language', newLanguage);
        setLanguageState(newLanguage);
        // Dispatch custom event for same-tab updates
        window.dispatchEvent(new Event('languageChange'));
    };

    return { t, language, changeLanguage };
};
