import { validateAnswer } from './TravelPreferenceWizard';

describe('validateAnswer', () => {
  const baseQuestion = { id: 'q1', title: 'Question', required: true, type: 'single' };

  it('requires value for required questions', () => {
    const err = validateAnswer(baseQuestion, '');
    expect(err).toContain('required');
  });

  it('validates multi-select min/max', () => {
    const q = { ...baseQuestion, type: 'multi', minSelections: 1, maxSelections: 2 };
    expect(validateAnswer(q, [])).toContain('at least');
    expect(validateAnswer(q, ['a', 'b', 'c'])).toContain('up to');
    expect(validateAnswer(q, ['a'])).toBe('');
  });
});
