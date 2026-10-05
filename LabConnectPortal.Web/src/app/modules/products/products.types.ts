import {
  ProductAttributeValueDto,
  ProductExpertReviewDto,
  ProductImageDto,
} from '@modules/admin/products/product-catalog.types';
import { DegreeLevel } from '@modules/profile/resume/resume.types';

export interface PublicProductListingCardDto {
  productId: string;
  title: string;
  brandTitle: string;
  centerName: string;
  centerProfileId?: string;
  hasCenterLogo?: boolean;
  categoryGroupTitle?: string | null;
  categoryTitle?: string | null;
  price: number;
  discountPercent?: number | null;
  finalPrice: number;
  isNegotiablePrice?: boolean;
  isUsed?: boolean;
  imagePath?: string | null;
  publishedAt?: string | null;
  acceptsResume?: boolean;
}

export interface GetPublishedListingsQuery {
  title?: string;
  categoryId?: number;
  categoryGroupId?: number;
  brandId?: number;
  brandIds?: number[];
  provinceId?: number;
  minPrice?: number;
  maxPrice?: number;
  attributeFilters?: PublicAttributeFilterItem[];
  page?: number;
  pageSize?: number;
  sortBy?: 'newest' | 'views' | 'discount' | 'price_asc' | 'price_desc';
}

export type ProductSortBy = NonNullable<GetPublishedListingsQuery['sortBy']>;

export interface PublicAttributeFilterItem {
  productAttributeId: number;
  value: string;
}

export interface PublicBrandFilterDto {
  brandId: number;
  title: string;
}

export interface PublicCategoryFilterDto {
  productCategoryId: number;
  title: string;
  productCategoryGroupId?: number | null;
  categoryGroupTitle?: string | null;
}

export interface PublicCategoryGroupFilterDto {
  productCategoryGroupId: number;
  title: string;
}

export interface PublicProvinceFilterDto {
  provinceId: number;
  name: string;
}

export interface PublicFilterAttributeDto {
  productAttributeId: number;
  title: string;
  productCategoryId: number;
  categoryTitle: string;
  fieldType?: number | null;
}

export interface PublicListingFiltersDto {
  brands: PublicBrandFilterDto[];
  categoryGroups?: PublicCategoryGroupFilterDto[];
  categories: PublicCategoryFilterDto[];
  provinces?: PublicProvinceFilterDto[];
  minPrice?: number | null;
  maxPrice?: number | null;
}

export interface PagedPublicProducts {
  items: PublicProductListingCardDto[];
  totalCount: number;
  page: number;
  pageSize: number;
}

export interface ProductReviewReplyDto {
  id: string;
  authorName: string;
  comment: string;
  createdAt: string;
}

export interface ProductUserReviewDto {
  id: string;
  authorName: string;
  rating: number;
  comment: string;
  createdAt: string;
  isApproved: boolean;
  productTitle: string;
  replies: ProductReviewReplyDto[];
}

export interface ProductReviewListItemDto {
  id: string;
  authorName: string;
  productTitle: string;
  rating: number;
  comment: string;
  createdAt: string;
  isApproved: boolean;
}

export interface ProductReviewEligibilityDto {
  canReview: boolean;
  hasPendingReview: boolean;
  orderItemId?: string | null;
}

export interface GetProductReviewsQuery {
  productId: string;
  page?: number;
  pageSize?: number;
}

export interface GetPendingProductReviewsQuery {
  page?: number;
  pageSize?: number;
}

export interface PagedProductReviews<T> {
  items: T[];
  totalCount: number;
  page: number;
  pageSize: number;
}

export interface PublicProductListingDetailDto {
  productId: string;
  title: string;
  brandTitle: string;
  centerName: string;
  centerProfileId: string;
  hasCenterLogo?: boolean;
  warranty: string;
  description: string;
  price: number;
  discountPercent?: number | null;
  finalPrice: number;
  isNegotiablePrice?: boolean;
  isUsed?: boolean;
  stockQuantity: number;
  publishedAt?: string | null;
  categoryTitles: string[];
  attributeValues: ProductAttributeValueDto[];
  images: ProductImageDto[];
  expertReviews: ProductExpertReviewDto[];
  userReviews: ProductUserReviewDto[];
  latitude?: number | null;
  longitude?: number | null;
  provinceId?: number | null;
  provinceName?: string | null;
  acceptsResume?: boolean;
}

export interface CreateProductReviewCommand {
  orderItemId: string;
  rating: number;
  comment: string;
}

export interface ReplyToReviewCommand {
  reviewId: string;
  comment: string;
}

export interface ProductSearchSuggestionDto {
  type: 'product' | 'category';
  title: string;
  categoryLabel?: string | null;
  productId?: string | null;
  categoryId?: number | null;
}

export interface SellerContactDto {
  phone: string;
  mobileNumber: string;
}

export type ProductResumeApplicationStatus =
  | 'Pending'
  | 'Approved'
  | 'Rejected'
  | 0
  | 1
  | 2;

export interface ProductResumeApplicationDto {
  productResumeApplicationId: string;
  productId: string;
  productTitle: string;
  status: ProductResumeApplicationStatus;
  reviewNotes: string;
  submittedAt: string;
  reviewedAt?: string | null;
}

export interface ProductResumeApplicationForOwnerDto {
  productResumeApplicationId: string;
  productId: string;
  productTitle: string;
  applicantUserId: string;
  applicantFirstName: string;
  applicantLastName: string;
  applicantMobileNumber: string;
  jobTitle: string;
  degreeLevel: DegreeLevel | null;
  status: ProductResumeApplicationStatus;
  reviewNotes: string;
  submittedAt: string;
  reviewedAt?: string | null;
}

export interface ReviewProductResumeApplicationCommand {
  productResumeApplicationId: string;
  approved: boolean;
  reviewNotes: string;
}

export interface GetProductResumeApplicationsQuery {
  productId: string;
  page?: number;
  pageSize?: number;
}

export interface ProductResumeApplicationsPageDto {
  productTitle: string;
  items: ProductResumeApplicationForOwnerDto[];
  totalCount: number;
  page: number;
  pageSize: number;
}

export interface PublicBrandCardDto {
  brandId: number;
  title: string;
  imagePath: string;
}

export interface PublicCategoryCardDto {
  productCategoryId: number;
  title: string;
}

export interface PublicCategoryGroupCardDto {
  productCategoryGroupId: number;
  title: string;
  homePageImagePath: string;
}
