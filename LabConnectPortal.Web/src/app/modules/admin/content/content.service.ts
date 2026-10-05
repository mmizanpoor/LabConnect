import { Injectable } from '@angular/core';
import { ApiHttpService } from '@core/services/http/api-http.service';
import { environment } from '@env/environment';
import {
  ContentGroupDto,
  ContentPostDto,
  ContentPostListItemDto,
  GetContentGroupsQuery,
  GetContentPostsQuery,
  PagedResult,
  SaveContentGroupCommand,
  SaveContentPostCommand,
  UpdateContentGroupCommand,
  UpdateContentPostCommand,
} from './content.types';

@Injectable({ providedIn: 'root' })
export class ContentService {
  constructor(private _http: ApiHttpService) {}

  getGroups(query: GetContentGroupsQuery) {
    return this._http.post<PagedResult<ContentGroupDto>>('ContentGroup', 'GetAll', query);
  }

  getGroupOptions() {
    return this._http.get<ContentGroupDto[]>('ContentGroup', 'GetAllOptions');
  }

  createGroup(command: SaveContentGroupCommand) {
    return this._http.post<ContentGroupDto>('ContentGroup', 'Create', command);
  }

  updateGroup(command: UpdateContentGroupCommand) {
    return this._http.post<ContentGroupDto>('ContentGroup', 'Update', command);
  }

  deleteGroup(contentGroupId: number) {
    return this._http.delete('ContentGroup', 'Delete', { contentGroupId });
  }

  getPosts(query: GetContentPostsQuery) {
    return this._http.post<PagedResult<ContentPostListItemDto>>('ContentPost', 'GetAll', query);
  }

  getPostById(contentPostId: string) {
    return this._http.get<ContentPostDto>('ContentPost', 'GetById', { contentPostId });
  }

  createPost(command: SaveContentPostCommand) {
    return this._http.post<ContentPostDto>('ContentPost', 'Create', command);
  }

  updatePost(command: UpdateContentPostCommand) {
    return this._http.post<ContentPostDto>('ContentPost', 'Update', command);
  }

  deletePost(contentPostId: string) {
    return this._http.delete('ContentPost', 'Delete', { contentPostId });
  }

  uploadFeaturedImage(contentPostId: string, file: File) {
    const formData = new FormData();
    formData.append('file', file);
    return this._http.postForm<ContentPostDto>('ContentPost', 'UploadFeaturedImage', formData, {
      contentPostId,
    });
  }

  deleteFeaturedImage(contentPostId: string) {
    return this._http.post<ContentPostDto>('ContentPost', 'DeleteFeaturedImage', {}, { contentPostId });
  }

  getFeaturedImageUrl(imagePath: string): string {
    return `${environment.apiUrl}ContentPost/GetFeaturedImage?path=${encodeURIComponent(imagePath)}`;
  }
}
