import { Injectable } from '@angular/core';
import { ApiHttpService } from '@core/services/http/api-http.service';
import { environment } from '@env/environment';
import {
  BrandDto,
  DeleteProductImageCommand,
  GetAttributesByCategoryIdsQuery,
  GetBrandsQuery,
  GetProductAttributesQuery,
  GetProductCategoriesQuery,
  GetProductCategoryGroupsQuery,
  GetProductsQuery,
  PagedResult,
  ProductAttributeDto,
  ProductCategoryDto,
  ProductCategoryGroupDto,
  ProductCatalogOptionDto,
  ProductDto,
  ProductImageDto,
  ProductListItemDto,
  ProductProvinceDto,
  RejectProductCommand,
  SearchCatalogQuery,
  SaveBrandCommand,
  SaveProductAttributeCommand,
  SaveProductCategoryCommand,
  SaveProductCategoryGroupCommand,
  SaveProductCommand,
  UpdateBrandCommand,
  UpdateProductAttributeCommand,
  UpdateProductCategoryCommand,
  UpdateProductCategoryGroupCommand,
  UpdateProductCommand,
} from './product-catalog.types';

@Injectable({ providedIn: 'root' })
export class ProductCatalogService {
  constructor(private _http: ApiHttpService) {}

  getCategories(query: GetProductCategoriesQuery) {
    return this._http.post<PagedResult<ProductCategoryDto>>('ProductCategory', 'GetAll', query);
  }

  getCategoryOptions() {
    return this._http.get<ProductCategoryDto[]>('ProductCategory', 'GetAllOptions');
  }

  createCategory(command: SaveProductCategoryCommand) {
    return this._http.post<ProductCategoryDto>('ProductCategory', 'Create', command);
  }

  updateCategory(command: UpdateProductCategoryCommand) {
    return this._http.post<ProductCategoryDto>('ProductCategory', 'Update', command);
  }

  deleteCategory(productCategoryId: number) {
    return this._http.delete('ProductCategory', 'Delete', { productCategoryId });
  }

  getCategoryGroups(query: GetProductCategoryGroupsQuery) {
    return this._http.post<PagedResult<ProductCategoryGroupDto>>('ProductCategoryGroup', 'GetAll', query);
  }

  getCategoryGroupOptions() {
    return this._http.get<ProductCategoryGroupDto[]>('ProductCategoryGroup', 'GetAllOptions');
  }

  createCategoryGroup(command: SaveProductCategoryGroupCommand) {
    return this._http.post<ProductCategoryGroupDto>('ProductCategoryGroup', 'Create', command);
  }

  updateCategoryGroup(command: UpdateProductCategoryGroupCommand) {
    return this._http.post<ProductCategoryGroupDto>('ProductCategoryGroup', 'Update', command);
  }

  deleteCategoryGroup(productCategoryGroupId: number) {
    return this._http.delete('ProductCategoryGroup', 'Delete', { productCategoryGroupId });
  }

  uploadCategoryGroupHomePageImage(productCategoryGroupId: number, file: File) {
    const formData = new FormData();
    formData.append('file', file);
    return this._http.postForm<ProductCategoryGroupDto>(
      'ProductCategoryGroup',
      'UploadHomePageImage',
      formData,
      { productCategoryGroupId },
    );
  }

  deleteCategoryGroupHomePageImage(productCategoryGroupId: number) {
    return this._http.post<ProductCategoryGroupDto>(
      'ProductCategoryGroup',
      'DeleteHomePageImage',
      {},
      { productCategoryGroupId },
    );
  }

  getCategoryGroupHomePageImageUrl(imagePath: string): string {
    return `${environment.apiUrl}ProductCategoryGroup/GetHomePageImage?path=${encodeURIComponent(imagePath)}`;
  }

  getAttributes(query: GetProductAttributesQuery) {
    return this._http.post<PagedResult<ProductAttributeDto>>('ProductAttribute', 'GetAll', query);
  }

  getAttributesByCategoryIds(query: GetAttributesByCategoryIdsQuery) {
    return this._http.post<ProductAttributeDto[]>('ProductAttribute', 'GetByCategoryIds', query);
  }

  createAttribute(command: SaveProductAttributeCommand) {
    return this._http.post<ProductAttributeDto>('ProductAttribute', 'Create', command);
  }

  updateAttribute(command: UpdateProductAttributeCommand) {
    return this._http.post<ProductAttributeDto>('ProductAttribute', 'Update', command);
  }

  deleteAttribute(productAttributeId: number) {
    return this._http.delete('ProductAttribute', 'Delete', { productAttributeId });
  }

  getBrands(query: GetBrandsQuery) {
    return this._http.post<PagedResult<BrandDto>>('Brand', 'GetAll', query);
  }

