import { Component, OnDestroy, OnInit, inject } from '@angular/core';
import { TranslocoPipe } from '@jsverse/transloco';
import { MatIconModule } from '@angular/material/icon';
import { PublicSiteService } from '@core/services/site/public-site.service';
import { SiteContextService } from '@core/services/site/site-context.service';
import { PortalLabAccessService } from '@core/services/site/portal-lab-access.service';
import { SiteSettingsPublicDto } from '@modules/admin/settings/site-settings.types';
import { ActiveSliderSlideDto } from '@modules/admin/slider-groups/slider-groups.types';
import { PublicPostCardDto } from '../admin/content/content.types';
import { PublicAdvertisementCardDto } from '../admin/advertisements/advertisements.types';
import { PublicAdvertisementsService } from '../admin/advertisements/advertisements.service';
import { PostCardComponent } from '../posts/post-card.component';
import { PublicPostsService } from '../posts/public-posts.service';
import { PublicBrandsService } from '../products/public-brands.service';
import { PublicCategoriesService } from '../products/public-categories.service';
import { PublicProductsService } from '../products/products.service';
import {
  PublicBrandCardDto,
  PublicCategoryGroupCardDto,
  PublicCategoryGroupFilterDto,
  PublicProductListingCardDto,
} from '../products/products.types';
import { ProductCardComponent } from '../products/product-card.component';
import { HomeBrandCardComponent } from './home-brand-card/home-brand-card.component';
import { HomeCategoryCardComponent } from './home-category-card/home-category-card.component';
import { HomeSliderComponent } from './home-slider/home-slider.component';
import { HomePromotionPostsComponent } from './home-promotion-posts/home-promotion-posts.component';
import { HomeScrollRowComponent } from './home-scroll-row/home-scroll-row.component';
import { HomeSectionHeaderComponent } from './home-section-header/home-section-header.component';
import { HomeSpecialOffersComponent } from './home-special-offers/home-special-offers.component';
import { PublicSpecialOffersService } from '../special-offers/public-special-offers.service';
import { PublicSpecialOfferCardDto } from '../profile/special-offers/special-offers.types';
import { SiteServiceDto } from '../admin/dashboard/site-services.types';

@Component({
  selector: 'app-home',
  standalone: true,
  imports: [
    TranslocoPipe,
    ProductCardComponent,
    MatIconModule,
    HomeSliderComponent,
    HomeSectionHeaderComponent,
    HomeScrollRowComponent,
    HomeBrandCardComponent,
    HomeCategoryCardComponent,
    HomePromotionPostsComponent,
    HomeSpecialOffersComponent,
    PostCardComponent,
  ],
  templateUrl: './home.component.html',
})
export class HomeComponent implements OnInit, OnDestroy {
  private _siteContext = inject(SiteContextService);
  private _portalLabAccess = inject(PortalLabAccessService);
  private _publicAdvertisementsService = inject(PublicAdvertisementsService);
  private _destroyed = false;
  private _deferredTimers: ReturnType<typeof setTimeout>[] = [];
  private _generations = new Map<HomeSectionKey, number>();

  categories: PublicCategoryGroupCardDto[] = [];
  latestPosts: PublicPostCardDto[] = [];
  promotionAds: PublicAdvertisementCardDto[] = [];
  specialOffers: PublicSpecialOfferCardDto[] = [];
  brands: PublicBrandCardDto[] = [];
  latestProducts: PublicProductListingCardDto[] = [];
  mostViewedProducts: PublicProductListingCardDto[] = [];
  categoryGroupSections: HomeCategoryGroupSection[] = [];
  slides: ActiveSliderSlideDto[] = [];
  siteServices: SiteServiceDto[] = [];
  siteSettings: SiteSettingsPublicDto | null = null;

  categoriesLoading = false;
  postsLoading = false;
  promotionPostsLoading = false;
  specialOffersLoading = false;
  brandsLoading = false;
  mostViewedLoading = false;
  categoryGroupSectionsLoading = false;
  productsLoading = false;
  siteServicesLoading = false;
  slidesLoading = false;

