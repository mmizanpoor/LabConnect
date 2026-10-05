import {
  AfterViewInit,
  Component,
  DestroyRef,
  ElementRef,
  Injector,
  OnInit,
  ViewChild,
  afterNextRender,
  inject,
} from '@angular/core';
import { NgStyle } from '@angular/common';
import { FormControl, ReactiveFormsModule } from '@angular/forms';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { ActivatedRoute, Router } from '@angular/router';
import { MatIconModule } from '@angular/material/icon';
import { TranslocoPipe } from '@jsverse/transloco';
import { LocalizationService } from '@core/services/localization/localization.service';
import { PortalLabAccessService } from '@core/services/site/portal-lab-access.service';
import { PublicProductsService } from './products.service';
import {
  ProductSortBy,
  PublicAttributeFilterItem,
  PublicCategoryFilterDto,
  PublicCategoryGroupFilterDto,
  PublicFilterAttributeDto,
  PublicListingFiltersDto,
  PublicProductListingCardDto,
  PublicProvinceFilterDto,
  PublicBrandFilterDto,
} from './products.types';
import { ProductCardComponent } from './product-card.component';

@Component({
  selector: 'app-products-list',
  standalone: true,
  imports: [
    NgStyle,
    ReactiveFormsModule,
    TranslocoPipe,
    MatIconModule,
    ProductCardComponent,
  ],
  templateUrl: './products-list.component.html',
  styleUrl: './products-list.component.scss',
})
export class ProductsListComponent implements OnInit, AfterViewInit {
  private _publicProductsService = inject(PublicProductsService);
  private _localization = inject(LocalizationService);
  private _portalLabAccess = inject(PortalLabAccessService);
  private _route = inject(ActivatedRoute);
  private _router = inject(Router);
  private _destroyRef = inject(DestroyRef);
  private _injector = inject(Injector);

  @ViewChild('loadMoreSentinel')
  private _loadMoreSentinel?: ElementRef<HTMLElement>;

  loading = false;
  loadingMore = false;
  filtersLoading = false;
  attributesLoading = false;
  error = '';
  products: PublicProductListingCardDto[] = [];
  filterOptions: PublicListingFiltersDto | null = null;
  filterAttributes: PublicFilterAttributeDto[] = [];
  totalCount = 0;
  page = 1;
  pageSize = 20;
  sortBy: ProductSortBy = 'newest';

  /** Applied filters (drive API + URL). */
  searchTitle = '';
  provinceId?: number;
  categoryGroupId?: number;
  categoryId?: number;
  brandId?: number;
  brandIds: number[] = [];
  minPrice?: number;
  maxPrice?: number;
  attributeFilters: PublicAttributeFilterItem[] = [];

  /** Draft filters (edited until Apply). */
  searchInput = new FormControl('', { nonNullable: true });
  draftProvinceId: number | null = null;
  draftCategoryGroupId: number | null = null;
  draftCategoryId: number | null = null;
  draftMinPrice: number | null = null;
  draftMaxPrice: number | null = null;
  draftAttributeValues: Record<number, string> = {};
  draftBrandIds: number[] = [];
  catalogMinPrice = 0;
  catalogMaxPrice = 0;
  brandSearchInput = new FormControl('', { nonNullable: true });
  expandedSections = {
    price: false,
    brand: false,
    province: false,
    categoryGroup: false,
    category: false,
    attributes: false,
  };

  private _infiniteScrollObserver?: IntersectionObserver;
  private readonly _scrollPrefetchRows = 5;
  private readonly _estimatedProductRowHeightPx = 410;

  readonly sortOptions: { value: ProductSortBy; labelKey: string }[] = [
    { value: 'discount', labelKey: 'modules.products.sort.mostDiscount' },
    { value: 'views', labelKey: 'modules.products.sort.mostViewed' },
    { value: 'newest', labelKey: 'modules.products.sort.newest' },
    { value: 'price_asc', labelKey: 'modules.products.sort.cheapest' },
    { value: 'price_desc', labelKey: 'modules.products.sort.mostExpensive' },
  ];

