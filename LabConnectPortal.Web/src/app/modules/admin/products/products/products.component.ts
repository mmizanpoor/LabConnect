import { Component, DestroyRef, OnInit, ViewChild, inject } from '@angular/core';
import { FormControl, ReactiveFormsModule } from '@angular/forms';
import { MatIconModule } from '@angular/material/icon';
import { MatDrawer, MatSidenavModule } from '@angular/material/sidenav';
import { Router } from '@angular/router';
import jMoment from 'moment-jalaali';
import { TranslocoPipe } from '@jsverse/transloco';
import { LocalizationService } from '@core/services/localization/localization.service';
import { BaseButtonComponent } from '@modules/base/components/base-button/base-button.component';
import {
  BaseFormFieldComponent,
  BaseFormSelectOption,
} from '@modules/base/components/base-form-field/base-form-field.component';
import { BaseGridComponent } from '@modules/base/components/base-grid/base-grid.component';
import { BasePagingComponent } from '@modules/base/components/base-paging/base-paging.component';
import { ProductCatalogService } from '../product-catalog.service';
import { BrandDto, ProductCategoryDto, ProductListItemDto, ProductStatus } from '../product-catalog.types';
import { BehaviorSubject } from 'rxjs';
import { ColDef } from 'ag-grid-community';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { ProductsColDef } from './products.coldef';
import { SystemEntity } from '@core/system-entity/system-entity';
import { bindSystemEntity } from '@core/system-entity/bind-system-entity';
import { SitePermissionService } from '@core/services/auth/site-permission.service';

interface ProductRow extends ProductListItemDto {
  categoriesText: string;
}

@Component({
  selector: 'app-products',
  standalone: true,
  imports: [
    ReactiveFormsModule,
    MatIconModule,
    MatSidenavModule,
    TranslocoPipe,
    BaseButtonComponent,
    BaseFormFieldComponent,
    BasePagingComponent,
    BaseGridComponent,
  ],
  templateUrl: './products.component.html',
  styles: `
    :host {
      display: block;
      height: 100%;
      min-height: 0;
    }

    :host ::ng-deep .mat-drawer-container {
      height: 100%;
    }
  `,
})
export class ProductsComponent implements OnInit {
  readonly entity = SystemEntity.Product;
  private readonly _systemEntityBinding = bindSystemEntity(SystemEntity.Product);

  @ViewChild('filterDrawer') filterDrawer?: MatDrawer;
  private readonly _destroyRef = inject(DestroyRef);
  loading = false;
  error = '';
  rows: ProductRow[] = [];
  colDef: ColDef[] = [];
  readonly list$ = new BehaviorSubject<ProductRow[]>([]);
  brands: BrandDto[] = [];
  categories: ProductCategoryDto[] = [];
  totalCount = 0;
  page = 1;
  pageSize = 20;

  titleControl = new FormControl('');
  createdByUserNameControl = new FormControl('');
  createdByCenterNameControl = new FormControl('');
  brandControl = new FormControl<number | null>(null);
  categoryControl = new FormControl<number | null>(null);
  statusControl = new FormControl<ProductStatus | null>(null);

  constructor(
    private _catalogService: ProductCatalogService,
    private _router: Router,
    private _localization: LocalizationService,
    private _colDef: ProductsColDef,
    private _sitePermission: SitePermissionService,
  ) {}

  get canCreate(): boolean {
    // Site admin only moderates/edits products created by centers.
    return false;
  }

  get canUpdate(): boolean {
    return this._sitePermission.can(this.entity, 'update');
  }

  get statusFilterOptions(): BaseFormSelectOption[] {
    return [
      { value: null, label: this._localization.translate('shared.all') },
      { value: 'Draft', label: this._localization.translate('modules.admin.products.status.draft') },
      {
        value: 'PendingApproval',
        label: this._localization.translate('modules.admin.products.status.pendingApproval'),
      },
      { value: 'Approved', label: this._localization.translate('modules.admin.products.status.approved') },
      { value: 'Rejected', label: this._localization.translate('modules.admin.products.status.rejected') },
      {
        value: 'Unpublished',
        label: this._localization.translate('modules.admin.products.status.unpublished'),
      },
    ];
  }