  categoriesError = false;
  postsError = false;
  promotionPostsError = false;
  specialOffersError = false;
  brandsError = false;
  mostViewedError = false;
  categoryGroupSectionsError = false;
  categoryGroupSectionsPartialError = false;
  productsError = false;
  siteServicesError = false;
  slidesError = false;

  readonly serviceThemes = [
    { circleBg: '#F1ECFA', iconColor: '#6B5B95' },
    { circleBg: '#E8F1FA', iconColor: '#4A6F94' },
    { circleBg: '#FFF3E8', iconColor: '#C97A2B' },
    { circleBg: '#EAF7EE', iconColor: '#3D8B5A' },
  ] as const;

  readonly skeletonCategorySlots = [0, 1, 2, 3, 4, 5];
  readonly skeletonProductSlots = [0, 1, 2, 3];
  readonly skeletonBrandSlots = [0, 1, 2, 3, 4, 5];
  readonly skeletonPostSlots = [0, 1];

  serviceTheme(index: number) {
    return this.serviceThemes[index % this.serviceThemes.length];
  }

  publicSiteServiceImageUrl(imagePath: string): string {
    return PublicSiteService.siteServiceImageUrl(imagePath, 240);
  }

  readonly mostViewedQueryParams = { sortBy: 'views' };
  readonly homePostsCount = 8;
  readonly homeProductsCount = 10;
  private static readonly categoryGroupBatchSize = 2;

  categoryGroupQueryParams(group: {
    productCategoryGroupId: number;
    title: string;
  }) {
    return {
      categoryGroupId: group.productCategoryGroupId,
      categoryGroupTitle: group.title,
    };
  }

  constructor(
    private _publicCategoriesService: PublicCategoriesService,
    private _publicPostsService: PublicPostsService,
    private _publicBrandsService: PublicBrandsService,
    private _publicProductsService: PublicProductsService,
    private _publicSiteService: PublicSiteService,
    private _publicSpecialOffersService: PublicSpecialOffersService
  ) {}

  ngOnInit(): void {
    this.startHomeLoads();
    void this._portalLabAccess.ensure();
  }

  ngOnDestroy(): void {
    this._destroyed = true;
    for (const timer of this._deferredTimers) {
      clearTimeout(timer);
    }
    this._deferredTimers = [];
  }

  private startHomeLoads(): void {
    void this.loadCategories();
    void this.loadMostViewedProducts();
    void this.loadLatestProducts();
    void this.loadBrands();

    void this.loadPromotionPosts();
    void this.loadLatestPosts();
    void this.loadCategoryGroupSections();

    // Reserve compact independent slots so low-priority sections don't leave a blank gap
    // before their deferred requests start — without blocking critical loads above.
    this.slidesLoading = true;
    this.siteServicesLoading = true;
    this.specialOffersLoading = true;

    this.scheduleDeferred(() => {
      void this.loadSiteContent();
      void this.loadSiteServices();
      void this.loadSpecialOffers();
    }, this.isCompactViewport() ? 450 : 120);
  }

  private isCompactViewport(): boolean {
    return (
      typeof matchMedia === 'function' &&
      matchMedia('(max-width: 767px)').matches
    );
  }

  private scheduleDeferred(work: () => void, delayMs: number): void {
    const timer = setTimeout(() => {
      this._deferredTimers = this._deferredTimers.filter((t) => t !== timer);
      if (this._destroyed) return;
      work();
    }, delayMs);
    this._deferredTimers.push(timer);
  }

  private alive(): boolean {
    return !this._destroyed;
  }

  /**
   * Every load/retry claims a new generation for its section. A response may only
   * touch section state while it still owns the latest generation, so a slow
   * in-flight request can never overwrite the result of a newer retry.
   */
  private startGeneration(section: HomeSectionKey): number {
    const generation = (this._generations.get(section) ?? 0) + 1;
    this._generations.set(section, generation);
    return generation;
  }

