import { Injectable } from '@angular/core';
import { ApiHttpService } from '@core/services/http/api-http.service';
import { environment } from '@env/environment';
import {
  CreateProductReviewCommand,
  GetPendingProductReviewsQuery,
  GetProductReviewsQuery,
  GetPublishedListingsQuery,
  PagedProductReviews,
  PagedPublicProducts,
  ProductReviewEligibilityDto,
  ProductReviewListItemDto,
  ProductReviewReplyDto,
  ProductSearchSuggestionDto,
  ProductUserReviewDto,
  PublicProductListingDetailDto,
  PublicListingFiltersDto,
  PublicFilterAttributeDto,
  PublicBrandFilterDto,
  ProductResumeApplicationDto,
  ReplyToReviewCommand,
  SellerContactDto,
} from './products.types';

@Injectable({ providedIn: 'root' })
export class PublicProductsService {
  constructor(private _http: ApiHttpService) {}

  getPublishedListings(query: GetPublishedListingsQuery) {
    return this._http.post<PagedPublicProducts>('PublicProduct', 'GetPublishedListings', query);
  }

  getProductDetail(productId: string) {
    return this._http.get<PublicProductListingDetailDto>('PublicProduct', 'GetListingDetail', { productId });
  }

  searchSuggestions(term: string, limit = 8) {
    return this._http.get<ProductSearchSuggestionDto[]>('PublicProduct', 'SearchSuggestions', { term, limit });
  }

  getSellerContact(productId: string) {
    return this._http.get<SellerContactDto>('PublicProduct', 'GetSellerContact', { productId });
  }

  submitResume(productId: string) {
    return this._http.post<ProductResumeApplicationDto>('ProductResumeApplication', 'Submit', { productId });
  }

  getMyResumeApplications() {
    return this._http.get<ProductResumeApplicationDto[]>('ProductResumeApplication', 'GetMyApplications');
  }

  recordView(productId: string) {
    return this._http.post<void>('PublicProduct', 'RecordView', {}, { productId });
  }

  getListingFilters() {
    return this._http.get<PublicListingFiltersDto>('PublicProduct', 'GetListingFilters');
  }

  getFilterAttributes(categoryGroupId: number, categoryId?: number) {
    return this._http.get<PublicFilterAttributeDto[]>('PublicProduct', 'GetFilterAttributes', {
      categoryGroupId,
      ...(categoryId ? { categoryId } : {}),
    });
  }

  getFilterBrands(categoryGroupId: number, categoryId?: number) {
    return this._http.get<PublicBrandFilterDto[]>('PublicProduct', 'GetFilterBrands', {
      categoryGroupId,
      ...(categoryId ? { categoryId } : {}),
    });
  }

  static productImageUrl(
    path: string | null | undefined,
    maxWidth?: number
  ): string {
    if (!path) return '';
    const width = maxWidth ? `&w=${maxWidth}` : '';
    return `${environment.apiUrl}Product/GetImage?path=${encodeURIComponent(path)}${width}`;
  }

  static centerLogoUrl(centerProfileId: string): string {
    return `${environment.apiUrl}PublicSpecialOffer/GetLabLogo?profileId=${centerProfileId}`;
  }
}

@Injectable({ providedIn: 'root' })
export class ProductReviewService {
  constructor(private _http: ApiHttpService) {}

  create(command: CreateProductReviewCommand) {
    return this._http.post<ProductUserReviewDto>('ProductReview', 'Create', command);
  }

  reply(command: ReplyToReviewCommand) {
    return this._http.post<ProductReviewReplyDto>('ProductReview', 'Reply', command);
  }

  getReviewEligibility(productId: string) {
    return this._http.get<ProductReviewEligibilityDto>('ProductReview', 'GetReviewEligibility', { productId });
  }

  getSellerProductReviews(query: GetProductReviewsQuery) {
    return this._http.post<PagedProductReviews<ProductUserReviewDto>>('ProductReview', 'GetSellerProductReviews', query);
  }

  getPendingReviews(query: GetPendingProductReviewsQuery) {
    return this._http.post<PagedProductReviews<ProductReviewListItemDto>>('ProductReview', 'GetPendingReviews', query);
  }

  approve(reviewId: string) {
    return this._http.post('ProductReview', 'Approve', { reviewId });
  }

  reject(reviewId: string) {
    return this._http.post('ProductReview', 'Reject', { reviewId });
  }
}