  ngOnInit(): void {
    void this._portalLabAccess.ensure();
    void this.loadFilterOptions();

    this._route.queryParamMap
      .pipe(takeUntilDestroyed(this._destroyRef))
      .subscribe((params) => {
        this.searchTitle = params.get('title')?.trim() ?? '';
        this.searchInput.setValue(this.searchTitle, { emitEvent: false });

        const provinceParam = params.get('provinceId');
        this.provinceId = provinceParam ? Number(provinceParam) : undefined;
        this.draftProvinceId = this.provinceId ?? null;

        const categoryGroupParam = params.get('categoryGroupId');
        const parsedCategoryGroupId = categoryGroupParam
          ? Number(categoryGroupParam)
          : undefined;
        this.categoryGroupId =
          parsedCategoryGroupId && Number.isFinite(parsedCategoryGroupId)
            ? parsedCategoryGroupId
            : undefined;
        this.draftCategoryGroupId = this.categoryGroupId ?? null;
        if (this.categoryGroupId) {
          this.expandedSections.categoryGroup = true;
        }

        const categoryParam = params.get('categoryId');
        const parsedCategoryId = categoryParam
          ? Number(categoryParam)
          : undefined;
        this.categoryId =
          parsedCategoryId && Number.isFinite(parsedCategoryId)
            ? parsedCategoryId
            : undefined;
        this.draftCategoryId = this.categoryId ?? null;
        if (this.categoryId) {
          this.expandedSections.category = true;
        }

        const brandParam = params.get('brandId');
        const brandIdsParam = params.get('brandIds');
        this.brandIds = this.parseIdCsv(brandIdsParam);
        const singleBrandId = brandParam ? Number(brandParam) : undefined;
        this.brandId =
          singleBrandId && Number.isFinite(singleBrandId)
            ? singleBrandId
            : undefined;
        if (this.brandId && !this.brandIds.includes(this.brandId)) {
          this.brandIds = [...this.brandIds, this.brandId];
        }
        this.draftBrandIds = [...this.brandIds];
        if (this.brandIds.length) {
          this.expandedSections.brand = true;
        }

        const minParam = params.get('minPrice');
        this.minPrice = minParam ? Number(minParam) : undefined;
        const maxParam = params.get('maxPrice');
        this.maxPrice = maxParam ? Number(maxParam) : undefined;
        this.syncDraftPricesFromApplied();

        this.attributeFilters = this.parseAttributeFilters(params.get('attrs'));
        this.draftAttributeValues = Object.fromEntries(
          this.attributeFilters.map((item) => [
            item.productAttributeId,
            item.value,
          ])
        );

        const sortParam = params.get('sortBy');
        this.sortBy = this.isValidSort(sortParam) ? sortParam : 'newest';

        void this.syncAttributesForDraftGroup();
        this.page = 1;
        void this.load();
      });

    this._destroyRef.onDestroy(() =>
      this._infiniteScrollObserver?.disconnect()
    );
  }

  ngAfterViewInit(): void {
    this._infiniteScrollObserver = new IntersectionObserver(
      (entries) => {
        if (entries.some((entry) => entry.isIntersecting)) void this.loadMore();
      },
      {
        root: null,
        rootMargin: `0px 0px ${
          this._scrollPrefetchRows * this._estimatedProductRowHeightPx
        }px 0px`,
        threshold: 0,
      }
    );
    this.observeLoadMoreSentinel();
  }

  get hasMoreProducts(): boolean {
    return this.products.length < this.totalCount;
  }

  get provinces(): PublicProvinceFilterDto[] {
    return this.filterOptions?.provinces ?? [];
  }

