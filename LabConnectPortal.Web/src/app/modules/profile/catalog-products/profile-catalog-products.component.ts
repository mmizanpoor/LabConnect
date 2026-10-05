import { AsyncPipe, NgClass } from '@angular/common';
import {
  Component,
  DestroyRef,
  OnInit,
  ViewChild,
  inject,
} from '@angular/core';
import { FormControl, ReactiveFormsModule } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatDrawer, MatSidenavModule } from '@angular/material/sidenav';
import { ActivatedRoute, Router } from '@angular/router';
import { TranslocoPipe } from '@jsverse/transloco';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { BehaviorSubject } from 'rxjs';
import jMoment, { Moment } from 'moment-jalaali';
import { LocalizationService } from '@core/services/localization/localization.service';
import { BaseButtonComponent } from '@modules/base/components/base-button/base-button.component';
import {
  BaseFormFieldComponent,
  BaseFormSelectOption,
} from '@modules/base/components/base-form-field/base-form-field.component';
import {
  BaseMenuActionItem,
  BaseMenuActionsComponent,
} from '@modules/base/components/base-menu-actions/base-menu-actions.component';
import { BasePagingComponent } from '@modules/base/components/base-paging/base-paging.component';
import { ProductCatalogService } from '@modules/admin/products/product-catalog.service';
import {
  ProductListItemDto,
  ProductStatus,
} from '@modules/admin/products/product-catalog.types';
import { SystemEntity } from '@core/system-entity/system-entity';
import { bindSystemEntity } from '@core/system-entity/bind-system-entity';
import { LabPermissionService } from '@core/services/auth/lab-permission.service';

interface ProductRow extends ProductListItemDto {
  categoriesText: string;
}

type StatusTab = 'all' | 'active' | 'draft' | 'published';

const STATUS_QUERY_VALUES = new Set<string>([
  'Draft',
  'PendingApproval',
  'Approved',
  'Rejected',
  'Unpublished',
]);

@Component({
  selector: 'app-profile-catalog-products',
  standalone: true,
  imports: [
    AsyncPipe,
    NgClass,
    ReactiveFormsModule,
    MatButtonModule,
    MatIconModule,
    MatSidenavModule,
    TranslocoPipe,
    BaseButtonComponent,
    BaseFormFieldComponent,
    BaseMenuActionsComponent,
    BasePagingComponent,
  ],
  templateUrl: './profile-catalog-products.component.html',
  styleUrl: './profile-catalog-products.component.scss',
})
export class ProfileCatalogProductsComponent implements OnInit {
  readonly entity = SystemEntity.Product;
  private readonly _systemEntityBinding = bindSystemEntity(
    SystemEntity.Product
  );
  private readonly _destroyRef = inject(DestroyRef);

  @ViewChild('filterDrawer') filterDrawer?: MatDrawer;

  loading = false;
  error = '';
  success = '';
  readonly list$ = new BehaviorSubject<ProductRow[]>([]);
  totalCount = 0;
  page = 1;
  pageSize = 10;
  activeStatusTab: StatusTab = 'all';

  titleControl = new FormControl('', { nonNullable: true });
  statusControl = new FormControl<ProductStatus | null>(null);
  /** null = all; true = resume categories; false = non-resume */
  acceptsResumeFilter: boolean | null = null;
  fromDateControl = new FormControl<Moment | null>(null);
  toDateControl = new FormControl<Moment | null>(null);

  readonly statusTabs: {
    id: StatusTab;
    labelKey: string;
    status: ProductStatus | null;
  }[] = [
    { id: 'all', labelKey: 'shared.all', status: null },
    {
      id: 'active',
      labelKey: 'modules.profile.productListings.tabs.active',
      status: 'Approved',
    },
    {
      id: 'draft',
      labelKey: 'modules.profile.productListings.tabs.draft',
      status: 'Draft',
    },
    {
      id: 'published',
      labelKey: 'modules.profile.productListings.tabs.published',
      status: 'Approved',
    },
  ];

