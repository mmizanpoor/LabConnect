import { Injectable } from '@angular/core';
import { ApiHttpService } from '@core/services/http/api-http.service';
import { environment } from '@env/environment';
import { SiteSettingsPublicDto } from '@modules/admin/settings/site-settings.types';
import { ActiveSliderSlideDto } from '@modules/admin/slider-groups/slider-groups.types';
import { SiteServiceDto } from '@modules/admin/dashboard/site-services.types';

@Injectable({ providedIn: 'root' })
export class PublicSiteService {
  constructor(private _http: ApiHttpService) {}

  getSettings() {
    return this._http.get<SiteSettingsPublicDto>('PublicSite', 'GetSettings');
  }

  getActiveSlides() {
    return this._http.get<ActiveSliderSlideDto[]>(
      'PublicSite',
      'GetActiveSlides'
    );
  }

  getActiveServices() {
    return this._http.get<SiteServiceDto[]>('PublicSite', 'GetActiveServices');
  }

  trackVisit() {
    return this._http.post('PublicSite', 'TrackVisit', {});
  }

  static logoUrl(): string {
    return `${environment.apiUrl}SiteSettings/GetLogo`;
  }

  static footerLogoUrl(): string {
    return `${environment.apiUrl}SiteSettings/GetFooterLogo`;
  }

  static slideImageUrl(imagePath: string, maxWidth?: number): string {
    const width = maxWidth ? `&w=${maxWidth}` : '';
    return `${
      environment.apiUrl
    }SliderGroup/GetSlideImage?path=${encodeURIComponent(imagePath)}${width}`;
  }

  /** Responsive hero sources for the home slider (width descriptor = requested max width). */
  static slideImageSrcSet(imagePath: string): string {
    return [
      `${this.slideImageUrl(imagePath, 640)} 640w`,
      `${this.slideImageUrl(imagePath, 1280)} 1280w`,
      `${this.slideImageUrl(imagePath, 1920)} 1920w`,
      `${this.slideImageUrl(imagePath, 2880)} 2880w`,
    ].join(', ');
  }

  static siteServiceImageUrl(imagePath: string, maxWidth?: number): string {
    const width = maxWidth ? `&w=${maxWidth}` : '';
    return `${environment.apiUrl}SiteService/GetImage?path=${encodeURIComponent(
      imagePath
    )}${width}`;
  }
}
