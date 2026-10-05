import { Injectable } from '@angular/core';
import { ApiHttpService } from '@core/services/http/api-http.service';
import { environment } from '@env/environment';
import { GetPublishedPostsQuery, PagedResult, PublicPostCardDto, PublicPostDetailDto, ContentGroupDto } from '../admin/content/content.types';

@Injectable({ providedIn: 'root' })
export class PublicPostsService {
  constructor(private _http: ApiHttpService) {}

  getPublishedPosts(query: GetPublishedPostsQuery) {
    return this._http.post<PagedResult<PublicPostCardDto>>('PublicPost', 'GetPublishedPosts', query);
  }

  getHomePosts(pageSize = 6) {
    return this._http.get<PublicPostCardDto[]>('PublicPost', 'GetHomePosts', { pageSize });
  }

  getById(contentPostId: string) {
    return this._http.get<PublicPostDetailDto>('PublicPost', 'GetById', { contentPostId });
  }

  getPublishedGroups() {
    return this._http.get<ContentGroupDto[]>('PublicPost', 'GetPublishedGroups');
  }

  recordView(contentPostId: string) {
    return this._http.post('PublicPost', 'RecordView', {}, { contentPostId });
  }

  static featuredImageUrl(
    path: string | null | undefined,
    maxWidth?: number
  ): string {
    if (!path) return '';
    const width = maxWidth ? `&w=${maxWidth}` : '';
    return `${environment.apiUrl}ContentPost/GetFeaturedImage?path=${encodeURIComponent(path)}${width}`;
  }
}