  get brandFilterOptions(): BaseFormSelectOption[] {
    return [
      { value: null, label: this._localization.translate('shared.all') },
      ...this.brands.map((brand) => ({ value: brand.brandId, label: brand.title })),
    ];
  }

  get categoryFilterOptions(): BaseFormSelectOption[] {
    return [
      { value: null, label: this._localization.translate('shared.all') },
      ...this.categories.map((category) => ({
        value: category.productCategoryId,
        label: category.title,
      })),
    ];
  }

  ngOnInit(): void {
    this.colDef = this._colDef.get((v) => this.formatJalaliDate(v), {
      showReviewAction: true,
      canUpdate: this.canUpdate,
      // Admin moderates via review/edit; product creation/deletion belongs to centers.
      canDelete: false,
      mutateScope: 'all',
    });
    this._colDef.actionClicked
      .pipe(takeUntilDestroyed(this._destroyRef))
      .subscribe((evt) => {
        if (!evt?.row) return;
        if (evt.type === 'review') {
          this.openReview(evt.row);
        }
        if (evt.type === 'edit') {
          if (!this.canUpdate) return;
          this.openEdit(evt.row);
        }
      });
    void this.load();
  }

  async load(): Promise<void> {
    this.loading = true;
    this.error = '';
    try {
      const [productsResult, brandsResult, categoriesResult] = await Promise.all([
        this._catalogService.getProducts({
          title: this.titleControl.value || undefined,
          createdByUserName: this.createdByUserNameControl.value || undefined,
          createdByCenterName: this.createdByCenterNameControl.value || undefined,
          brandId: this.brandControl.value ?? undefined,
          categoryId: this.categoryControl.value ?? undefined,
          status: this.statusControl.value ?? undefined,
          page: this.page,
          pageSize: this.pageSize,
        }),
        this._catalogService.getBrandOptions(),
        this._catalogService.getCategoryOptions(),
      ]);

      if (!productsResult.success || !productsResult.data) {
        throw new Error(
          productsResult.message ?? this._localization.translate('modules.admin.products.errors.loadFailed'),
        );
      }

      this.rows = productsResult.data.items.map((item) => ({
        ...item,
        categoriesText: item.categoryTitles.join('، '),
      }));
      this.totalCount = productsResult.data.totalCount;
      this.list$.next(this.rows);

      if (brandsResult.success && brandsResult.data) {
        this.brands = brandsResult.data;
      }
      if (categoriesResult.success && categoriesResult.data) {
        this.categories = categoriesResult.data;
      }
    } catch (e: unknown) {
      this.error =
        e instanceof Error ? e.message : this._localization.translate('modules.admin.products.errors.loadFailed');
      this.rows = [];
      this.list$.next([]);
    } finally {
      this.loading = false;
    }
  }

  applyFilters(): void {
    this.page = 1;
    void this.filterDrawer?.close();
    void this.load();
  }

  onPageChange(page: number): void {
    this.page = page;
    void this.load();
  }

  formatJalaliDate(value?: string | null): string {
    if (!value?.trim()) return '—';
    const parsed = jMoment(value).locale('fa');
    return parsed.isValid() ? parsed.format('jYYYY/jMM/jDD') : value.trim();
  }

  openCreate(): void {
    return;
  }

  openEdit(row: ProductListItemDto): void {
    if (!this.canUpdate) return;
    void this._router.navigate(['/admin/products', row.productId, 'edit']);
  }

  openReview(row: ProductListItemDto): void {
    void this._router.navigate(['/admin/products', row.productId]);
  }
}