  get categoryGroups(): PublicCategoryGroupFilterDto[] {
    return this.filterOptions?.categoryGroups ?? [];
  }

  get brands(): PublicBrandFilterDto[] {
    return this.filterOptions?.brands ?? [];
  }

  get filteredBrands(): PublicBrandFilterDto[] {
    const term = this.brandSearchInput.value.trim();
    const brands = term
      ? this.brands.filter((brand) => brand.title.includes(term))
      : this.brands;
    if (!this.draftBrandIds.length) return brands;
    const selected = new Set(this.draftBrandIds);
    return [...brands].sort(
      (a, b) =>
        Number(selected.has(b.brandId)) - Number(selected.has(a.brandId))
    );
  }

  get subCategories(): PublicCategoryFilterDto[] {
    if (!this.draftCategoryGroupId) return [];
    return (this.filterOptions?.categories ?? []).filter(
      (category) =>
        category.productCategoryGroupId === this.draftCategoryGroupId
    );
  }

  get hasActiveFilters(): boolean {
    return !!(
      this.searchTitle ||
      this.provinceId ||
      this.categoryGroupId ||
      this.categoryId ||
      this.brandId ||
      this.brandIds.length ||
      this.minPrice != null ||
      this.maxPrice != null ||
      this.attributeFilters.length
    );
  }

  async loadFilterOptions(): Promise<void> {
    this.filtersLoading = true;
    try {
      const result = await this._publicProductsService.getListingFilters();
      if (result.success && result.data) {
        this.filterOptions = result.data;
        this.initCatalogPriceBounds();
        this.syncDraftPricesFromApplied();
      }
    } catch {
      // Filters sidebar is optional.
    } finally {
      this.filtersLoading = false;
      if (!this.hasCatalogPriceBounds()) {
        this.catalogMinPrice = 0;
        this.catalogMaxPrice = 100_000_000;
        this.syncDraftPricesFromApplied();
      }
    }
  }

  async onCategoryGroupDraftChange(rawValue: string): Promise<void> {
    this.draftCategoryGroupId = rawValue ? Number(rawValue) : null;
    this.draftCategoryId = null;
    this.draftAttributeValues = {};
    this.filterAttributes = [];
    await this.syncAttributesForDraftGroup();
  }

  onProvinceDraftChange(rawValue: string): void {
    this.draftProvinceId = rawValue ? Number(rawValue) : null;
  }

  async onCategorySubDraftChange(rawValue: string): Promise<void> {
    this.draftCategoryId = rawValue ? Number(rawValue) : null;
    this.draftAttributeValues = {};
    await this.syncAttributesForDraftGroup();
  }

  onBrandDraftChange(brandId: number, checked: boolean): void {
    this.draftBrandIds = checked
      ? [...this.draftBrandIds, brandId]
      : this.draftBrandIds.filter((id) => id !== brandId);
  }

  isBrandSelected(brandId: number): boolean {
    return this.draftBrandIds.includes(brandId);
  }

  toggleSection(
    section:
      | 'price'
      | 'brand'
      | 'province'
      | 'categoryGroup'
      | 'category'
      | 'attributes'
  ): void {
    this.expandedSections[section] = !this.expandedSections[section];
  }

  clearBrandSearch(): void {
    this.brandSearchInput.setValue('');
  }

  onAttributeDraftChange(attributeId: number, value: string): void {
    const trimmed = value.trim();
    if (!trimmed) {
      delete this.draftAttributeValues[attributeId];
      return;
    }
    this.draftAttributeValues[attributeId] = trimmed;
  }

  get sliderDraftMin(): number {
    return this.draftMinPrice ?? this.catalogMinPrice;
  }

  get sliderDraftMax(): number {
    return this.draftMaxPrice ?? this.catalogMaxPrice;
  }

  get priceSliderStep(): number {
    const span = this.catalogMaxPrice - this.catalogMinPrice;
    if (span <= 0) return 1;
    return Math.max(1, Math.round(span / 200));
  }