  getBrandOptions() {
    return this._http.get<BrandDto[]>('Brand', 'GetAllOptions');
  }

  createBrand(command: SaveBrandCommand) {
    return this._http.post<BrandDto>('Brand', 'Create', command);
  }

  updateBrand(command: UpdateBrandCommand) {
    return this._http.post<BrandDto>('Brand', 'Update', command);
  }

  deleteBrand(brandId: number) {
    return this._http.delete('Brand', 'Delete', { brandId });
  }

  uploadBrandImage(brandId: number, file: File) {
    const formData = new FormData();
    formData.append('file', file);
    return this._http.postForm<BrandDto>('Brand', 'UploadImage', formData, { brandId });
  }

  deleteBrandImage(brandId: number) {
    return this._http.post<BrandDto>('Brand', 'DeleteImage', {}, { brandId });
  }

  getBrandImageUrl(imagePath: string): string {
    return `${environment.apiUrl}Brand/GetImage?path=${encodeURIComponent(imagePath)}`;
  }

  getProducts(query: GetProductsQuery) {
    return this._http.post<PagedResult<ProductListItemDto>>('Product', 'GetAll', query);
  }

  /** Counts current center listings by status (uses existing GetAll totals). */
  async getMyProductDashboardStats(options?: { acceptsResume?: boolean }): Promise<{
    publishedCount: number;
    pendingCount: number;
    expiredCount: number;
    rejectedCount: number;
  }> {
    const acceptsResume = options?.acceptsResume;
    const base = {
      mineOnly: true as const,
      page: 1,
      pageSize: 1,
      ...(acceptsResume === undefined ? {} : { acceptsResume }),
    };
    const [published, pending, expired, rejected] = await Promise.all([
      this.getProducts({ ...base, status: 'Approved' }),
      this.getProducts({ ...base, status: 'PendingApproval' }),
      this.getProducts({ ...base, status: 'Unpublished' }),
      this.getProducts({ ...base, status: 'Rejected' }),
    ]);

    return {
      publishedCount: published.success ? (published.data?.totalCount ?? 0) : 0,
      pendingCount: pending.success ? (pending.data?.totalCount ?? 0) : 0,
      expiredCount: expired.success ? (expired.data?.totalCount ?? 0) : 0,
      rejectedCount: rejected.success ? (rejected.data?.totalCount ?? 0) : 0,
    };
  }

  searchCatalog(query: SearchCatalogQuery) {
    return this._http.post<PagedResult<ProductCatalogOptionDto>>('Product', 'SearchCatalog', query);
  }

  getProductById(productId: string) {
    return this._http.get<ProductDto>('Product', 'GetById', { productId });
  }

  resolveProvinceFromLocation(latitude: number, longitude: number) {
    return this._http.get<ProductProvinceDto>('Product', 'ResolveProvinceFromLocation', {
      latitude,
      longitude,
    });
  }

  createProduct(command: SaveProductCommand) {
    return this._http.post<ProductDto>('Product', 'Create', command);
  }

  updateProduct(command: UpdateProductCommand) {
    return this._http.post<ProductDto>('Product', 'Update', command);
  }

  deleteProduct(productId: string) {
    return this._http.delete('Product', 'Delete', { productId });
  }

  uploadImage(productId: string, file: File) {
    const formData = new FormData();
    formData.append('file', file);
    return this._http.postForm<ProductImageDto>('Product', 'UploadImage', formData, { productId });
  }

  deleteImage(command: DeleteProductImageCommand) {
    return this._http.post('Product', 'DeleteImage', command);
  }

  submitForApproval(productId: string) {
    return this._http.post<ProductDto>('Product', 'SubmitForApproval', { productId });
  }

  unpublishProduct(productId: string) {
    return this._http.post<ProductDto>('Product', 'Unpublish', { productId });
  }

  republishProduct(productId: string) {
    return this._http.post<ProductDto>('Product', 'Republish', { productId });
  }

  approveProduct(productId: string) {
    return this._http.post<ProductDto>('Product', 'Approve', { productId });
  }

  rejectProduct(command: RejectProductCommand) {
    return this._http.post<ProductDto>('Product', 'Reject', command);
  }

  uploadFeaturedImage(productId: string, file: File) {
    const formData = new FormData();
    formData.append('file', file);
    return this._http.postForm<ProductDto>('Product', 'UploadFeaturedImage', formData, { productId });
  }

  deleteFeaturedImage(productId: string) {
    return this._http.post<ProductDto>('Product', 'DeleteFeaturedImage', {}, { productId });
  }

  getProductImageUrl(imagePath: string): string {
    return `${environment.apiUrl}Product/GetImage?path=${encodeURIComponent(imagePath)}`;
  }
}
