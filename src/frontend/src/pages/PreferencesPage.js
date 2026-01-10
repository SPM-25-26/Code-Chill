import { useEffect, useMemo, useState } from 'react';
import { useNavigate } from 'react-router-dom';
import TravelPreferenceWizard from '../components/preferences/TravelPreferenceWizard.js';
import instance from '../utilities/Utilities.js';
import './styles/PreferencesPage.css';

export default function PreferencesPage() {
  const [loading, setLoading] = useState(true);
  const [schema, setSchema] = useState(null);
  const [initialAnswers, setInitialAnswers] = useState({});
  const [error, setError] = useState('');
  const [saving, setSaving] = useState(false);
  const [successMessage, setSuccessMessage] = useState('');
  const token = localStorage.getItem('token');
  const navigate = useNavigate();

  const authHeaders = useMemo(
    () => ({
      Authorization: token ? `Bearer ${token}` : undefined,
    }),
    [token],
  );

  useEffect(() => {
    const load = async () => {
      try {
        setLoading(true);
        const [schemaRes, responseRes] = await Promise.all([
          instance.get('/api/preferences/schema', { headers: authHeaders }),
          instance.get('/api/preferences/response', { headers: authHeaders }).catch(() => null),
        ]);
        setSchema(schemaRes.data);
        if (responseRes?.data?.answers) {
          setInitialAnswers(responseRes.data.answers);
        }
      } catch (e) {
        setError('Failed to load questionnaire. Please try again.');
      } finally {
        setLoading(false);
      }
    };
    load();
  }, [authHeaders]);

  const handleSubmit = async (answers) => {
    setSaving(true);
    setError('');
    setSuccessMessage('');
    try {
      await instance.put(
        '/api/preferences/response?includePreview=true',
        { questionnaireVersion: schema?.version ?? 'v1', answers },
        { headers: authHeaders },
      );
      setSuccessMessage('Preferences saved.');
      setTimeout(() => navigate('/login/dashboard'), 1500);
    } catch (e) {
      setError('Failed to save preferences. Please try again.');
    } finally {
      setSaving(false);
    }
  };

  /**
   * Handles the "Skip for now" action in the preferences questionnaire.
   * 
   * - If the user already has saved preferences, skipping will NOT overwrite them.
   *   The existing answers are re-sent to the backend to preserve the profile.
   * - If the user has no existing preferences, an empty preferences object is saved,
   *   explicitly marking the questionnaire as skipped.
   */
  const handleSkip = async () => {
    setSaving(true);
    setError('');
    setSuccessMessage('');

    try {
      const hasExistingAnswers =
        initialAnswers && Object.keys(initialAnswers).length > 0;

      await instance.put(
        '/api/preferences/response?skip=true',
        {
          questionnaireVersion: schema?.version ?? 'v1',
          answers: hasExistingAnswers ? initialAnswers : {}
        },
        { headers: authHeaders },
      );

      // Clear autosaved data if no existing answers
      if (!hasExistingAnswers) {
        localStorage.removeItem('preference-wizard');
      }

      setSuccessMessage('Skipped for now.');
      setTimeout(() => navigate('/login/dashboard'), 1200);
    } catch (e) {
      setError('Failed to skip preferences. Please try again.');
    } finally {
      setSaving(false);
    }
  };

  if (loading) {
    return <div className="preferences-shell">Loading questionnaire...</div>;
  }

  if (error) {
    return <div className="preferences-shell error">{error}</div>;
  }

  return (
    <div className="preferences-shell">
      <h1 className="preferences-title">Your Travel Preferences</h1>
      <p className="preferences-subtitle">
        Tell us how you like to explore so we can personalize your recommendations.
      </p>
      {successMessage && <div className="preferences-toast success">{successMessage}</div>}
      <TravelPreferenceWizard
        questions={schema?.questions ?? []}
        initialAnswers={initialAnswers}
        onSubmit={handleSubmit}
        onSkip={handleSkip}
        autosaveKey="preference-wizard"
      />
      {saving && <div className="preferences-saving">Saving...</div>}
      {successMessage && <div className="preferences-saving success">{successMessage}</div>}
    </div>
  );
}