  get priceSliderFillStyle(): Record<string, string> {
    const span = this.catalogMaxPrice - this.catalogMinPrice;
    if (span <= 0) return { left: '0%', right: '0%' };
    const min = this.sliderDraftMin;
    const max = this.sliderDraftMax;
    return {
      right: `${((min - this.catalogMinPrice) / span) * 100}%`,
      left: `${((this.catalogMaxPrice - max) / span) * 100}%`,
    };
  }

  onDraftMinPriceChange(rawValue: string): void {
    const parsed = this.parsePriceInput(rawValue);
    if (parsed == null) {
      this.draftMinPrice = null;
      return;
    }
    this.draftMinPrice = this.clampPrice(parsed);
    if (this.draftMaxPrice != null && this.draftMinPrice > this.draftMaxPrice) {
      this.draftMaxPrice = this.draftMinPrice;
    }
  }

  onDraftMaxPriceChange(rawValue: string): void {
    const parsed = this.parsePriceInput(rawValue);
    if (parsed == null) {
      this.draftMaxPrice = null;
      return;
    }
    this.draftMaxPrice = this.clampPrice(parsed);
    if (this.draftMinPrice != null && this.draftMaxPrice < this.draftMinPrice) {
      this.draftMinPrice = this.draftMaxPrice;
    }
  }

  onSliderMinChange(rawValue: string): void {
    const value = this.clampPrice(Number(rawValue));
    this.draftMinPrice = Math.min(value, this.sliderDraftMax);
  }

  onSliderMaxChange(rawValue: string): void {
    const value = this.clampPrice(Number(rawValue));
    this.draftMaxPrice = Math.max(value, this.sliderDraftMin);
  }

  formatDraftPrice(value: number | null): string {
    if (value == null) return '';
    return new Intl.NumberFormat('fa-IR', {
      useGrouping: true,
      maximumFractionDigits: 0,
    }).format(value);
  }

  hasCatalogPriceBounds(): boolean {
    return this.catalogMaxPrice > this.catalogMinPrice;
  }

  applyFilters(): void {
    const attributeFilters = Object.entries(this.draftAttributeValues)
      .map(([id, value]) => ({
        productAttributeId: Number(id),
        value: value.trim(),
      }))
      .filter((item) => item.productAttributeId > 0 && item.value);

    const { minPrice, maxPrice } = this.normalizeDraftPricesForApply();

    void this._router.navigate([], {
      relativeTo: this._route,
      queryParams: {
        title: this.searchInput.value.trim() || null,
        provinceId: this.draftProvinceId || null,
        categoryGroupId: this.draftCategoryGroupId || null,
        categoryId: this.draftCategoryId || null,
        categoryGroupTitle: null,
        categoryTitle: null,
        brandId: this.draftBrandIds.length === 1 ? this.draftBrandIds[0] : null,
        brandIds: this.serializeIds(this.draftBrandIds),
        brandTitle: null,
        attrs: this.serializeAttributeFilters(attributeFilters),
        minPrice: minPrice ?? null,
        maxPrice: maxPrice ?? null,
      },
      queryParamsHandling: 'merge',
    });
  }

  clearAllFilters(): void {
    this.searchInput.setValue('');
    this.draftProvinceId = null;
    this.draftCategoryGroupId = null;
    this.draftCategoryId = null;
    this.draftAttributeValues = {};
    this.filterAttributes = [];
    this.draftBrandIds = [];
    this.brandSearchInput.setValue('');
    this.draftMinPrice = this.hasCatalogPriceBounds()
      ? this.catalogMinPrice
      : null;
    this.draftMaxPrice = this.hasCatalogPriceBounds()
      ? this.catalogMaxPrice
      : null;

    void this._router.navigate([], {
      relativeTo: this._route,
      queryParams: {
        title: null,
        provinceId: null,
        categoryGroupId: null,
        categoryId: null,
        categoryGroupTitle: null,
        categoryTitle: null,
        brandId: null,
        brandIds: null,
        brandTitle: null,
        attrs: null,
        minPrice: null,
        maxPrice: null,
      },
      queryParamsHandling: 'merge',
    });
  }

