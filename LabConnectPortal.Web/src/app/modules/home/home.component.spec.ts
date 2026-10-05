import { TestBed } from '@angular/core/testing';
import { PublicSiteService } from '@core/services/site/public-site.service';
import { SiteContextService } from '@core/services/site/site-context.service';
import { PortalLabAccessService } from '@core/services/site/portal-lab-access.service';
import { PublicAdvertisementsService } from '../admin/advertisements/advertisements.service';
import { PublicPostsService } from '../posts/public-posts.service';
import { PublicBrandsService } from '../products/public-brands.service';
import { PublicCategoriesService } from '../products/public-categories.service';
import { PublicProductsService } from '../products/products.service';
import { PublicSpecialOffersService } from '../special-offers/public-special-offers.service';
import { HomeComponent } from './home.component';

interface Deferred<T> {
  promise: Promise<T>;
  resolve: (value: T) => void;
  reject: (reason?: unknown) => void;
}

function defer<T>(): Deferred<T> {
  let resolve!: (value: T) => void;
  let reject!: (reason?: unknown) => void;
  const promise = new Promise<T>((res, rej) => {
    resolve = res;
    reject = rej;
  });
  return { promise, resolve, reject };
}

function ok<T>(data: T) {
  return { status: true, success: true, data } as any;
}

function category(productCategoryGroupId: number, title: string) {
  return { productCategoryGroupId, title, homePageImagePath: '' } as any;
}

function fail<T = never>(message = 'boom') {
  return { status: false, success: false, message } as {
    status: boolean;
    success: boolean;
    message: string;
    data?: T;
  };
}

/** Lets pending microtasks (awaits inside the component) settle. */
async function flush(): Promise<void> {
  for (let i = 0; i < 5; i++) {
    await Promise.resolve();
  }
}

