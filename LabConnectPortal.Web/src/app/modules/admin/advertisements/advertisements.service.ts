import { Injectable } from '@angular/core';
import { ApiHttpService } from '@core/services/http/api-http.service';
import { environment } from '@env/environment';
import {
  AdvertisementDto,
  PublicAdvertisementCardDto,
  SaveAdvertisementCommand,
  UpdateAdvertisementCommand,
} from './advertisements.types';

@Injectable({ providedIn: 'root' })
export class AdvertisementsService {
  constructor(private _http: ApiHttpService) {}

  getAll() {
    return this._http.get<AdvertisementDto[]>('Advertisement', 'GetAll');
  }

  getById(advertisementId: string) {
    return this._http.get<AdvertisementDto>('Advertisement', 'GetById', { advertisementId });
  }

  create(command: SaveAdvertisementCommand) {
    return this._http.post<AdvertisementDto>('Advertisement', 'Create', command);
  }

  update(command: UpdateAdvertisementCommand) {
    return this._http.post<AdvertisementDto>('Advertisement', 'Update', command);
  }

  delete(advertisementId: string) {
    return this._http.delete('Advertisement', 'Delete', { advertisementId });
  }

  uploadImage(advertisementId: string, file: File) {
    const formData = new FormData();
    formData.append('file', file);
    return this._http.postForm<AdvertisementDto>('Advertisement', 'UploadImage', formData, {
      advertisementId,
    });
  }

  deleteImage(advertisementId: string) {
    return this._http.post<AdvertisementDto>('Advertisement', 'DeleteImage', {}, { advertisementId });
  }

  getImageUrl(imagePath: string | null | undefined): string {
    if (!imagePath) return '';
    return `${environment.apiUrl}Advertisement/GetImage?path=${encodeURIComponent(imagePath)}`;
  }
}

@Injectable({ providedIn: 'root' })
export class PublicAdvertisementsService {
  constructor(private _http: ApiHttpService) {}

  getActiveForHome(pageSize = 4) {
    return this._http.get<PublicAdvertisementCardDto[]>('PublicAdvertisement', 'GetActiveForHome', {
      pageSize,
    });
  }

  static imageUrl(path: string | null | undefined, maxWidth?: number): string {
    if (!path) return '';
    const width = maxWidth ? `&w=${maxWidth}` : '';
    return `${environment.apiUrl}Advertisement/GetImage?path=${encodeURIComponent(path)}${width}`;
  }
}
