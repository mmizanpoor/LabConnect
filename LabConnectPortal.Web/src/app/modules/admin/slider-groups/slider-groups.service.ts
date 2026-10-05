import { Injectable } from '@angular/core';
import { ApiHttpService } from '@core/services/http/api-http.service';
import { environment } from '@env/environment';
import {
  DeleteSliderSlideCommand,
  GetSliderGroupsQuery,
  PagedResult,
  SliderGroupDto,
  SliderGroupListItemDto,
  SliderSlideDto,
  UpdateSliderGroupCommand,
  UpdateSliderSlideCommand,
  SaveSliderGroupCommand,
} from './slider-groups.types';

@Injectable({ providedIn: 'root' })
export class SliderGroupsService {
  constructor(private _http: ApiHttpService) {}

  getAll(query: GetSliderGroupsQuery) {
    return this._http.post<PagedResult<SliderGroupListItemDto>>('SliderGroup', 'GetAll', query);
  }

  getById(id: string) {
    return this._http.get<SliderGroupDto>('SliderGroup', 'GetById', { id });
  }

  create(command: SaveSliderGroupCommand) {
    return this._http.post<SliderGroupDto>('SliderGroup', 'Create', command);
  }

  update(command: UpdateSliderGroupCommand) {
    return this._http.post<SliderGroupDto>('SliderGroup', 'Update', command);
  }

  delete(id: string) {
    return this._http.delete('SliderGroup', 'Delete', { id });
  }

  uploadSlideImage(sliderGroupId: string, file: File) {
    const formData = new FormData();
    formData.append('file', file);
    return this._http.postForm<SliderSlideDto>('SliderGroup', 'UploadSlideImage', formData, { sliderGroupId });
  }

  updateSlide(command: UpdateSliderSlideCommand) {
    return this._http.post<SliderSlideDto>('SliderGroup', 'UpdateSlide', command);
  }

  reorderSlides(command: { sliderGroupId: string; slideIds: string[] }) {
    return this._http.post('SliderGroup', 'ReorderSlides', command);
  }

  deleteSlide(command: DeleteSliderSlideCommand) {
    return this._http.post('SliderGroup', 'DeleteSlide', command);
  }

  static slideImageUrl(imagePath: string): string {
    return `${environment.apiUrl}SliderGroup/GetSlideImage?path=${encodeURIComponent(imagePath)}`;
  }
}
