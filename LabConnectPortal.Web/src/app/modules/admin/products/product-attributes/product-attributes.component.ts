import { Component, DestroyRef, OnInit, ViewChild, inject } from '@angular/core';
import { FormControl, ReactiveFormsModule } from '@angular/forms';
import { MatDialog, MatDialogModule } from '@angular/material/dialog';
import { MatIconModule } from '@angular/material/icon';
import { MatDrawer, MatSidenavModule } from '@angular/material/sidenav';
import { TranslocoPipe } from '@jsverse/transloco';
import { LocalizationService } from '@core/services/localization/localization.service';
import { BaseButtonComponent } from '@modules/base/components/base-button/base-button.component';
import {
  BaseFormFieldComponent,
  BaseFormSelectOption,
} from '@modules/base/components/base-form-field/base-form-field.component';
import { BaseGridComponent } from '@modules/base/components/base-grid/base-grid.component';
import { BasePagingComponent } from '@modules/base/components/base-paging/base-paging.component';
import { BASE_DIALOG_PANEL_CLASS } from '@modules/base/components/base-dialog/base-dialog.component';
import { ProductCatalogService } from '../product-catalog.service';
import { ProductAttributeDto, ProductCategoryDto } from '../product-catalog.types';
import { AttributeFormDialogComponent } from './attribute-form-dialog.component';
import { BehaviorSubject } from 'rxjs';
import { ColDef } from 'ag-grid-community';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { ProductAttributesColDef } from './product-attributes.coldef';
import { SystemEntity } from '@core/system-entity/system-entity';
import { bindSystemEntity } from '@core/system-entity/bind-system-entity';
import { SitePermissionService } from '@core/services/auth/site-permission.service';

@Component({
  selector: 'app-product-attributes',
  standalone: true,
  imports: [
    ReactiveFormsModule,
    MatDialogModule,
    MatIconModule,
    MatSidenavModule,
    TranslocoPipe,
    BaseButtonComponent,
    BaseFormFieldComponent,
    BasePagingComponent,
    BaseGridComponent,
  ],
  templateUrl: './product-attributes.component.html',
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
export class ProductAttributesComponent implements OnInit {
  readonly entity = SystemEntity.ProductAttribute;
  private readonly _systemEntityBinding = bindSystemEntity(SystemEntity.ProductAttribute);

  @ViewChild('filterDrawer') filterDrawer?: MatDrawer;
  private readonly _destroyRef = inject(DestroyRef);
  loading = false;
  error = '';
  rows: ProductAttributeDto[] = [];
  colDef: ColDef[] = [];
  readonly list$ = new BehaviorSubject<ProductAttributeDto[]>([]);
  categories: ProductCategoryDto[] = [];
  totalCount = 0;
  page = 1;
  pageSize = 20;

  categoryControl = new FormControl<number | null>(null);
  titleControl = new FormControl('');

  constructor(
    private _catalogService: ProductCatalogService,
    private _dialog: MatDialog,
    private _localization: LocalizationService,
    private _colDef: ProductAttributesColDef,
    private _sitePermission: SitePermissionService,
  ) {}

  get canCreate(): boolean {
    return this._sitePermission.can(this.entity, 'create');
  }

  get canUpdate(): boolean {
    return this._sitePermission.can(this.entity, 'update');
  }

  get canDelete(): boolean {
    return this._sitePermission.can(this.entity, 'delete');
  }

  ngOnInit(): void {
    this.colDef = this._colDef.get();
    this._colDef.actionClicked
      .pipe(takeUntilDestroyed(this._destroyRef))
      .subscribe((evt) => {
        if (!evt?.row) return;
        if (evt.type === 'edit') {
          if (!this.canUpdate) return;
          this.openEdit(evt.row);
        }
        if (evt.type === 'delete') {
          if (!this.canDelete) return;
          void this.deleteAttribute(evt.row);
        }
      });
    void this.load();
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

  async load(): Promise<void> {
    this.loading = true;
    this.error = '';
    try {
      const [attributesResult, categoriesResult] = await Promise.all([
        this._catalogService.getAttributes({
          productCategoryId: this.categoryControl.value ?? undefined,
          title: this.titleControl.value || undefined,
          page: this.page,
          pageSize: this.pageSize,
        }),
        this._catalogService.getCategoryOptions(),
      ]);

      if (!attributesResult.success || !attributesResult.data) {
        throw new Error(
          attributesResult.message ??
            this._localization.translate('modules.admin.productAttributes.errors.loadFailed'),
        );
      }

      this.rows = attributesResult.data.items;
      this.totalCount = attributesResult.data.totalCount;
      this.list$.next(this.rows);

      if (categoriesResult.success && categoriesResult.data) {
        this.categories = categoriesResult.data;
      }
    } catch (e: unknown) {
      this.error =
        e instanceof Error
          ? e.message
          : this._localization.translate('modules.admin.productAttributes.errors.loadFailed');
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

  openCreate(): void {
    if (!this.canCreate) return;
    const ref = this._dialog.open(AttributeFormDialogComponent, {
      width: '420px',
      panelClass: BASE_DIALOG_PANEL_CLASS,
      data: { categories: this.categories },
    });
    ref.afterClosed().subscribe((saved) => {
      if (saved) void this.load();
    });
  }

  openEdit(attribute: ProductAttributeDto): void {
    if (!this.canUpdate) return;
    const ref = this._dialog.open(AttributeFormDialogComponent, {
      width: '420px',
      panelClass: BASE_DIALOG_PANEL_CLASS,
      data: { categories: this.categories, attribute },
    });
    ref.afterClosed().subscribe((saved) => {
      if (saved) void this.load();
    });
  }

  async deleteAttribute(attribute: ProductAttributeDto): Promise<void> {
    if (!this.canDelete) return;
    const message = this._localization.translate('modules.admin.productAttributes.confirmDelete', {
      name: attribute.title,
    });
    if (!confirm(message)) return;

    const result = await this._catalogService.deleteAttribute(attribute.productAttributeId);
    if (!result.success) {
      this.error =
        result.message ?? this._localization.translate('modules.admin.productAttributes.errors.deleteFailed');
      return;
    }
    await this.load();
  }
}
