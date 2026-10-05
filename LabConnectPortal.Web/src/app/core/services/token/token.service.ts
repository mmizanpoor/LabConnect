import { Injectable } from '@angular/core';

@Injectable({ providedIn: 'root' })
export class TokenService {
  private readonly accessKey = 'lcp_access_token';
  private readonly refreshKey = 'lcp_refresh_token';

  get accessToken(): string | null {
    return localStorage.getItem(this.accessKey);
  }

  set accessToken(value: string | null) {
    if (value) localStorage.setItem(this.accessKey, value);
    else localStorage.removeItem(this.accessKey);
  }

  get refreshToken(): string | null {
    return localStorage.getItem(this.refreshKey);
  }

  set refreshToken(value: string | null) {
    if (value) localStorage.setItem(this.refreshKey, value);
    else localStorage.removeItem(this.refreshKey);
  }

  clear(): void {
    this.accessToken = null;
    this.refreshToken = null;
  }
}