  constructor(
    private _catalogService: ProductCatalogService,
    private _router: Router,
    private _route: ActivatedRoute,
    private _localization: LocalizationService,
    private _labPermission: LabPermissionService
  ) {}

  get canCreate(): boolean {
    return this._labPermission.can(SystemEntity.Product, 'create');
  }

  get canUpdate(): boolean {
    return this._labPermission.can(SystemEntity.Product, 'update');
  }

  get canDelete(): boolean {
    return this._labPermission.can(SystemEntity.Product, 'delete');
  }

  get statusFilterOptions(): BaseFormSelectOption[] {
    return [
      { value: null, label: this._localization.translate('shared.all') },
      {
        value: 'Draft',
        label: this._localization.translate(
          'modules.admin.products.status.draft'
        ),
      },
      {
        value: 'PendingApproval',
        label: this._localization.translate(
          'modules.admin.products.status.pendingApproval'
        ),
      },
      {
        value: 'Approved',
        label: this._localization.translate(
          'modules.admin.products.status.approved'
        ),
      },
      {
        value: 'Rejected',
        label: this._localization.translate(
          'modules.admin.products.status.rejected'
        ),
      },
      {
        value: 'Unpublished',
        label: this._localization.translate(
          'modules.admin.products.status.unpublished'
        ),
      },
    ];
  }

  get rangeFrom(): number {
    if (this.totalCount === 0) return 0;
    return (this.page - 1) * this.pageSize + 1;
  }

  get rangeTo(): number {
    return Math.min(this.page * this.pageSize, this.totalCount);
  }

  ngOnInit(): void {
    const listingSubmitted = (
      history.state as { listingSubmitted?: boolean } | null
    )?.listingSubmitted;
    if (listingSubmitted) {
      this.success = this._localization.translate(
        'modules.admin.products.submitForApprovalSuccess'
      );
    }

    this.applyFiltersFromQuery(this._route.snapshot.queryParamMap);
    void this.load();

    this._route.queryParamMap
      .pipe(takeUntilDestroyed(this._destroyRef))
      .subscribe((params) => {
        const status = this.normalizeStatusQuery(params.get('status'));
        const acceptsResume = this.normalizeAcceptsResumeQuery(
          params.get('acceptsResume')
        );
        if (
          status === this.statusControl.value &&
          acceptsResume === this.acceptsResumeFilter
        ) {
          return;
        }
        this.applyFiltersFromQuery(params);
        this.page = 1;
        void this.load();
      });
  }

  private normalizeStatusQuery(value: string | null): ProductStatus | null {
    if (!value || !STATUS_QUERY_VALUES.has(value)) return null;
    return value as ProductStatus;
  }

  private normalizeAcceptsResumeQuery(value: string | null): boolean | null {
    if (value === 'true') return true;
    if (value === 'false') return false;
    return null;
  }

  private applyFiltersFromQuery(params: {
    get(name: string): string | null;
  }): void {
    this.statusControl.setValue(
      this.normalizeStatusQuery(params.get('status')),
      {
        emitEvent: false,
      }
    );
    this.acceptsResumeFilter = this.normalizeAcceptsResumeQuery(
      params.get('acceptsResume')
    );
    this.syncActiveStatusTab();
  }

  private syncActiveStatusTab(): void {
    const status = this.statusControl.value;
    if (status === 'Approved' || status === 2) {
      if (
        this.activeStatusTab !== 'active' &&
        this.activeStatusTab !== 'published'
      ) {
        this.activeStatusTab = 'published';
      }
    } else if (status === 'Draft' || status === 0) {
      this.activeStatusTab = 'draft';
    } else {
      this.activeStatusTab = 'all';
    }
  }