describe('HomeComponent independent section loading', () => {
  let categoriesService: jasmine.SpyObj<PublicCategoriesService>;
  let postsService: jasmine.SpyObj<PublicPostsService>;
  let brandsService: jasmine.SpyObj<PublicBrandsService>;
  let productsService: jasmine.SpyObj<PublicProductsService>;
  let siteService: jasmine.SpyObj<PublicSiteService>;
  let specialOffersService: jasmine.SpyObj<PublicSpecialOffersService>;
  let siteContext: jasmine.SpyObj<SiteContextService>;
  let advertisementsService: jasmine.SpyObj<PublicAdvertisementsService>;

  function createComponent(): HomeComponent {
    return TestBed.runInInjectionContext(
      () =>
        new HomeComponent(
          categoriesService,
          postsService,
          brandsService,
          productsService,
          siteService,
          specialOffersService
        )
    );
  }

  beforeEach(() => {
    categoriesService = jasmine.createSpyObj<PublicCategoriesService>(
      'PublicCategoriesService',
      ['getHomeCategories']
    );
    postsService = jasmine.createSpyObj<PublicPostsService>(
      'PublicPostsService',
      ['getHomePosts']
    );
    brandsService = jasmine.createSpyObj<PublicBrandsService>(
      'PublicBrandsService',
      ['getHomeBrands']
    );
    productsService = jasmine.createSpyObj<PublicProductsService>(
      'PublicProductsService',
      ['getPublishedListings', 'getListingFilters']
    );
    siteService = jasmine.createSpyObj<PublicSiteService>('PublicSiteService', [
      'getActiveSlides',
      'getActiveServices',
    ]);
    specialOffersService = jasmine.createSpyObj<PublicSpecialOffersService>(
      'PublicSpecialOffersService',
      ['getActiveForHome']
    );
    siteContext = jasmine.createSpyObj<SiteContextService>(
      'SiteContextService',
      ['ensureLoaded']
    );
    advertisementsService = jasmine.createSpyObj<PublicAdvertisementsService>(
      'PublicAdvertisementsService',
      ['getActiveForHome']
    );

    TestBed.configureTestingModule({
      providers: [
        { provide: SiteContextService, useValue: siteContext },
        { provide: PublicAdvertisementsService, useValue: advertisementsService },
        {
          provide: PortalLabAccessService,
          useValue: { ensure: () => Promise.resolve() },
        },
      ],
    });
  });

  describe('single section lifecycle', () => {
    it('goes Loading -> Success', async () => {
      const pending = defer<any>();
      categoriesService.getHomeCategories.and.returnValue(pending.promise);
      const component = createComponent();

      const load = component.loadCategories();
      expect(component.categoriesLoading).toBeTrue();

      pending.resolve(ok([category(1, 'A')]));
      await load;

      expect(component.categoriesLoading).toBeFalse();
      expect(component.categoriesError).toBeFalse();
      expect(component.categories.length).toBe(1);
    });

    it('goes Loading -> Empty without an error state', async () => {
      categoriesService.getHomeCategories.and.returnValue(
        Promise.resolve(ok([]))
      );
      const component = createComponent();

      await component.loadCategories();

      expect(component.categoriesLoading).toBeFalse();
      expect(component.categoriesError).toBeFalse();
      expect(component.categories).toEqual([]);
    });

    it('goes Loading -> Error when the request reports failure', async () => {
      categoriesService.getHomeCategories.and.returnValue(
        Promise.resolve(fail())
      );
      const component = createComponent();

      await component.loadCategories();

      expect(component.categoriesError).toBeTrue();
      expect(component.categories).toEqual([]);
    });

    it('goes Error -> Retry -> Loading -> Success', async () => {
      categoriesService.getHomeCategories.and.returnValue(
        Promise.resolve(fail())
      );
      const component = createComponent();
      await component.loadCategories();
      expect(component.categoriesError).toBeTrue();

      categoriesService.getHomeCategories.and.returnValue(
        Promise.resolve(ok([category(2, 'B')]))
      );
      await component.loadCategories();

      expect(component.categoriesError).toBeFalse();
      expect(component.categories.length).toBe(1);
    });
  });

  describe('retry races', () => {
    it('ignores the stale response when a retry is already in flight', async () => {
      const first = defer<any>();
      const second = defer<any>();
      categoriesService.getHomeCategories.and.returnValues(
        first.promise,
        second.promise
      );
      const component = createComponent();

      const firstLoad = component.loadCategories();
      const secondLoad = component.loadCategories();

      second.resolve(ok([category(9, 'fresh')]));
      await secondLoad;

      expect(component.categoriesLoading).toBeFalse();
      expect(component.categories.length).toBe(1);

      first.resolve(ok([category(1, 'stale')]));
      await firstLoad;

      expect(component.categories.length).toBe(1);
      expect(component.categories[0].title).toBe('fresh');
      expect(component.categoriesLoading).toBeFalse();
    });

    it('does not let a stale failure overwrite a newer success', async () => {
      const first = defer<any>();
      const second = defer<any>();
      categoriesService.getHomeCategories.and.returnValues(
        first.promise,
        second.promise
      );
      const component = createComponent();

      const firstLoad = component.loadCategories();
      const secondLoad = component.loadCategories();

      second.resolve(ok([category(9, 'fresh')]));
      await secondLoad;

      first.reject(new Error('late failure'));
      await firstLoad;

      expect(component.categoriesError).toBeFalse();
      expect(component.categories.length).toBe(1);
    });
  });

  describe('destroy mid-request', () => {
    it('does not touch state after the component is destroyed', async () => {
      const pending = defer<any>();
      categoriesService.getHomeCategories.and.returnValue(pending.promise);
      const component = createComponent();

      const load = component.loadCategories();
      component.ngOnDestroy();

      pending.resolve(ok([category(1, 'late')]));
      await load;

      expect(component.categories).toEqual([]);
      expect(component.categoriesLoading).toBeTrue();
    });
  });

  describe('slider vs site settings', () => {
    it('keeps the slider successful when site settings fail', async () => {
      siteContext.ensureLoaded.and.returnValue(
        Promise.reject(new Error('settings down'))
      );
      siteService.getActiveSlides.and.returnValue(
        Promise.resolve(ok([{ sliderSlideId: 1 } as any]))
      );
      const component = createComponent();

      await component.loadSiteContent();
      await flush();

      expect(component.slidesError).toBeFalse();
      expect(component.slides.length).toBe(1);
      expect(component.siteSettings).toBeNull();
    });

    it('marks only the slider as failed when slides fail', async () => {
      siteContext.ensureLoaded.and.returnValue(
        Promise.resolve({ siteName: 'x' } as any)
      );
      siteService.getActiveSlides.and.returnValue(Promise.resolve(fail()));
      const component = createComponent();

      await component.loadSiteContent();
      await flush();

      expect(component.slidesError).toBeTrue();
      expect(component.slides).toEqual([]);
      expect(component.siteSettings).toEqual({ siteName: 'x' } as any);
    });
  });

  describe('category group sections', () => {
    const groups = {
      categoryGroups: [category(1, 'G1'), category(2, 'G2')],
    };

    it('treats a total listing failure as Error, not Empty', async () => {
      productsService.getListingFilters.and.returnValue(
        Promise.resolve(ok(groups) as any)
      );
      productsService.getPublishedListings.and.returnValue(
        Promise.resolve(fail() as any)
      );
      const component = createComponent();

      await component.loadCategoryGroupSections();

      expect(component.categoryGroupSections).toEqual([]);
      expect(component.categoryGroupSectionsError).toBeTrue();
      expect(component.categoryGroupSectionsLoading).toBeFalse();
    });

    it('renders successful groups and flags a partial failure', async () => {
      productsService.getListingFilters.and.returnValue(
        Promise.resolve(ok(groups) as any)
      );
      productsService.getPublishedListings.and.callFake((query: any) =>
        query.categoryGroupId === 1
          ? (Promise.resolve(ok({ items: [{ productId: 'p1' }] })) as any)
          : (Promise.resolve(fail()) as any)
      );
      const component = createComponent();

      await component.loadCategoryGroupSections();

      expect(component.categoryGroupSections.length).toBe(1);
      expect(component.categoryGroupSectionsError).toBeFalse();
      expect(component.categoryGroupSectionsPartialError).toBeTrue();
    });

    it('stays Empty when every group succeeds with no items', async () => {
      productsService.getListingFilters.and.returnValue(
        Promise.resolve(ok(groups) as any)
      );
      productsService.getPublishedListings.and.returnValue(
        Promise.resolve(ok({ items: [] }) as any)
      );
      const component = createComponent();

      await component.loadCategoryGroupSections();

      expect(component.categoryGroupSections).toEqual([]);
      expect(component.categoryGroupSectionsError).toBeFalse();
      expect(component.categoryGroupSectionsPartialError).toBeFalse();
    });

    it('does not let a stale batched run append to a newer run', async () => {
      const staleFilters = defer<any>();
      const freshFilters = defer<any>();
      productsService.getListingFilters.and.returnValues(
        staleFilters.promise,
        freshFilters.promise
      );
      productsService.getPublishedListings.and.callFake(
        () => Promise.resolve(ok({ items: [{ productId: 'p1' }] })) as any
      );
      const component = createComponent();

      const staleRun = component.loadCategoryGroupSections();
      const freshRun = component.loadCategoryGroupSections();

      freshFilters.resolve(ok(groups));
      await freshRun;
      const sectionsAfterFreshRun = component.categoryGroupSections.length;

      staleFilters.resolve(ok(groups));
      await staleRun;

      expect(component.categoryGroupSections.length).toBe(
        sectionsAfterFreshRun
      );
    });
  });
});
