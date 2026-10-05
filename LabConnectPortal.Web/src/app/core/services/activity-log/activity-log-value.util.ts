import jMoment from 'moment-jalaali';

/**
 * Formats activity-log field values for display.
 * Converts Gregorian / UTC date strings to Jalali (Shamsi).
 */
export function formatActivityLogDisplayValue(value: string): string {
  const trimmed = value.trim();
  if (!trimmed) return '';

  const utcMatch = trimmed.match(
    /^(\d{4}-\d{2}-\d{2})[ T](\d{2}:\d{2}:\d{2})(?:\.\d+)?(?:\s*UTC)?$/i,
  );
  if (utcMatch) {
    const [, datePart, timePart] = utcMatch;
    if (timePart === '00:00:00') {
      const dateOnly = jMoment(datePart, 'YYYY-MM-DD').locale('fa');
      if (dateOnly.isValid()) {
        return `\u2066${dateOnly.format('jYYYY/jMM/jDD')}\u2069`;
      }
    }

    const parsed = jMoment.utc(`${datePart} ${timePart}`, 'YYYY-MM-DD HH:mm:ss');
    if (parsed.isValid()) {
      return `\u2066${parsed.local().locale('fa').format('jYYYY/jMM/jDD HH:mm')}\u2069`;
    }
  }

  const dateOnlyMatch = trimmed.match(/^(\d{4}-\d{2}-\d{2})$/);
  if (dateOnlyMatch) {
    const parsed = jMoment(dateOnlyMatch[1], 'YYYY-MM-DD').locale('fa');
    if (parsed.isValid()) {
      return `\u2066${parsed.format('jYYYY/jMM/jDD')}\u2069`;
    }
  }

  if (/^\d{4}-\d{2}-\d{2}T/.test(trimmed) || /Z$/i.test(trimmed)) {
    const parsed = jMoment(trimmed).locale('fa');
    if (parsed.isValid()) {
      const hasTime = !/T00:00:00/.test(trimmed);
      const format = hasTime ? 'jYYYY/jMM/jDD HH:mm' : 'jYYYY/jMM/jDD';
      return `\u2066${parsed.format(format)}\u2069`;
    }
  }

  return trimmed;
}