  onSortChange(sort: ProductSortBy): void {
    void this._router.navigate([], {
      relativeTo: this._route,
      queryParams: { sortBy: sort === 'newest' ? null : sort },
      queryParamsHandling: 'merge',
    });
  }

  async load(): Promise<void> {
    this.page = 1;
    await this.fetchProducts(true);
  }

  async loadMore(): Promise<void> {
    if (this.loading || this.loadingMore || !this.hasMoreProducts) return;
    this.page += 1;
    await this.fetchProducts(false);
  }

  private async syncAttributesForDraftGroup(): Promise<void> {
    if (!this.draftCategoryGroupId) {
      this.filterAttributes = [];
      return;
    }

    this.attributesLoading = true;
    try {
      const result = await this._publicProductsService.getFilterAttributes(
        this.draftCategoryGroupId,
        this.draftCategoryId ?? undefined
      );
      this.filterAttributes = this.sanitizeFilterAttributes(
        result.success && result.data ? result.data : []
      );
    } catch {
      this.filterAttributes = [];
    } finally {
      this.attributesLoading = false;
    }
  }

  private async fetchProducts(reset: boolean): Promise<void> {
    if (reset) {
      this.loading = true;
      this.error = '';
    } else this.loadingMore = true;

    try {
      const result = await this._publicProductsService.getPublishedListings({
        title: this.searchTitle || undefined,
        provinceId: this.provinceId,
        categoryId: this.categoryId,
        categoryGroupId: this.categoryGroupId,
        brandId: this.brandId,
        brandIds: this.brandIds.length ? this.brandIds : undefined,
        minPrice: this.minPrice,
        maxPrice: this.maxPrice,
        attributeFilters: this.attributeFilters.length
          ? this.attributeFilters
          : undefined,
        page: this.page,
        pageSize: this.pageSize,
        sortBy: this.sortBy,
      });
      if (!result.success || !result.data) {
        throw new Error(
          result.message ??
            this._localization.translate('modules.products.errors.loadFailed')
        );
      }

      if (reset) {
        this.products = result.data.items;
      } else {
        const existingIds = new Set(
          this.products.map((product) => product.productId)
        );
        const newItems = result.data.items.filter(
          (product) => !existingIds.has(product.productId)
        );
        this.products = [...this.products, ...newItems];
      }

      this.totalCount = result.data.totalCount;
    } catch (e: unknown) {
      if (reset) {
        this.error =
          e instanceof Error
            ? e.message
            : this._localization.translate(
                'modules.products.errors.loadFailed'
              );
      } else if (this.page > 1) this.page -= 1;
    } finally {
      this.loading = false;
      this.loadingMore = false;
      this.observeLoadMoreSentinel();
    }
  }

  private serializeAttributeFilters(
    items: PublicAttributeFilterItem[]
  ): string | null {
    if (!items.length) return null;
    return items
      .map(
        (item) => `${item.productAttributeId}~${encodeURIComponent(item.value)}`
      )
      .join('|');
  }

  private parseAttributeFilters(
    raw: string | null
  ): PublicAttributeFilterItem[] {
    if (!raw?.trim()) return [];
    return raw
      .split('|')
      .map((part) => {
        const [idPart, ...valueParts] = part.split('~');
        const productAttributeId = Number(idPart);
        const value = decodeURIComponent(valueParts.join('~')).trim();
        return { productAttributeId, value };
      })
      .filter((item) => item.productAttributeId > 0 && item.value);
  }

