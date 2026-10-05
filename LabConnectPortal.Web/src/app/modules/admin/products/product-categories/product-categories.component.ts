import { Component, DestroyRef, OnInit, ViewChild, inject } from '@angular/core';
import { FormControl, ReactiveFormsModule } from '@angular/forms';
import { MatDialog, MatDialogModule } from '@angular/material/dialog';
import { MatIconModule } from '@angular/material/icon';
import { MatDrawer, MatSidenavModule } from '@angular/material/sidenav';
import { MatTabsModule } from '@angular/material/tabs';
import { TranslocoPipe } from '@jsverse/transloco';
import { LocalizationService } from '@core/services/localization/localization.service';
import { BaseButtonComponent } from '@modules/base/components/base-button/base-button.component';
import { BaseFormFieldComponent } from '@modules/base/components/base-form-field/base-form-field.component';
import { BaseGridComponent } from '@modules/base/components/base-grid/base-grid.component';
import { BasePagingComponent } from '@modules/base/components/base-paging/base-paging.component';
import { BASE_DIALOG_PANEL_CLASS } from '@modules/base/components/base-dialog/base-dialog.component';
import { ProductCatalogService } from '../product-catalog.service';
import { ProductCategoryDto, ProductCategoryGroupDto } from '../product-catalog.types';
import { CategoryFormDialogComponent } from './category-form-dialog.component';
import { CategoryGroupFormDialogComponent } from './category-group-form-dialog.component';
import { BehaviorSubject } from 'rxjs';
import { ColDef } from 'ag-grid-community';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { ProductCategoriesColDef } from './product-categories.coldef';
import { ProductCategoryGroupsColDef } from './category-groups.coldef';
import { SystemEntity } from '@core/system-entity/system-entity';
import { bindSystemEntity } from '@core/system-entity/bind-system-entity';
import { SitePermissionService } from '@core/services/auth/site-permission.service';

type ActiveTab = 'categories' | 'groups';

@Component({
  selector: 'app-product-categories',
  standalone: true,
  imports: [
    ReactiveFormsModule,
    MatDialogModule,
    MatIconModule,
    MatSidenavModule,
    MatTabsModule,
    TranslocoPipe,
    BaseButtonComponent,
    BaseFormFieldComponent,
    BasePagingComponent,
    BaseGridComponent,
  ],
  templateUrl: './product-categories.component.html',
  styles: `
    :host {
      display: block;
      height: 100%;
      min-height: 0;
    }

    :host ::ng-deep .mat-drawer-container {
      height: 100%;
    }

    :host ::ng-deep .mat-mdc-tab-body-wrapper,
    :host ::ng-deep .mat-mdc-tab-body,
    :host ::ng-deep .mat-mdc-tab-body-content {
      height: 100%;
    }

    :host ::ng-deep .mat-mdc-tab-group {
      display: flex;
      flex-direction: column;
      height: 100%;
      min-height: 0;
    }

    :host ::ng-deep .mat-mdc-tab-header {
      flex-shrink: 0;
    }
  `,
})
export class ProductCategoriesComponent implements OnInit {
  readonly entity = SystemEntity.ProductCategory;
  private readonly _systemEntityBinding = bindSystemEntity(SystemEntity.ProductCategory);

  @ViewChild('filterDrawer') filterDrawer?: MatDrawer;
  @ViewChild('groupFilterDrawer') groupFilterDrawer?: MatDrawer;
  private readonly _destroyRef = inject(DestroyRef);

  activeTab: ActiveTab = 'categories';
  loading = false;
  groupsLoading = false;
  error = '';
  groupsError = '';

  rows: ProductCategoryDto[] = [];
  colDef: ColDef[] = [];
  readonly list$ = new BehaviorSubject<ProductCategoryDto[]>([]);
  totalCount = 0;
  page = 1;
  pageSize = 20;
  titleControl = new FormControl('');

  groupRows: ProductCategoryGroupDto[] = [];
  groupColDef: ColDef[] = [];
  readonly groupList$ = new BehaviorSubject<ProductCategoryGroupDto[]>([]);
  groupTotalCount = 0;
  groupPage = 1;
  groupPageSize = 20;
  groupNameControl = new FormControl('');
  private groupsLoaded = false;

