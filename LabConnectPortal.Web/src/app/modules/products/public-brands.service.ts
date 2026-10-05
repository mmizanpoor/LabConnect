import { Injectable } from '@angular/core';
import { ApiHttpService } from '@core/services/http/api-http.service';
import { environment } from '@env/environment';
import { PublicBrandCardDto } from './products.types';

@Injectable({ providedIn: 'root' })
export class PublicBrandsService {
  constructor(private _http: ApiHttpService) {}

  getHomeBrands() {
    return this._http.get<PublicBrandCardDto[]>('PublicBrand', 'GetHomeBrands');
  }

  static brandImageUrl(path: string | null | undefined, maxWidth?: number): string {
    if (!path) return '';
    const width = maxWidth ? `&w=${maxWidth}` : '';
    return `${environment.apiUrl}Brand/GetImage?path=${encodeURIComponent(path)}${width}`;
  }
}
