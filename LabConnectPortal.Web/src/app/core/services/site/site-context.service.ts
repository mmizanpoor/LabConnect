import { Injectable, inject } from '@angular/core';
import { SiteSettingsPublicDto } from '@modules/admin/settings/site-settings.types';
import { PublicSiteService } from './public-site.service';
import { SeoService } from './seo.service';

@Injectable({ providedIn: 'root' })
export class SiteContextService {
  private _publicSiteService = inject(PublicSiteService);
  private _seoService = inject(SeoService);

  settings: SiteSettingsPublicDto | null = null;
  private _loadPromise: Promise<SiteSettingsPublicDto | null> | null = null;

  ensureLoaded(): Promise<SiteSettingsPublicDto | null> {
    if (this.settings) {
      return Promise.resolve(this.settings);
    }

    if (!this._loadPromise) {
      this._loadPromise = this.loadInternal();
    }

    return this._loadPromise;
  }

  private async loadInternal(): Promise<SiteSettingsPublicDto | null> {
    void this.trackVisitOnce();

    try {
      const result = await this._publicSiteService.getSettings();
      if (result.success && result.data) {
        this.settings = result.data;
        this._seoService.applySettings(result.data);
      }
    } catch {
      // Public pages still render with defaults when settings fail to load.
    }

    return this.settings;
  }

  private async trackVisitOnce(): Promise<void> {
    if (sessionStorage.getItem('siteVisitTracked') === '1') {
      return;
    }

    try {
      const result = await this._publicSiteService.trackVisit();
      if (result.success) {
        sessionStorage.setItem('siteVisitTracked', '1');
      }
    } catch {
      // Visit tracking is best-effort and should not block the app.
    }
  }
}