  private initCatalogPriceBounds(): void {
    const min = this.filterOptions?.minPrice;
    const max = this.filterOptions?.maxPrice;
    if (min == null || max == null || max <= min) {
      this.catalogMinPrice = 0;
      this.catalogMaxPrice = 100_000_000;
      return;
    }
    this.catalogMinPrice = Math.floor(min);
    this.catalogMaxPrice = Math.ceil(max);
  }

  private clampPrice(value: number): number {
    if (!Number.isFinite(value)) return this.catalogMinPrice;
    if (!this.hasCatalogPriceBounds()) return Math.max(0, Math.round(value));
    return Math.min(
      this.catalogMaxPrice,
      Math.max(this.catalogMinPrice, Math.round(value))
    );
  }

  private syncDraftPricesFromApplied(): void {
    if (!this.hasCatalogPriceBounds()) {
      this.draftMinPrice = this.minPrice ?? null;
      this.draftMaxPrice = this.maxPrice ?? null;
      return;
    }
    this.draftMinPrice = this.minPrice ?? this.catalogMinPrice;
    this.draftMaxPrice = this.maxPrice ?? this.catalogMaxPrice;
  }

  private normalizeDraftPricesForApply(): {
    minPrice?: number;
    maxPrice?: number;
  } {
    let min = this.draftMinPrice;
    let max = this.draftMaxPrice;

    if (min != null && max != null && min > max) {
      [min, max] = [max, min];
      this.draftMinPrice = min;
      this.draftMaxPrice = max;
    }

    const useMin =
      min != null &&
      (!this.hasCatalogPriceBounds() || min > this.catalogMinPrice)
        ? min
        : undefined;
    const useMax =
      max != null &&
      (!this.hasCatalogPriceBounds() || max < this.catalogMaxPrice)
        ? max
        : undefined;

    return { minPrice: useMin, maxPrice: useMax };
  }

  private parsePriceInput(rawValue: string): number | null {
    const normalized = rawValue
      .replace(/[۰-۹]/g, (digit) =>
        String(digit.charCodeAt(0) - '۰'.charCodeAt(0))
      )
      .replace(/[٠-٩]/g, (digit) =>
        String(digit.charCodeAt(0) - '٠'.charCodeAt(0))
      )
      .replace(/[^\d]/g, '');
    if (!normalized) return null;
    const value = Number(normalized);
    return Number.isFinite(value) ? value : null;
  }

  private sanitizeFilterAttributes(
    items: PublicFilterAttributeDto[]
  ): PublicFilterAttributeDto[] {
    const seen = new Set<string>();
    const result: PublicFilterAttributeDto[] = [];

    for (const item of items) {
      const key = this.normalizeAttributeTitle(item.title);
      if (!key || key === 'استان' || key === 'برند' || seen.has(key)) continue;
      seen.add(key);
      result.push(item);
    }

    return result;
  }

  private normalizeAttributeTitle(title: string | null | undefined): string {
    return (title ?? '').trim().replace(/\s+/g, ' ');
  }

  private parseIdCsv(raw: string | null): number[] {
    if (!raw?.trim()) return [];
    return raw
      .split(',')
      .map((part) => Number(part.trim()))
      .filter((id) => Number.isFinite(id) && id > 0);
  }

  private serializeIds(ids: number[]): string | null {
    const unique = [...new Set(ids.filter((id) => id > 0))];
    return unique.length ? unique.join(',') : null;
  }

  private isValidSort(value: string | null): value is ProductSortBy {
    return (
      value === 'newest' ||
      value === 'views' ||
      value === 'discount' ||
      value === 'price_asc' ||
      value === 'price_desc'
    );
  }

  private observeLoadMoreSentinel(): void {
    afterNextRender(
      () => {
        this._infiniteScrollObserver?.disconnect();
        const el = this._loadMoreSentinel?.nativeElement;
        if (el && this.hasMoreProducts)
          this._infiniteScrollObserver?.observe(el);
      },
      { injector: this._injector }
    );
  }
}