  private ownsGeneration(section: HomeSectionKey, generation: number): boolean {
    return this.alive() && this._generations.get(section) === generation;
  }

  async loadSiteContent(): Promise<void> {
    void this.loadSiteSettings();
    await this.loadSlides();
  }

  private async loadSiteSettings(): Promise<void> {
    const generation = this.startGeneration('siteSettings');
    try {
      const settings = await this._siteContext.ensureLoaded();
      if (!this.ownsGeneration('siteSettings', generation)) return;
      this.siteSettings = settings;
    } catch {
      // Site settings back no visible section; a failure here must not affect the slider.
    }
  }

  async loadSlides(): Promise<void> {
    if (!this.alive()) return;
    const generation = this.startGeneration('slides');
    this.slidesLoading = true;
    this.slidesError = false;
    try {
      const result = await this._publicSiteService.getActiveSlides();
      if (!this.ownsGeneration('slides', generation)) return;
      if (result.success) {
        this.slides = result.data ?? [];
      } else {
        this.slidesError = true;
        this.slides = [];
      }
    } catch {
      if (this.ownsGeneration('slides', generation)) {
        this.slidesError = true;
        this.slides = [];
      }
    } finally {
      if (this.ownsGeneration('slides', generation)) this.slidesLoading = false;
    }
  }

  async loadSiteServices(): Promise<void> {
    if (!this.alive()) return;
    const generation = this.startGeneration('siteServices');
    this.siteServicesLoading = true;
    this.siteServicesError = false;
    try {
      const result = await this._publicSiteService.getActiveServices();
      if (!this.ownsGeneration('siteServices', generation)) return;
      if (result.success) {
        this.siteServices = result.data ?? [];
      } else {
        this.siteServicesError = true;
        this.siteServices = [];
      }
    } catch {
      if (this.ownsGeneration('siteServices', generation)) {
        this.siteServicesError = true;
        this.siteServices = [];
      }
    } finally {
      if (this.ownsGeneration('siteServices', generation))
        this.siteServicesLoading = false;
    }
  }

  async loadCategories(): Promise<void> {
    if (!this.alive()) return;
    const generation = this.startGeneration('categories');
    this.categoriesLoading = true;
    this.categoriesError = false;
    try {
      const result = await this._publicCategoriesService.getHomeCategories();
      if (!this.ownsGeneration('categories', generation)) return;
      if (result.success) {
        this.categories = result.data ?? [];
      } else {
        this.categoriesError = true;
        this.categories = [];
      }
    } catch {
      if (this.ownsGeneration('categories', generation)) {
        this.categoriesError = true;
        this.categories = [];
      }
    } finally {
      if (this.ownsGeneration('categories', generation))
        this.categoriesLoading = false;
    }
  }

  async loadLatestPosts(): Promise<void> {
    if (!this.alive()) return;
    const generation = this.startGeneration('posts');
    this.postsLoading = true;
    this.postsError = false;
    try {
      const result = await this._publicPostsService.getHomePosts(
        this.homePostsCount
      );
      if (!this.ownsGeneration('posts', generation)) return;
      if (result.success) {
        this.latestPosts = result.data ?? [];
      } else {
        this.postsError = true;
        this.latestPosts = [];
      }
    } catch {
      if (this.ownsGeneration('posts', generation)) {
        this.postsError = true;
        this.latestPosts = [];
      }
    } finally {
      if (this.ownsGeneration('posts', generation)) this.postsLoading = false;
    }
  }

  async loadPromotionPosts(): Promise<void> {
    if (!this.alive()) return;
    const generation = this.startGeneration('promotionPosts');
    this.promotionPostsLoading = true;
    this.promotionPostsError = false;
    try {
      const result = await this._publicAdvertisementsService.getActiveForHome(
        4
      );
      if (!this.ownsGeneration('promotionPosts', generation)) return;
      if (result.success) {
        this.promotionAds = result.data ?? [];
      } else {
        this.promotionPostsError = true;
        this.promotionAds = [];
      }
    } catch {
      if (this.ownsGeneration('promotionPosts', generation)) {
        this.promotionPostsError = true;
        this.promotionAds = [];
      }
    } finally {
      if (this.ownsGeneration('promotionPosts', generation))
        this.promotionPostsLoading = false;
    }
  }

