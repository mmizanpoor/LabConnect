import { Injectable } from '@angular/core';
import { ApiHttpService } from '@core/services/http/api-http.service';
import { environment } from '@env/environment';
import { SiteSettingsDto, UpdateSiteSettingsCommand } from './site-settings.types';

@Injectable({ providedIn: 'root' })
export class SiteSettingsService {
  constructor(private _http: ApiHttpService) {}

  get() {
    return this._http.get<SiteSettingsDto>('SiteSettings', 'Get');
  }

  update(command: UpdateSiteSettingsCommand) {
    return this._http.post<SiteSettingsDto>('SiteSettings', 'Update', command);
  }

  uploadLogo(file: File) {
    const formData = new FormData();
    formData.append('file', file);
    return this._http.postForm<SiteSettingsDto>('SiteSettings', 'UploadLogo', formData);
  }

  uploadFooterLogo(file: File) {
    const formData = new FormData();
    formData.append('file', file);
    return this._http.postForm<SiteSettingsDto>('SiteSettings', 'UploadFooterLogo', formData);
  }

  getLogoBlob() {
    return this._http.getBlob('SiteSettings', 'GetLogo');
  }

  getFooterLogoBlob() {
    return this._http.getBlob('SiteSettings', 'GetFooterLogo');
  }

  static logoUrl(): string {
    return `${environment.apiUrl}SiteSettings/GetLogo`;
  }

  static footerLogoUrl(): string {
    return `${environment.apiUrl}SiteSettings/GetFooterLogo`;
  }
}