  async load(): Promise<void> {
    this.loading = true;
    this.error = '';
    this.applyFiltersFromQuery(this._route.snapshot.queryParamMap);
    const result = await this._catalogService.getProducts({
      mineOnly: true,
      title: this.titleControl.value.trim() || undefined,
      status: this.statusControl.value ?? undefined,
      ...(this.acceptsResumeFilter === null
        ? {}
        : { acceptsResume: this.acceptsResumeFilter }),
      updatedFrom:
        this.toIsoDate(this.fromDateControl.value, false) ?? undefined,
      updatedTo: this.toIsoDate(this.toDateControl.value, true) ?? undefined,
      page: this.page,
      pageSize: this.pageSize,
    });
    if (result.success && result.data) {
      let items = result.data.items;
      if (this.acceptsResumeFilter === true) {
        items = items.filter((item) => !!item.acceptsResume);
      } else if (this.acceptsResumeFilter === false) {
        items = items.filter((item) => !item.acceptsResume);
      }
      this.list$.next(
        items.map((item) => ({
          ...item,
          categoriesText: item.categoryTitles.join('، '),
        }))
      );
      this.totalCount = result.data.totalCount;
    } else {
      this.error =
        result.message ??
        this._localization.translate(
          'modules.admin.products.errors.loadFailed'
        );
      this.list$.next([]);
    }
    this.loading = false;
  }

  async applyFilters(): Promise<void> {
    this.page = 1;
    await this._router.navigate([], {
      relativeTo: this._route,
      queryParams: {
        status: this.statusControl.value || null,
        acceptsResume:
          this.acceptsResumeFilter === null
            ? null
            : this.acceptsResumeFilter
            ? 'true'
            : 'false',
      },
      queryParamsHandling: 'merge',
    });
    await this.load();
    void this.filterDrawer?.close();
  }

  async selectStatusTab(tab: StatusTab): Promise<void> {
    this.activeStatusTab = tab;
    const match = this.statusTabs.find((t) => t.id === tab);
    this.statusControl.setValue(match?.status ?? null, { emitEvent: false });
    this.page = 1;
    await this._router.navigate([], {
      relativeTo: this._route,
      queryParams: {
        status: this.statusControl.value || null,
      },
      queryParamsHandling: 'merge',
    });
    await this.load();
  }

  async searchByTitle(): Promise<void> {
    this.page = 1;
    await this.load();
  }

  onPageChange(page: number): void {
    this.page = page;
    void this.load();
  }

  openCreate(): void {
    if (!this.canCreate) return;
    void this._router.navigate(['/profile/product-listings/new'], {
      queryParamsHandling: 'preserve',
    });
  }

  openEdit(row: ProductListItemDto): void {
    if (!this.canUpdate) return;
    void this._router.navigate(['/profile/product-listings', row.productId], {
      queryParamsHandling: 'preserve',
    });
  }

  openResumes(row: ProductListItemDto): void {
    void this._router.navigate(
      ['/profile/product-listings', row.productId, 'resumes'],
      {
        queryParamsHandling: 'preserve',
      }
    );
  }

  rowActions(row: ProductListItemDto): BaseMenuActionItem[] {
    const actions: BaseMenuActionItem[] = [];

    if (this.canUpdate) {
      actions.push({
        label: this._localization.translate('shared.edit'),
        icon: 'heroicons_outline:pencil-square',
        iconClass: 'text-[#35507c]',
        action: () => this.openEdit(row),
      });
    }

    if (this.canDelete) {
      actions.push({
        label: this._localization.translate('shared.delete'),
        icon: 'heroicons_outline:trash',
        iconClass: 'text-red-500',
        danger: true,
        action: () => void this.deleteProduct(row),
      });
    }

    if (row.acceptsResume) {
      actions.push({
        label: this._localization.translate(
          'modules.profile.productListings.meta.resume'
        ),
        icon: 'heroicons_outline:document-text',
        iconClass: 'text-[#35507c]',
        action: () => this.openResumes(row),
      });
    }

    return actions;
  }