  constructor(
    private _catalogService: ProductCatalogService,
    private _dialog: MatDialog,
    private _localization: LocalizationService,
    private _colDef: ProductCategoriesColDef,
    private _groupColDef: ProductCategoryGroupsColDef,
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
    this.groupColDef = this._groupColDef.get();

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
          void this.deleteCategory(evt.row);
        }
      });

    this._groupColDef.actionClicked
      .pipe(takeUntilDestroyed(this._destroyRef))
      .subscribe((evt) => {
        if (!evt?.row) return;
        if (evt.type === 'edit') {
          if (!this.canUpdate) return;
          this.openEditGroup(evt.row);
        }
        if (evt.type === 'delete') {
          if (!this.canDelete) return;
          void this.deleteGroup(evt.row);
        }
      });

    void this.load();
  }

  onTabChange(index: number): void {
    this.activeTab = index === 1 ? 'groups' : 'categories';
    if (this.activeTab === 'groups' && !this.groupsLoaded && !this.groupsLoading) {
      void this.loadGroups();
    }
  }

  async load(): Promise<void> {
    this.loading = true;
    this.error = '';
    const result = await this._catalogService.getCategories({
      title: this.titleControl.value || undefined,
      page: this.page,
      pageSize: this.pageSize,
    });
    if (result.success && result.data) {
      this.rows = result.data.items;
      this.totalCount = result.data.totalCount;
      this.list$.next(this.rows);
    } else {
      this.error =
        result.message ?? this._localization.translate('modules.admin.productCategories.errors.loadFailed');
      this.rows = [];
      this.list$.next([]);
    }
    this.loading = false;
  }

  async loadGroups(): Promise<void> {
    this.groupsLoading = true;
    this.groupsError = '';
    const result = await this._catalogService.getCategoryGroups({
      name: this.groupNameControl.value || undefined,
      page: this.groupPage,
      pageSize: this.groupPageSize,
    });
    if (result.success && result.data) {
      this.groupRows = result.data.items;
      this.groupTotalCount = result.data.totalCount;
      this.groupList$.next(this.groupRows);
    } else {
      this.groupsError =
        result.message ??
        this._localization.translate(
          'modules.admin.productCategories.categoryGroups.errors.loadFailed',
        );
      this.groupRows = [];
      this.groupList$.next([]);
    }
    this.groupsLoading = false;
    this.groupsLoaded = true;
  }

  applyFilters(): void {
    this.page = 1;
    void this.filterDrawer?.close();
    void this.load();
  }

  applyGroupFilters(): void {
    this.groupPage = 1;
    void this.groupFilterDrawer?.close();
    void this.loadGroups();
  }

  onPageChange(page: number): void {
    this.page = page;
    void this.load();
  }

  onGroupPageChange(page: number): void {
    this.groupPage = page;
    void this.loadGroups();
  }

  openCreate(): void {
    if (!this.canCreate) return;
    const ref = this._dialog.open(CategoryFormDialogComponent, {
      width: '420px',
      panelClass: BASE_DIALOG_PANEL_CLASS,
      data: {},
    });
    ref.afterClosed().subscribe((saved) => {
      if (saved) void this.load();
    });
  }

  openEdit(category: ProductCategoryDto): void {
    if (!this.canUpdate) return;
    const ref = this._dialog.open(CategoryFormDialogComponent, {
      width: '420px',
      panelClass: BASE_DIALOG_PANEL_CLASS,
      data: { category },
    });
    ref.afterClosed().subscribe((saved) => {
      if (saved) void this.load();
    });
  }

  async deleteCategory(category: ProductCategoryDto): Promise<void> {
    if (!this.canDelete) return;
    const message = this._localization.translate('modules.admin.productCategories.confirmDelete', {
      title: category.title,
    });
    if (!confirm(message)) return;

    const result = await this._catalogService.deleteCategory(category.productCategoryId);
    if (!result.success) {
      this.error =
        result.message ?? this._localization.translate('modules.admin.productCategories.errors.deleteFailed');
      return;
    }
    await this.load();
  }

  openCreateGroup(): void {
    if (!this.canCreate) return;
    const ref = this._dialog.open(CategoryGroupFormDialogComponent, {
      width: '480px',
      panelClass: BASE_DIALOG_PANEL_CLASS,
      data: {},
    });
    ref.afterClosed().subscribe((saved) => {
      if (saved) void this.loadGroups();
    });
  }

  openEditGroup(group: ProductCategoryGroupDto): void {
    if (!this.canUpdate) return;
    const ref = this._dialog.open(CategoryGroupFormDialogComponent, {
      width: '480px',
      panelClass: BASE_DIALOG_PANEL_CLASS,
      data: { group },
    });
    ref.afterClosed().subscribe((saved) => {
      if (saved) void this.loadGroups();
    });
  }

  async deleteGroup(group: ProductCategoryGroupDto): Promise<void> {
    if (!this.canDelete) return;
    const message = this._localization.translate(
      'modules.admin.productCategories.categoryGroups.confirmDelete',
      { name: group.name },
    );
    if (!confirm(message)) return;

    const result = await this._catalogService.deleteCategoryGroup(group.productCategoryGroupId);
    if (!result.success) {
      this.groupsError =
        result.message ??
        this._localization.translate(
          'modules.admin.productCategories.categoryGroups.errors.deleteFailed',
        );
      return;
    }
    await this.loadGroups();
  }
}