  async loadSpecialOffers(): Promise<void> {
    if (!this.alive()) return;
    const generation = this.startGeneration('specialOffers');
    this.specialOffersLoading = true;
    this.specialOffersError = false;
    try {
      const result = await this._publicSpecialOffersService.getActiveForHome();
      if (!this.ownsGeneration('specialOffers', generation)) return;
      if (result.success) {
        this.specialOffers = result.data ?? [];
      } else {
        this.specialOffersError = true;
        this.specialOffers = [];
      }
    } catch {
      if (this.ownsGeneration('specialOffers', generation)) {
        this.specialOffersError = true;
        this.specialOffers = [];
      }
    } finally {
      if (this.ownsGeneration('specialOffers', generation))
        this.specialOffersLoading = false;
    }
  }

  get primaryPromotionAds(): PublicAdvertisementCardDto[] {
    return this.promotionAds.slice(0, 2);
  }

  get secondaryPromotionAds(): PublicAdvertisementCardDto[] {
    return this.promotionAds.slice(2, 4);
  }

  get homeSpecialOffers(): PublicSpecialOfferCardDto[] {
    return this.specialOffers;
  }

  async loadBrands(): Promise<void> {
    if (!this.alive()) return;
    const generation = this.startGeneration('brands');
    this.brandsLoading = true;
    this.brandsError = false;
    try {
      const result = await this._publicBrandsService.getHomeBrands();
      if (!this.ownsGeneration('brands', generation)) return;
      if (result.success) {
        this.brands = result.data ?? [];
      } else {
        this.brandsError = true;
        this.brands = [];
      }
    } catch {
      if (this.ownsGeneration('brands', generation)) {
        this.brandsError = true;
        this.brands = [];
      }
    } finally {
      if (this.ownsGeneration('brands', generation)) this.brandsLoading = false;
    }
  }

  async loadMostViewedProducts(): Promise<void> {
    if (!this.alive()) return;
    const generation = this.startGeneration('mostViewed');
    this.mostViewedLoading = true;
    this.mostViewedError = false;
    try {
      const result = await this._publicProductsService.getPublishedListings({
        page: 1,
        pageSize: this.homeProductsCount,
        sortBy: 'views',
      });
      if (!this.ownsGeneration('mostViewed', generation)) return;
      if (result.success) {
        this.mostViewedProducts = result.data?.items ?? [];
      } else {
        this.mostViewedError = true;
        this.mostViewedProducts = [];
      }
    } catch {
      if (this.ownsGeneration('mostViewed', generation)) {
        this.mostViewedError = true;
        this.mostViewedProducts = [];
      }
    } finally {
      if (this.ownsGeneration('mostViewed', generation))
        this.mostViewedLoading = false;
    }
  }

