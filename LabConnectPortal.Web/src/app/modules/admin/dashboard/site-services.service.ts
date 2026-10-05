import { Injectable } from '@angular/core';
import { ApiHttpService } from '@core/services/http/api-http.service';
import { OperationResult } from '@core/types/operation-result.types';
import { environment } from '@env/environment';
import { SiteServiceDto } from './site-services.types';

@Injectable({ providedIn: 'root' })
export class SiteServicesService {
  constructor(private _http: ApiHttpService) {}

  getAll(): Promise<OperationResult<SiteServiceDto[]>> {
    return this._http.get<SiteServiceDto[]>('SiteService', 'GetAll', {
      _: Date.now(),
    });
  }

  create(cmd: Omit<SiteServiceDto, 'siteServiceId'>): Promise<OperationResult<SiteServiceDto>> {
    return this._http.post<SiteServiceDto>('SiteService', 'Create', cmd);
  }

  update(cmd: SiteServiceDto): Promise<OperationResult<SiteServiceDto>> {
    return this._http.post<SiteServiceDto>('SiteService', 'Update', cmd);
  }

  uploadImage(id: number, file: File): Promise<OperationResult<SiteServiceDto>> {
    const formData = new FormData();
    formData.append('file', file);
    return this._http.postForm<SiteServiceDto>('SiteService', 'UploadImage', formData, { id });
  }

  deleteImage(id: number): Promise<OperationResult<SiteServiceDto>> {
    return this._http.post<SiteServiceDto>('SiteService', 'DeleteImage', {}, { id });
  }

  delete(id: number): Promise<OperationResult<void>> {
    return this._http.delete<void>('SiteService', 'Delete', { id });
  }

  getImageUrl(imagePath: string): string {
    return `${environment.apiUrl}SiteService/GetImage?path=${encodeURIComponent(imagePath)}`;
  }
}