  async deleteProduct(row: ProductListItemDto): Promise<void> {
    if (!this.canDelete) return;
    const message = this._localization.translate(
      'modules.admin.products.confirmDelete',
      {
        name: row.title,
      }
    );
    if (!confirm(message)) return;
    const result = await this._catalogService.deleteProduct(row.productId);
    if (!result.success) {
      this.error =
        result.message ??
        this._localization.translate(
          'modules.admin.products.errors.deleteFailed'
        );
      return;
    }
    await this.load();
  }

  featuredImageUrl(row: ProductListItemDto): string {
    const path = row.featuredImagePath?.trim();
    if (!path) return '';
    return this._catalogService.getProductImageUrl(path);
  }

  statusLabel(status: ProductStatus): string {
    const s = String(status);
    switch (s) {
      case 'PendingApproval':
      case '1':
        return this._localization.translate(
          'modules.admin.products.status.pendingApproval'
        );
      case 'Approved':
      case '2':
        return this._localization.translate(
          'modules.admin.products.status.approved'
        );
      case 'Rejected':
      case '3':
        return this._localization.translate(
          'modules.admin.products.status.rejected'
        );
      case 'Unpublished':
      case '4':
        return this._localization.translate(
          'modules.admin.products.status.unpublished'
        );
      default:
        return this._localization.translate(
          'modules.admin.products.status.draft'
        );
    }
  }

  statusBadgeClass(status: ProductStatus): string {
    const s = String(status);
    switch (s) {
      case 'Approved':
      case '2':
        return 'bg-emerald-50 text-emerald-700 ring-emerald-200';
      case 'PendingApproval':
      case '1':
        return 'bg-amber-50 text-amber-700 ring-amber-200';
      case 'Rejected':
      case '3':
        return 'bg-rose-50 text-rose-700 ring-rose-200';
      case 'Unpublished':
      case '4':
        return 'bg-slate-100 text-slate-600 ring-slate-200';
      default:
        return 'bg-violet-50 text-violet-700 ring-violet-200';
    }
  }

  statusDotClass(status: ProductStatus): string {
    const s = String(status);
    switch (s) {
      case 'Approved':
      case '2':
        return 'bg-emerald-500';
      case 'PendingApproval':
      case '1':
        return 'bg-amber-500';
      case 'Rejected':
      case '3':
        return 'bg-rose-500';
      case 'Unpublished':
      case '4':
        return 'bg-slate-400';
      default:
        return 'bg-violet-500';
    }
  }

  formatPrice(row: ProductListItemDto): string {
    if (row.acceptsResume) {
      return this._localization.translate(
        'modules.profile.productListings.meta.resume'
      );
    }
    if (row.isNegotiablePrice) {
      return this._localization.translate(
        'modules.profile.productListings.meta.negotiablePrice'
      );
    }
    const price = row.price ?? 0;
    const discount = row.discountPercent ?? 0;
    const final =
      discount > 0 ? Math.round(price * (1 - Number(discount) / 100)) : price;
    return new Intl.NumberFormat('fa-IR').format(final);
  }

  formatStock(row: ProductListItemDto): string {
    return new Intl.NumberFormat('fa-IR').format(row.stockQuantity ?? 0);
  }

  formatJalaliDate(value?: string | null): string {
    if (!value?.trim()) return '—';
    const parsed = jMoment(value).locale('fa');
    return parsed.isValid() ? parsed.format('jYYYY/jMM/jDD') : value.trim();
  }

  private toIsoDate(value: Moment | null, endOfDay: boolean): string | null {
    if (!value?.isValid()) return null;
    const date = value.clone().locale('fa');
    if (endOfDay) {
      date.hours(23).minutes(59).seconds(59).milliseconds(999);
    } else {
      date.hours(0).minutes(0).seconds(0).milliseconds(0);
    }
    return date.toISOString();
  }
}
