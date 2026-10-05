import { Injectable, inject } from '@angular/core';
import { NavigationEnd, Params, Router } from '@angular/router';
import { BehaviorSubject, filter } from 'rxjs';

const FLAG_KEY = 'labconnect.isLoggedInFromAzmoonPro';
const LAB_CODE_NEW_KEY = 'labconnect.azmoonLabCodeNew';
const AZMOON_PROFILE_CHROME_PATH =
  /\/profile\/(?:product-listings|special-offers|center|agreement-settings|test-infos)(?:\/|$|\?|#)/i;
const AZMOON_FLAG_IN_URL = /[?&#]isLoggedInFromAzmoonPro(?:=([^&?#]*))?/i;

@Injectable({ providedIn: 'root' })
export class AzmoonProContextService {
  private _router = inject(Router);
  private _hideChrome$ = new BehaviorSubject(false);
  private _labCodeNew$ = new BehaviorSubject(this.readStoredLabCodeNew());

  readonly hideChrome$ = this._hideChrome$.asObservable();

  constructor() {
    this.syncFromUrl();
    this._router.events
      .pipe(
        filter(
          (event): event is NavigationEnd => event instanceof NavigationEnd
        )
      )
      .subscribe((event) => this.syncFromUrl(event.urlAfterRedirects));
  }

  get hideChrome(): boolean {
    return this._hideChrome$.value;
  }

  get labCodeNew(): string {
    return this._labCodeNew$.value;
  }

  isLoggedInFromAzmoonPro(url?: string): boolean {
    const parsed = this.parseFromCandidates(url);
    if (parsed.flagPresent) {
      return parsed.isLoggedInFromAzmoonPro;
    }

    return parsed.isLoggedInFromAzmoonPro || this.readSessionFlag();
  }

  shouldHidePublicChrome(url?: string): boolean {
    return this.isLoggedInFromAzmoonPro(url);
  }

  shouldHideProfileChrome(url?: string): boolean {
    if (!this.isAzmoonProfileChromePath(url)) {
      return false;
    }

    const parsed = this.parseFromCandidates(url);
    if (parsed.flagPresent) {
      return parsed.isLoggedInFromAzmoonPro;
    }

    return this.readSessionFlag();
  }

  clearCapturedUrlParams(): void {
    this.writeStoredFlag(false);
    this.removeStoredLabCodeNew();
    this._labCodeNew$.next('');
    if (this._hideChrome$.value) {
      this._hideChrome$.next(false);
    }
  }

  syncFromUrl(url?: string): void {
    const parsed = this.parseFromCandidates(url);

    if (parsed.labCodeNew && parsed.labCodeNew !== this._labCodeNew$.value) {
      this.writeStoredLabCodeNew(parsed.labCodeNew);
      this._labCodeNew$.next(parsed.labCodeNew);
    }

    if (parsed.flagPresent) {
      this.writeStoredFlag(parsed.isLoggedInFromAzmoonPro);
    } else if (parsed.isLoggedInFromAzmoonPro) {
      this.writeStoredFlag(true);
    }

    const nextHideChrome = parsed.flagPresent
      ? parsed.isLoggedInFromAzmoonPro
      : parsed.isLoggedInFromAzmoonPro || this.readSessionFlag();
    if (this._hideChrome$.value !== nextHideChrome) {
      this._hideChrome$.next(nextHideChrome);
    }

    if (!this._labCodeNew$.value) {
      this._labCodeNew$.next(this.readStoredLabCodeNew());
    }
  }

  isAzmoonProfileChromePath(url?: string): boolean {
    return this.candidateUrls(url).some((candidate) =>
      AZMOON_PROFILE_CHROME_PATH.test(candidate)
    );
  }

  private parseFromCandidates(url?: string) {
    let flagPresent = false;
    let isLoggedInFromAzmoonPro = false;
    let labCodeNew = '';
    let labCode = '';

    for (const candidate of this.candidateUrls(url)) {
      const fromRaw = parseAzmoonProFromRawUrl(candidate);
      flagPresent ||= fromRaw.flagPresent;
      isLoggedInFromAzmoonPro ||= fromRaw.isLoggedInFromAzmoonPro;
      labCodeNew ||= fromRaw.labCodeNew;
      labCode ||= fromRaw.labCode;

      try {
        const fromRouter = parseAzmoonProQuery(
          this._router.parseUrl(candidate).queryParams
        );
        flagPresent ||= fromRouter.flagPresent;
        isLoggedInFromAzmoonPro ||= fromRouter.isLoggedInFromAzmoonPro;
        labCodeNew ||= fromRouter.labCodeNew;
        labCode ||= fromRouter.labCode;
      } catch {
        // Ignore malformed router URLs and keep scanning other candidates.
      }
    }

    return { flagPresent, isLoggedInFromAzmoonPro, labCodeNew, labCode };
  }

  private candidateUrls(url?: string): string[] {
    const urls = [url, this._router.url];
    if (typeof window !== 'undefined') {
      urls.push(
        window.location.href,
        window.location.search,
        window.location.hash
      );
    }
    return [...new Set(urls.filter((value): value is string => !!value))];
  }

  private readSessionFlag(): boolean {
    try {
      return sessionStorage.getItem(FLAG_KEY) === 'true';
    } catch {
      return false;
    }
  }

  private writeStoredFlag(value: boolean): void {
    try {
      if (value) {
        sessionStorage.setItem(FLAG_KEY, 'true');
        localStorage.setItem(FLAG_KEY, 'true');
      } else {
        sessionStorage.removeItem(FLAG_KEY);
        localStorage.removeItem(FLAG_KEY);
      }
    } catch {
      // Ignore storage failures in restricted iframe contexts.
    }
  }

  private readStoredLabCodeNew(): string {
    try {
      return (
        sessionStorage.getItem(LAB_CODE_NEW_KEY)?.trim() ||
        localStorage.getItem(LAB_CODE_NEW_KEY)?.trim() ||
        ''
      );
    } catch {
      return '';
    }
  }

  private writeStoredLabCodeNew(value: string): void {
    try {
      if (value) {
        sessionStorage.setItem(LAB_CODE_NEW_KEY, value);
        localStorage.setItem(LAB_CODE_NEW_KEY, value);
      }
    } catch {
      // Ignore storage failures in restricted iframe contexts.
    }
  }

  private removeStoredLabCodeNew(): void {
    try {
      sessionStorage.removeItem(LAB_CODE_NEW_KEY);
      localStorage.removeItem(LAB_CODE_NEW_KEY);
    } catch {
      // Ignore storage failures in restricted iframe contexts.
    }
  }
}

export function parseAzmoonProQuery(queryParams: Params): {
  flagPresent: boolean;
  isLoggedInFromAzmoonPro: boolean;
  labCodeNew: string;
  labCode: string;
} {
  const rawParam = readQueryValue(queryParams, 'isLoggedInFromAzmoonPro');
  const raw =
    rawParam === true || rawParam === 1 ? 'true' : String(rawParam ?? '');
  let labCodeNew = String(
    readQueryValue(queryParams, 'labCodeNew') ?? ''
  ).trim();
  let labCode = String(readQueryValue(queryParams, 'labCode') ?? '').trim();
  let decoded = raw.trim();
  try {
    decoded = decodeURIComponent(decoded);
  } catch {
    // Keep the original value when it is not percent-encoded.
  }

  const extraIndex = decoded.search(/[?&]/);
  const flagPart = extraIndex >= 0 ? decoded.slice(0, extraIndex) : decoded;

  if (extraIndex >= 0) {
    const extra = new URLSearchParams(
      decoded.slice(extraIndex + 1).replace(/^\?/, '')
    );
    if (!labCodeNew) {
      labCodeNew = extra.get('labCodeNew')?.trim() ?? '';
    }
    if (!labCode) {
      labCode = extra.get('labCode')?.trim() ?? '';
    }
  }

  const flagPresent =
    (rawParam != null && String(rawParam).length > 0) || flagPart.length > 0;
  const normalized = flagPart.trim().toLowerCase();
  const isLoggedInFromAzmoonPro =
    normalized === 'true' ||
    normalized === '1' ||
    normalized === 'yes' ||
    (flagPresent && normalized === '');

  return { flagPresent, isLoggedInFromAzmoonPro, labCodeNew, labCode };
}

export function parseAzmoonProFromRawUrl(url: string): {
  flagPresent: boolean;
  isLoggedInFromAzmoonPro: boolean;
  labCodeNew: string;
  labCode: string;
} {
  let decoded = url;
  try {
    decoded = decodeURIComponent(url);
  } catch {
    decoded = url;
  }
  decoded = decoded.replace(/\?([^?#]*)\?/, '?$1&');

  const flagMatch = decoded.match(AZMOON_FLAG_IN_URL);
  const flagValue = flagMatch?.[1]?.trim().toLowerCase() ?? '';
  const flagPresent = !!flagMatch;
  const isLoggedInFromAzmoonPro =
    flagPresent &&
    (flagValue === '' ||
      flagValue === 'true' ||
      flagValue === '1' ||
      flagValue === 'yes');

  const labCodeNew =
    decoded.match(/[?&#]labCodeNew=([^&?#]*)/i)?.[1]?.trim() ?? '';
  const labCode = decoded.match(/[?&#]labCode=([^&?#]*)/i)?.[1]?.trim() ?? '';

  return { flagPresent, isLoggedInFromAzmoonPro, labCodeNew, labCode };
}

function readQueryValue(queryParams: Params, key: string): unknown {
  const direct = queryParams[key];
  if (direct != null && direct !== '') {
    return direct;
  }
  const matched = Object.keys(queryParams).find(
    (item) => item.toLowerCase() === key.toLowerCase()
  );
  return matched ? queryParams[matched] : undefined;
}
