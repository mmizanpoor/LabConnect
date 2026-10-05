import { Component, DestroyRef, OnInit, ViewChild, inject } from '@angular/core';
import { FormControl, ReactiveFormsModule } from '@angular/forms';
import { MatDialog, MatDialogModule } from '@angular/material/dialog';
import { MatIconModule } from '@angular/material/icon';
import { MatDrawer, MatSidenavModule } from '@angular/material/sidenav';
import { TranslocoPipe } from '@jsverse/transloco';
import { LocalizationService } from '@core/services/localization/localization.service';
import { BaseButtonComponent } from '@modules/base/components/base-button/base-button.component';
import { BaseFormFieldComponent } from '@modules/base/components/base-form-field/base-form-field.component';
import { BaseGridComponent } from '@modules/base/components/base-grid/base-grid.component';
import { BasePagingComponent } from '@modules/base/components/base-paging/base-paging.component';
import { BASE_DIALOG_PANEL_CLASS } from '@modules/base/components/base-dialog/base-dialog.component';
import { ContentService } from '../content.service';
import { ContentGroupDto } from '../content.types';
import { ContentGroupFormDialogComponent } from './content-group-form-dialog.component';
import { BehaviorSubject } from 'rxjs';
import { ColDef } from 'ag-grid-community';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { ContentGroupsColDef } from './content-groups.coldef';
import { SystemEntity } from '@core/system-entity/system-entity';
import { bindSystemEntity } from '@core/system-entity/bind-system-entity';
import { SitePermissionService } from '@core/services/auth/site-permission.service';

@Component({
  selector: 'app-content-groups',
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
  templateUrl: './content-groups.component.html',
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
export class ContentGroupsComponent implements OnInit {
  readonly entity = SystemEntity.ContentGroup;
  private readonly _systemEntityBinding = bindSystemEntity(SystemEntity.ContentGroup);

  @ViewChild('filterDrawer') filterDrawer?: MatDrawer;
  private readonly _destroyRef = inject(DestroyRef);
  loading = false;
  error = '';
  rows: ContentGroupDto[] = [];
  colDef: ColDef[] = [];
  readonly list$ = new BehaviorSubject<ContentGroupDto[]>([]);
  totalCount = 0;
  page = 1;
  pageSize = 20;

  titleControl = new FormControl('');

  constructor(
    private _contentService: ContentService,
    private _dialog: MatDialog,
    private _localization: LocalizationService,
    private _colDef: ContentGroupsColDef,
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
          void this.deleteGroup(evt.row);
        }
      });
    void this.load();
  }

  async load(): Promise<void> {
    this.loading = true;
    this.error = '';
    const result = await this._contentService.getGroups({
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
        result.message ??
        this._localization.translate(
          'modules.admin.contentGroups.errors.loadFailed'
        );
      this.rows = [];
      this.list$.next([]);
    }
    this.loading = false;
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
    const ref = this._dialog.open(ContentGroupFormDialogComponent, {
      width: '420px',
      panelClass: BASE_DIALOG_PANEL_CLASS,
      data: {},
    });
    ref.afterClosed().subscribe((saved) => {
      if (saved) void this.load();
    });
  }

  openEdit(group: ContentGroupDto): void {
    if (!this.canUpdate) return;
    const ref = this._dialog.open(ContentGroupFormDialogComponent, {
      width: '420px',
      panelClass: BASE_DIALOG_PANEL_CLASS,
      data: { group },
    });
    ref.afterClosed().subscribe((saved) => {
      if (saved) void this.load();
    });
  }

  async deleteGroup(group: ContentGroupDto): Promise<void> {
    if (!this.canDelete) return;
    const message = this._localization.translate(
      'modules.admin.contentGroups.confirmDelete',
      {
        title: group.title,
      }
    );
    if (!confirm(message)) return;

    const result = await this._contentService.deleteGroup(group.contentGroupId);
    if (!result.success) {
      this.error =
        result.message ??
        this._localization.translate(
          'modules.admin.contentGroups.errors.deleteFailed'
        );
      return;
    }
    await this.load();
  }
}
