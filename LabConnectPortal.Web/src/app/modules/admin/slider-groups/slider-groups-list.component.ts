import { Component, DestroyRef, OnInit, ViewChild, inject } from '@angular/core';
import { FormControl, ReactiveFormsModule } from '@angular/forms';
import { MatIconModule } from '@angular/material/icon';
import { MatDrawer, MatSidenavModule } from '@angular/material/sidenav';
import { Router, RouterLink } from '@angular/router';
import { TranslocoPipe } from '@jsverse/transloco';
import jMoment from 'moment-jalaali';
import { LocalizationService } from '@core/services/localization/localization.service';
import { BaseButtonComponent } from '@modules/base/components/base-button/base-button.component';
import {
  BaseFormFieldComponent,
  BaseFormSelectOption,
} from '@modules/base/components/base-form-field/base-form-field.component';
import { BaseGridComponent } from '@modules/base/components/base-grid/base-grid.component';
import { BasePagingComponent } from '@modules/base/components/base-paging/base-paging.component';
import { SliderGroupsService } from './slider-groups.service';
import { SliderGroupListItemDto } from './slider-groups.types';
import { BehaviorSubject } from 'rxjs';
import { ColDef } from 'ag-grid-community';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { SliderGroupsColDef } from './slider-groups.coldef';
import { SystemEntity } from '@core/system-entity/system-entity';
import { bindSystemEntity } from '@core/system-entity/bind-system-entity';
import { SitePermissionService } from '@core/services/auth/site-permission.service';

@Component({
  selector: 'app-slider-groups-list',
  standalone: true,
  imports: [
    ReactiveFormsModule,
    RouterLink,
    MatIconModule,
    MatSidenavModule,
    TranslocoPipe,
    BaseButtonComponent,
    BaseFormFieldComponent,
    BasePagingComponent,
    BaseGridComponent,
  ],
  templateUrl: './slider-groups-list.component.html',
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
export class SliderGroupsListComponent implements OnInit {
  readonly entity = SystemEntity.SliderGroup;
  private readonly _systemEntityBinding = bindSystemEntity(SystemEntity.SliderGroup);

  @ViewChild('filterDrawer') filterDrawer?: MatDrawer;
  private readonly _destroyRef = inject(DestroyRef);
  loading = false;
  error = '';
  rows: SliderGroupListItemDto[] = [];
  colDef: ColDef[] = [];
  readonly list$ = new BehaviorSubject<any[]>([]);
  totalCount = 0;
  page = 1;
  pageSize = 20;

  titleControl = new FormControl('');
  activeControl = new FormControl<string>('all');

  get statusFilterOptions(): BaseFormSelectOption[] {
    return [
      { value: 'all', label: this._localization.translate('shared.all') },
      { value: 'active', label: this._localization.translate('modules.admin.sliderGroups.active') },
      { value: 'inactive', label: this._localization.translate('modules.admin.sliderGroups.inactive') },
    ];
  }

  constructor(
    private _sliderGroupsService: SliderGroupsService,
    private _localization: LocalizationService,
    private _router: Router,
    private _colDef: SliderGroupsColDef,
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
          void this._router.navigate(['/admin/slider-groups', evt.row.id]);
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
    const result = await this._sliderGroupsService.getAll({
      title: this.titleControl.value || undefined,
      isActive:
        this.activeControl.value === 'all'
          ? undefined
          : this.activeControl.value === 'active',
      page: this.page,
      pageSize: this.pageSize,
    });
    if (result.success && result.data) {
      this.rows = result.data.items;
      this.totalCount = result.data.totalCount;
      this.list$.next(
        this.rows.map((r) => ({
          ...r,
          datesText: `${this.formatJalaliDate(r.startDate)} تا ${this.formatJalaliDate(r.endDate)}`,
          statusText: this._localization.translate(
            r.isActive ? 'modules.admin.sliderGroups.active' : 'modules.admin.sliderGroups.inactive',
          ),
        })),
      );
    } else {
      this.error = result.message ?? this._localization.translate('modules.admin.sliderGroups.errors.loadFailed');
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

  formatJalaliDate(value: string): string {
    if (!value?.trim()) return '-';
    const parsed = jMoment(value).locale('fa');
    return parsed.isValid() ? parsed.format('jYYYY/jMM/jDD') : value.trim();
  }

  onPageChange(page: number): void {
    this.page = page;
    void this.load();
  }

  async deleteGroup(row: SliderGroupListItemDto): Promise<void> {
    if (!this.canDelete) return;
    const message = this._localization.translate('modules.admin.sliderGroups.confirmDelete', { title: row.title });
    if (!confirm(message)) return;

    const result = await this._sliderGroupsService.delete(row.id);
    if (!result.success) {
      this.error = result.message ?? this._localization.translate('modules.admin.sliderGroups.errors.deleteFailed');
      return;
    }
    await this.load();
  }
}
