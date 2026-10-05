import { Injectable } from '@angular/core';
import { TranslocoService } from '@jsverse/transloco';
import { LanguageDirection, SupportedLanguage, SupportedLanguages } from './langs';

@Injectable({ providedIn: 'root' })
export class LocalizationService {
  constructor(private _translocoService: TranslocoService) {
    this.transloco = _translocoService;
  }

  public transloco: TranslocoService;

  public get isRtl(): boolean {
    return SupportedLanguages.find((l) => l.id === this._translocoService.getActiveLang())?.direction === LanguageDirection.rtl;
  }

  public translate(key: string, params?: Record<string, unknown>): string {
    return this._translocoService.translate(key, params);
  }

  public translateEnum(group: string, value: string): string {
    return this.translate(`enum.${group}.${value}`);
  }

  public setActiveLang(lang: string): void {
    localStorage.setItem('lang', lang);
    location.reload();
  }

  public getActiveLang(): SupportedLanguage {
    return SupportedLanguages.find((l) => l.id === this.transloco.getActiveLang()) ?? SupportedLanguages[0];
  }

  public getLangFromLocalStorage(): string {
    const lang = localStorage.getItem('lang') ?? 'fa';
    if (this._translocoService.getAvailableLangs().findIndex((l) => (typeof l === 'string' ? l : l.id) === lang) === -1) {
      return 'fa';
    }
    return lang;
  }
}
