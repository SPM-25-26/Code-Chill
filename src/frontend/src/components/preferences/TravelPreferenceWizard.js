import { useEffect, useMemo, useState } from 'react';
import PropTypes from 'prop-types';
import './TravelPreferenceWizard.css';

/**
 * @typedef {Object} Question
 * @property {string} id
 * @property {string} title
 * @property {string} type // binary | single | multi
 * @property {boolean} required
 * @property {number=} minSelections
 * @property {number=} maxSelections
 * @property {{value:string,label:string,helperText?:string}[]} options
 */

/**
 * Validate a single answer against the question definition.
 */
export function validateAnswer(question, value) {
  if (question.required && (value === undefined || value === null || value === '')) {
    return `${question.title} is required`;
  }

  if (question.type === 'multi') {
    const list = Array.isArray(value) ? value : [];
    if (question.minSelections != null && list.length < question.minSelections) {
      return `Select at least ${question.minSelections}`;
    }
    if (question.maxSelections != null && list.length > question.maxSelections) {
      return `Select up to ${question.maxSelections}`;
    }
  }

  return '';
}

/**
 * TravelPreferenceWizard renders a multi-step questionnaire UI.
 */
export default function TravelPreferenceWizard({
  questions,
  initialAnswers = {},
  onSubmit,
  onSkip,
  onCancel,
  autosaveKey = 'preference-wizard',
}) {
  const [answers, setAnswers] = useState(initialAnswers);
  const [step, setStep] = useState(0);
  const [errors, setErrors] = useState({});
  const [isSubmitting, setIsSubmitting] = useState(false);

  const orderedQuestions = useMemo(
    () => (questions || []).filter(Boolean),
    [questions],
  );

  // hydrate from localStorage if present
  useEffect(() => {
    if (!autosaveKey) return;
    try {
      const raw = localStorage.getItem(autosaveKey);
      if (raw) {
        const saved = JSON.parse(raw);
        setAnswers((prev) => ({ ...prev, ...saved }));
      }
    } catch (e) {
      // ignore parse errors
    }
  }, [autosaveKey]);

  // autosave on step change
  useEffect(() => {
    if (!autosaveKey) return;
    localStorage.setItem(autosaveKey, JSON.stringify(answers));
  }, [answers, autosaveKey]);

  useEffect(() => {
    setAnswers(initialAnswers || {});
  }, [initialAnswers]);

  if (!orderedQuestions.length) {
    return <div className="wizard-card">No questions available.</div>;
  }

  const current = orderedQuestions[step];

  const updateAnswer = (id, value) => {
    setAnswers((prev) => ({ ...prev, [id]: value }));
    setErrors((prev) => ({ ...prev, [id]: '' }));
  };

  const goNext = () => {
    const err = validateAnswer(current, answers[current.id]);
    if (err) {
      setErrors((prev) => ({ ...prev, [current.id]: err }));
      return;
    }
    if (step < orderedQuestions.length - 1) {
      setStep((s) => s + 1);
    } else {
      submit();
    }
  };

  const goBack = () => setStep((s) => Math.max(0, s - 1));

  const canSubmit = orderedQuestions.every((q) => !validateAnswer(q, answers[q.id]));

  const submit = async () => {
    if (!canSubmit) return;
    setIsSubmitting(true);
    try {
      await onSubmit?.(answers);
      if (autosaveKey) localStorage.removeItem(autosaveKey);
    } finally {
      setIsSubmitting(false);
    }
  };

  const renderOptions = () => {
    if (!current) return null;
    const selected = answers[current.id];

    if (current.type === 'multi') {
      const list = Array.isArray(selected) ? selected : [];
      return (
        <div className="wizard-options grid">
          {current.options.map((opt) => {
            const active = list.includes(opt.value);
            const toggle = () => {
              const next = active
                ? list.filter((v) => v !== opt.value)
                : [...list, opt.value];
              updateAnswer(current.id, next.slice(0, current.maxSelections || next.length));
            };
            return (
              <button
                key={opt.value}
                type="button"
                className={`wizard-chip ${active ? 'active' : ''}`}
                onClick={toggle}
              >
                {opt.label}
              </button>
            );
          })}
        </div>
      );
    }

    // binary or single
    return (
      <div className="wizard-options column">
        {current.options.map((opt) => {
          const active = selected === opt.value;
          return (
            <button
              key={opt.value}
              type="button"
              className={`wizard-card-option ${active ? 'active' : ''}`}
              onClick={() => updateAnswer(current.id, opt.value)}
            >
              <div className="wizard-option-title">{opt.label}</div>
              {opt.helperText && <div className="wizard-option-sub">{opt.helperText}</div>}
            </button>
          );
        })}
      </div>
    );
  };

  return (
    <div className="wizard-card">
      <div className="wizard-progress">
        Step {step + 1} of {orderedQuestions.length}
      </div>

      <h2 className="wizard-title">{current.title}</h2>
      {renderOptions()}
      {errors[current.id] && <div className="wizard-error">{errors[current.id]}</div>}

      <div className="wizard-actions">
        <button className="wizard-btn secondary" onClick={goBack} disabled={step === 0 || isSubmitting}>
          Back
        </button>
        <div className="wizard-action-gap" />
        {step < orderedQuestions.length - 1 ? (
          <button className="wizard-btn primary" onClick={goNext} disabled={isSubmitting}>
            Next
          </button>
        ) : (
          <button
            className="wizard-btn primary"
            onClick={submit}
            disabled={isSubmitting || !canSubmit}
          >
            {isSubmitting ? 'Saving...' : 'Complete'}
          </button>
        )}
      </div>
      {onCancel && (
        <button className="wizard-link" type="button" onClick={onCancel} disabled={isSubmitting}>
          Cancel
        </button>
      )}
      {onSkip && (
        <button className="wizard-link" type="button" onClick={onSkip} disabled={isSubmitting}>
          Skip for now
        </button>
      )}
    </div>
  );
}

TravelPreferenceWizard.propTypes = {
  questions: PropTypes.arrayOf(PropTypes.object).isRequired,
  initialAnswers: PropTypes.object,
  onSubmit: PropTypes.func,
  onSkip: PropTypes.func,
  onCancel: PropTypes.func,
  autosaveKey: PropTypes.string,
};
