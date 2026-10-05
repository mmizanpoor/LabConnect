import { Injectable } from '@angular/core';
import { ApiHttpService } from '@core/services/http/api-http.service';
import { environment } from '@env/environment';
import { PublicCategoryGroupCardDto } from './products.types';

@Injectable({ providedIn: 'root' })
export class PublicCategoriesService {
  constructor(private _http: ApiHttpService) {}

  getHomeCategories() {
    return this._http.get<PublicCategoryGroupCardDto[]>('PublicCategory', 'GetHomeCategories');
  }

  static categoryHomePageImageUrl(
    path: string | null | undefined,
    maxWidth?: number
  ): string {
    if (!path) return '';
    const width = maxWidth ? `&w=${maxWidth}` : '';
    return `${environment.apiUrl}ProductCategoryGroup/GetHomePageImage?path=${encodeURIComponent(path)}${width}`;
  }
}