  async loadCategoryGroupSections(): Promise<void> {
    if (!this.alive()) return;
    const generation = this.startGeneration('categoryGroups');
    this.categoryGroupSectionsLoading = true;
    this.categoryGroupSectionsError = false;
    this.categoryGroupSectionsPartialError = false;
    this.categoryGroupSections = [];

    let requestedGroups = 0;
    let failedGroups = 0;

    try {
      const filtersResult =
        await this._publicProductsService.getListingFilters();
      if (!this.ownsGeneration('categoryGroups', generation)) return;
      if (!filtersResult.success) {
        this.categoryGroupSectionsError = true;
        return;
      }

      const groups = [
        ...(filtersResult.data?.categoryGroups
          ? filtersResult.data.categoryGroups
          : []),
      ].sort((a, b) => a.productCategoryGroupId - b.productCategoryGroupId);

      for (
        let i = 0;
        i < groups.length;
        i += HomeComponent.categoryGroupBatchSize
      ) {
        if (!this.ownsGeneration('categoryGroups', generation)) return;
        const batch = groups.slice(i, i + HomeComponent.categoryGroupBatchSize);
        const batchResults = await Promise.all(
          batch.map(async (group) => {
            try {
              const result =
                await this._publicProductsService.getPublishedListings({
                  page: 1,
                  pageSize: this.homeProductsCount,
                  categoryGroupId: group.productCategoryGroupId,
                });
              if (!result.success) {
                return { group, products: [], failed: true };
              }
              return {
                group,
                products: this.mapProductsForHomeSection(
                  result.data?.items ?? [],
                  group.title
                ),
                failed: false,
              };
            } catch {
              return { group, products: [], failed: true };
            }
          })
        );
        if (!this.ownsGeneration('categoryGroups', generation)) return;

        requestedGroups += batchResults.length;
        failedGroups += batchResults.filter((entry) => entry.failed).length;

        const withProducts = batchResults
          .filter((entry) => !entry.failed && entry.products.length > 0)
          .map(
            ({ group, products }) =>
              ({ group, products }) satisfies HomeCategoryGroupSection
          );
        if (withProducts.length) {
          this.categoryGroupSections = [
            ...this.categoryGroupSections,
            ...withProducts,
          ];
        }
      }

      // A failed listing request must never be presented as "no data".
      if (requestedGroups > 0 && failedGroups === requestedGroups) {
        this.categoryGroupSectionsError = true;
      } else if (failedGroups > 0) {
        this.categoryGroupSectionsPartialError = true;
      }
    } catch {
      if (this.ownsGeneration('categoryGroups', generation)) {
        if (this.categoryGroupSections.length) {
          this.categoryGroupSectionsPartialError = true;
        } else {
          this.categoryGroupSectionsError = true;
        }
      }
    } finally {
      if (this.ownsGeneration('categoryGroups', generation))
        this.categoryGroupSectionsLoading = false;
    }
  }

  async loadLatestProducts(): Promise<void> {
    if (!this.alive()) return;
    const generation = this.startGeneration('products');
    this.productsLoading = true;
    this.productsError = false;
    try {
      const result = await this._publicProductsService.getPublishedListings({
        page: 1,
        pageSize: this.homeProductsCount,
      });
      if (!this.ownsGeneration('products', generation)) return;
      if (result.success) {
        this.latestProducts = result.data?.items ?? [];
      } else {
        this.productsError = true;
        this.latestProducts = [];
      }
    } catch {
      if (this.ownsGeneration('products', generation)) {
        this.productsError = true;
        this.latestProducts = [];
      }
    } finally {
      if (this.ownsGeneration('products', generation))
        this.productsLoading = false;
    }
  }

  private static readonly homeGroupsWithoutCompanyInTitle = new Set([
    'کاریابی و استخدام',
    'خدمات و بازرگانی آزمایشگاه',
  ]);

  private mapProductsForHomeSection(
    products: PublicProductListingCardDto[],
    groupTitle: string
  ): PublicProductListingCardDto[] {
    if (!HomeComponent.homeGroupsWithoutCompanyInTitle.has(groupTitle)) {
      return products;
    }

    return products.map((product) => this.stripCompanyFromProductTitle(product));
  }

  private stripCompanyFromProductTitle(
    product: PublicProductListingCardDto
  ): PublicProductListingCardDto {
    const title = product.title?.trim() ?? '';
    const separatorIndex = title.lastIndexOf(' - ');
    if (separatorIndex <= 0) {
      return product;
    }

    const adTitle = title.slice(0, separatorIndex).trim();
    if (!adTitle) {
      return product;
    }

    return { ...product, title: adTitle };
  }
}

type HomeSectionKey =
  | 'siteSettings'
  | 'slides'
  | 'siteServices'
  | 'categories'
  | 'posts'
  | 'promotionPosts'
  | 'specialOffers'
  | 'brands'
  | 'mostViewed'
  | 'categoryGroups'
  | 'products';

interface HomeCategoryGroupSection {
  group: PublicCategoryGroupFilterDto;
  products: PublicProductListingCardDto[];
}
