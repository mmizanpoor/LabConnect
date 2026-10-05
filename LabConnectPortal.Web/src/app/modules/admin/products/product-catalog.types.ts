export interface PagedResult<T> {
  items: T[];
  totalCount: number;
  page: number;
  pageSize: number;
}

export interface ProductCategoryDto {
  productCategoryId: number;
  title: string;
  acceptsResume?: boolean;
  productCategoryGroupId?: number | null;
}

export interface ProductCategoryGroupItemDto {
  productCategoryId: number;
  title: string;
}

export interface ProductCategoryGroupDto {
  productCategoryGroupId: number;
  name: string;
  homePageImagePath?: string | null;
  showOnHomePage?: boolean;
  hasHomePageImage?: boolean;
  categoryCount: number;
  categories: ProductCategoryGroupItemDto[];
}

export interface SaveProductCategoryGroupCommand {
  name: string;
  showOnHomePage?: boolean;
  productCategoryIds: number[];
}

export interface UpdateProductCategoryGroupCommand extends SaveProductCategoryGroupCommand {
  productCategoryGroupId: number;
}

export interface GetProductCategoryGroupsQuery {
  name?: string;
  page?: number;
  pageSize?: number;
}

export interface SaveProductCategoryCommand {
  title: string;
  acceptsResume?: boolean;
}

export interface UpdateProductCategoryCommand extends SaveProductCategoryCommand {
  productCategoryId: number;
}

export interface GetProductCategoriesQuery {
  title?: string;
  page?: number;
  pageSize?: number;
}

export interface ProductAttributeDto {
  productAttributeId: number;
  productCategoryId: number;
  categoryTitle: string;
  title: string;
  fieldType?: AttributeFieldType | null;
}

/** Null/undefined = free-text attribute. */
export type AttributeFieldType = 'Date' | 'ExpiryDate';

export function isDateAttributeFieldType(
  fieldType: AttributeFieldType | null | undefined | string | number,
): boolean {
  return (
    fieldType === 'Date' ||
    fieldType === 'ExpiryDate' ||
    fieldType === 1 ||
    fieldType === 2 ||
    fieldType === '1' ||
    fieldType === '2'
  );
}

export function isExpiryDateAttributeFieldType(
  fieldType: AttributeFieldType | null | undefined | string | number,
): boolean {
  return fieldType === 'ExpiryDate' || fieldType === 2 || fieldType === '2';
}

export interface SaveProductAttributeCommand {
  productCategoryId: number;
  title: string;
  fieldType?: AttributeFieldType | null;
}

export interface UpdateProductAttributeCommand extends SaveProductAttributeCommand {
  productAttributeId: number;
}

export interface GetProductAttributesQuery {
  productCategoryId?: number;
  title?: string;
  page?: number;
  pageSize?: number;
}

export interface GetAttributesByCategoryIdsQuery {
  categoryIds: number[];
}

export interface BrandDto {
  brandId: number;
  title: string;
  imagePath?: string | null;
  hasImage?: boolean;
  showOnHomePage?: boolean;
}

export interface SaveBrandCommand {
  title: string;
  showOnHomePage?: boolean;
}

export interface UpdateBrandCommand extends SaveBrandCommand {
  brandId: number;
}

export interface GetBrandsQuery {
  title?: string;
  page?: number;
  pageSize?: number;
}

export interface ProductAttributeValueDto {
  productAttributeId: number;
  attributeTitle: string;
  productCategoryId: number;
  categoryTitle: string;
  value: string;
  fieldType?: AttributeFieldType | null;
}

export interface SaveProductAttributeValueCommand {
  productAttributeId: number;
  value: string;
}

export interface ProductExpertReviewDto {
  id?: string;
  title: string;
  description: string;
  sortOrder: number;
}

export interface ProductImageDto {
  id: string;
  imagePath: string;
  sortOrder: number;
}

export interface ProductListItemDto {
  productId: string;
  title: string;
  brandTitle: string;
  categoryTitles: string[];
  status: ProductStatus;
  featuredImagePath?: string | null;
  hasFeaturedImage?: boolean;
  price?: number;
  isNegotiablePrice?: boolean;
  stockQuantity?: number;
  discountPercent?: number | null;
  createdByUserId?: string | null;
  createdByUserName?: string;
  createdByCenterProfileId?: string | null;
  createdByCenterName?: string;
  /** True when defined by site Administrator/Admin (not shop/lab). */
  createdBySiteAdmin?: boolean;
  acceptsResume?: boolean;
  createdAt?: string;
  updatedAt: string;
}

export type ProductStatus =
  | 'Draft'
  | 'PendingApproval'
  | 'Approved'
  | 'Rejected'
  | 'Unpublished'
  | 0
  | 1
  | 2
  | 3
  | 4;

export interface ProductDto {
  productId: string;
  title: string;
  brandId?: number | null;
  brandTitle: string;
  warranty: string;
  description: string;
  productCategoryGroupId?: number | null;
  productCategoryGroupName?: string | null;
  categoryIds: number[];
  categoryTitles: string[];
  attributeValues: ProductAttributeValueDto[];
  images: ProductImageDto[];
  expertReviews: ProductExpertReviewDto[];
  status: ProductStatus;
  featuredImagePath?: string | null;
  hasFeaturedImage?: boolean;
  rejectionReason?: string | null;
  createdByCenterProfileId?: string | null;
  createdByCenterName?: string;
  createdByUserId?: string | null;
  createdByUserName?: string;
  createdBySiteAdmin?: boolean;
  price?: number;
  isNegotiablePrice?: boolean;
  isUsed?: boolean;
  stockQuantity?: number;
  discountPercent?: number | null;
  latitude?: number | null;
  longitude?: number | null;
  provinceId?: number | null;
  provinceName?: string | null;
  submittedAt?: string | null;
  approvedAt?: string | null;
  createdAt: string;
  updatedAt: string;
}

export interface SaveProductCommand {
  title: string;
  brandId?: number | null;
  warranty: string;
  description: string;
  productCategoryGroupId: number;
  createdByCenterProfileId?: string | null;
  price: number;
  isNegotiablePrice: boolean;
  isUsed: boolean;
  stockQuantity: number;
  discountPercent?: number | null;
  latitude?: number | null;
  longitude?: number | null;
  categoryIds: number[];
  attributeValues: SaveProductAttributeValueCommand[];
  expertReviews: ProductExpertReviewDto[];
}

export interface UpdateProductCommand extends SaveProductCommand {
  productId: string;
}

export interface ProductProvinceDto {
  provinceId: number;
  provinceName: string;
}

export interface GetProductsQuery {
  title?: string;
  createdByUserName?: string;
  createdByCenterProfileId?: string;
  createdByCenterName?: string;
  brandId?: number;
  categoryId?: number;
  status?: ProductStatus;
  pendingApprovalOnly?: boolean;
  mineOnly?: boolean;
  /** true = resume categories only; false = exclude resume categories */
  acceptsResume?: boolean;
  updatedFrom?: string;
  updatedTo?: string;
  page?: number;
  pageSize?: number;
}

export interface SearchCatalogQuery {
  title?: string;
  page?: number;
  pageSize?: number;
}

export interface ProductCatalogOptionDto {
  productId: string;
  title: string;
  brandTitle: string;
}

export interface ProductIdCommand {
  productId: string;
}

export interface RejectProductCommand {
  productId: string;
  rejectionReason?: string;
}

export interface DeleteProductImageCommand {
  productId: string;
  imageId: string;
}
