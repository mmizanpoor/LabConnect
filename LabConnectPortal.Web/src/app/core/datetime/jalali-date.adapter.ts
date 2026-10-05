import { DateAdapter } from '@angular/material/core';
import jMoment, { Moment } from 'moment-jalaali';

jMoment.loadPersian({ dialect: 'persian-modern', usePersianDigits: true });

export class JalaliDateAdapter extends DateAdapter<Moment> {
  constructor() {
    super();
    this.setLocale('fa');
  }

  override getYear(date: Moment): number {
    return this.clone(date).jYear();
  }

  override getMonth(date: Moment): number {
    return this.clone(date).jMonth();
  }

  override getDate(date: Moment): number {
    return this.clone(date).jDate();
  }

  override getDayOfWeek(date: Moment): number {
    return this.clone(date).day();
  }

  override getMonthNames(style: 'long' | 'short' | 'narrow'): string[] {
    const localeData = jMoment().locale('fa').localeData() as jMoment.Locale & {
      _jMonths?: string[];
      _jMonthsShort?: string[];
    };

    if (style === 'narrow' && localeData._jMonthsShort?.length) {
      return [...localeData._jMonthsShort];
    }

    if (localeData._jMonths?.length) {
      return [...localeData._jMonths];
    }

    return localeData.months().slice(0);
  }

  override getDateNames(): string[] {
    const localeData = jMoment().locale('fa').localeData() as jMoment.Locale & {
      postformat?: (value: string) => string;
    };

    return Array.from({ length: 31 }, (_, i) => {
      const value = String(i + 1);
      return localeData.postformat ? localeData.postformat(value) : value;
    });
  }

  override getDayOfWeekNames(style: 'long' | 'short' | 'narrow'): string[] {
    const localeData = jMoment().localeData();
    switch (style) {
      case 'long':
        return localeData.weekdays().slice(0);
      case 'short':
        return localeData.weekdaysShort().slice(0);
      default:
        return localeData.weekdaysMin().slice(0);
    }
  }

  override getYearName(date: Moment): string {
    return this.clone(date).jYear().toString();
  }

  override getFirstDayOfWeek(): number {
    return 6;
  }

  override getNumDaysInMonth(date: Moment): number {
    return jMoment.jDaysInMonth(this.getYear(date), this.getMonth(date));
  }

  override clone(date: Moment): Moment {
    return date.clone().locale('fa');
  }

  override createDate(year: number, month: number, date: number): Moment {
    return jMoment()
      .jYear(year)
      .jMonth(month)
      .jDate(date)
      .hours(0)
      .minutes(0)
      .seconds(0)
      .milliseconds(0);
  }

  override today(): Moment {
    return jMoment().locale('fa');
  }

  override parse(value: unknown, parseFormat: string | string[]): Moment | null {
    if (value == null || value === '') {
      return null;
    }
    if (typeof value === 'string') {
      const parsed = jMoment(value, parseFormat, 'fa', true);
      return parsed.isValid() ? parsed : null;
    }
    if (this.isDateInstance(value)) {
      return this.clone(value);
    }
    return null;
  }

  override format(date: Moment, displayFormat: string): string {
    const cloned = this.clone(date);
    if (!this.isValid(cloned)) {
      return '';
    }
    return cloned.format(displayFormat);
  }

  override addCalendarYears(date: Moment, years: number): Moment {
    return this.clone(date).add(years, 'jYear');
  }

  override addCalendarMonths(date: Moment, months: number): Moment {
    return this.clone(date).add(months, 'jMonth');
  }

  override addCalendarDays(date: Moment, days: number): Moment {
    return this.clone(date).add(days, 'day');
  }

  override toIso8601(date: Moment): string {
    return this.clone(date).format('YYYY-MM-DD');
  }

  override isDateInstance(obj: unknown): obj is Moment {
    return jMoment.isMoment(obj);
  }

  override isValid(date: Moment): boolean {
    return this.clone(date).isValid();
  }

  override invalid(): Moment {
    return jMoment.invalid();
  }

  override deserialize(value: unknown): Moment | null {
    if (typeof value === 'string') {
      if (!value) return null;
      const parsed = jMoment(value, ['jYYYY/jMM/jDD', 'YYYY-MM-DD', jMoment.ISO_8601], 'fa', true);
      return parsed.isValid() ? parsed : null;
    }
    if (value instanceof Date) {
      const parsed = jMoment(value).locale('fa');
      return parsed.isValid() ? parsed : null;
    }
    if (this.isDateInstance(value)) {
      return this.isValid(value) ? this.clone(value) : null;
    }
    return super.deserialize(value);
  }
}
