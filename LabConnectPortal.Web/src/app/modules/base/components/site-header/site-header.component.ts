import {
  Component,
  DestroyRef,
  ElementRef,
  HostListener,
  OnDestroy,
  OnInit,
  inject,
} from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { FormControl, ReactiveFormsModule } from '@angular/forms';
import { NavigationEnd, Router, RouterLink, RouterLinkActive } from '@angular/router';
import { MatIconModule } from '@angular/material/icon';
import { TranslocoPipe } from '@jsverse/transloco';
import { AuthUtils } from '@core/services/auth/auth.utils';
import { PublicSiteService } from '@core/services/site/public-site.service';
import { SiteContextService } from '@core/services/site/site-context.service';
import { SiteSettingsPublicDto } from '@modules/admin/settings/site-settings.types';
import { HeaderUserMenuComponent } from '@modules/base/components/header-user-menu/header-user-menu.component';
import { HeaderCategoryMenuComponent } from '@modules/base/components/header-category-menu/header-category-menu.component';
import { PublicProductsService } from '@modules/products/products.service';
import { ProductSearchSuggestionDto } from '@modules/products/products.types';
import {
  catchError,
  debounceTime,
  distinctUntilChanged,
  filter,
  from,
  map,
  of,
  switchMap,
} from 'rxjs';

@Component({
  selector: 'app-site-header',
  standalone: true,
  imports: [
    ReactiveFormsModule,
    MatIconModule,
    TranslocoPipe,
    RouterLink,
    RouterLinkActive,
    HeaderUserMenuComponent,
    HeaderCategoryMenuComponent,
  ],
  templateUrl: './site-header.component.html',
})
export class SiteHeaderComponent implements OnInit, OnDestroy {
  private _siteContext = inject(SiteContextService);
  private _router = inject(Router);
  private _authUtils = inject(AuthUtils);
  private _products = inject(PublicProductsService);
  private _elementRef = inject(ElementRef<HTMLElement>);
  private _destroyRef = inject(DestroyRef);

  readonly navItems = [
    {
      link: '/products',
      labelKey: 'modules.home.nav.products',
      icon: 'storefront',
      exact: false,
    },
    {
      link: '/special-offers',
      labelKey: 'modules.home.nav.specialOffers',
      icon: 'local_offer',
      exact: false,
    },
    {
      link: '/news',
      labelKey: 'modules.home.nav.news',
      icon: 'newspaper',
      exact: false,
    },
    {
      link: '/articles',
      labelKey: 'modules.home.nav.articles',
      icon: 'menu_book',
      exact: false,
    },
    {
      link: '/documents',
      labelKey: 'modules.home.nav.documents',
      icon: 'folder_shared',
      exact: false,
    },
    {
      link: '/contact-us',
      labelKey: 'modules.home.nav.contactUs',
      icon: 'support_agent',
      exact: false,
    },
    {
      link: '/company-regulations',
      labelKey: 'modules.home.nav.companyRegulations',
      icon: 'gavel',
      exact: false,
    },
  ] as const;

  siteSettings: SiteSettingsPublicDto | null = null;
  searchControl = new FormControl('');
  suggestions: ProductSearchSuggestionDto[] = [];
  showSuggestions = false;
  searchLoading = false;
  mobileMenuOpen = false;

  get siteTitle(): string {
    return this.siteSettings?.siteTitle?.trim() || 'LabConnect';
  }

  get siteTagline(): string {
    return this.siteSettings?.tagline?.trim() ?? '';
  }

  get logoUrl(): string | null {
    return this.siteSettings?.hasLogo ? PublicSiteService.logoUrl() : null;
  }

  get isAuthenticated(): boolean {
    return this._authUtils.isAuthenticated;
  }

  get supportPhone(): string {
    return (
      this.siteSettings?.supportLandline?.trim() ||
      this.siteSettings?.supportMobile?.trim() ||
      ''
    );
  }

  get loginLabelKey(): string {
    return 'modules.home.loginRegister';
  }

  ngOnInit(): void {
    void this.loadSiteSettings();
    this.setupSearchSuggestions();
    this._router.events
      .pipe(
        filter((event): event is NavigationEnd => event instanceof NavigationEnd),
        takeUntilDestroyed(this._destroyRef),
      )
      .subscribe(() => this.closeMobileMenu());
  }

  @HostListener('document:click', ['$event'])
  onDocumentClick(event: MouseEvent): void {
    if (!this._elementRef.nativeElement.contains(event.target as Node)) {
      this.showSuggestions = false;
    }
  }

  @HostListener('document:keydown.escape')
  onEscape(): void {
    if (this.mobileMenuOpen) this.closeMobileMenu();
  }

  toggleMobileMenu(): void {
    if (this.mobileMenuOpen) this.closeMobileMenu();
    else this.openMobileMenu();
  }

  openMobileMenu(): void {
    this.mobileMenuOpen = true;
    this.showSuggestions = false;
  }

  closeMobileMenu(): void {
    this.mobileMenuOpen = false;
  }

  ngOnDestroy(): void {
    this.mobileMenuOpen = false;
  }

  async loadSiteSettings(): Promise<void> {
    try {
      this.siteSettings = await this._siteContext.ensureLoaded();
    } catch {
      // Header still renders with defaults when settings fail to load.
    }
  }

  onSearchFocus(): void {
    const term = this.searchControl.value?.trim() ?? '';
    if (term.length >= 2) {
      this.showSuggestions = true;
    }
  }

  onSearch(event: Event): void {
    event.preventDefault();
    const title = this.searchControl.value?.trim();
    this.showSuggestions = false;
    void this._router.navigate(['/products'], {
      queryParams: title ? { title } : {},
    });
  }

  clearSearch(event: Event): void {
    event.preventDefault();
    event.stopPropagation();
    this.searchControl.setValue('');
    this.suggestions = [];
    this.showSuggestions = false;
  }

  selectSuggestion(suggestion: ProductSearchSuggestionDto, event: Event): void {
    event.preventDefault();
    event.stopPropagation();
    this.showSuggestions = false;

    if (suggestion.type === 'product' && suggestion.productId) {
      void this._router.navigate(['/products', suggestion.productId]);
      return;
    }

    if (suggestion.type === 'category' && suggestion.categoryId) {
      void this._router.navigate(['/products'], {
        queryParams: { categoryId: suggestion.categoryId },
      });
    }
  }

  trackSuggestion(item: ProductSearchSuggestionDto): string {
    return item.type === 'product'
      ? `product-${item.productId}`
      : `category-${item.categoryId}`;
  }

  goToLogin(): void {
    void this._router.navigate(['/auth/login']);
  }

  private setupSearchSuggestions(): void {
    this.searchControl.valueChanges
      .pipe(
        debounceTime(250),
        distinctUntilChanged(),
        switchMap((value) => {
          const term = value?.trim() ?? '';
          if (term.length < 2) {
            this.suggestions = [];
            this.showSuggestions = false;
            this.searchLoading = false;
            return of<ProductSearchSuggestionDto[]>([]);
          }

          this.searchLoading = true;
          this.showSuggestions = true;

          return from(this._products.searchSuggestions(term)).pipe(
            map((result) => (result.success && result.data ? result.data : [])),
            catchError(() => of<ProductSearchSuggestionDto[]>([]))
          );
        }),
        takeUntilDestroyed(this._destroyRef)
      )
      .subscribe((items) => {
        this.suggestions = items;
        this.searchLoading = false;
      });
  }
}
