import { Component, OnInit, ViewChild } from '@angular/core';
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
import { UsersColDef } from './users.coldef';
import { UsersService } from './users.service';
import { UserDto, UserType } from './users.types';
import { UserDetailComponent } from './user-detail/user-detail.component';
import { UserResumeDialogComponent } from './user-resume-dialog/user-resume-dialog.component';
import { BehaviorSubject } from 'rxjs';
import { ColDef, RowClickedEvent } from 'ag-grid-community';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { SystemEntity } from '@core/system-entity/system-entity';
import { bindSystemEntity } from '@core/system-entity/bind-system-entity';

interface UserRow extends UserDto {
  fullName: string;
  userTypeText: string;
  isActiveText: string;
}

const USER_TYPE_FILTER_OPTIONS: UserType[] = [
  'User',
  'UserLab',
  'Store',
  'AdminLab',
  'Administrator',
  'Admin',
];

@Component({
  selector: 'app-users',
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
  templateUrl: './users.component.html',
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
export class UsersComponent implements OnInit {
  readonly entity = SystemEntity.User;
  private readonly _systemEntityBinding = bindSystemEntity(SystemEntity.User);

  @ViewChild('filterDrawer') filterDrawer?: MatDrawer;
  colDef: ColDef[] = [];
  readonly list$ = new BehaviorSubject<UserRow[]>([]);
  rows: UserRow[] = [];
  totalCount = 0;
  page = 1;
  pageSize = 20;
  loading = false;
  userTypeFilterOptions = USER_TYPE_FILTER_OPTIONS;

  searchControl = new FormControl('');
  userTypeControl = new FormControl<UserType | ''>('');
  activeControl = new FormControl<'all' | 'active' | 'inactive'>('all');

  constructor(
    private _usersService: UsersService,
    private _dialog: MatDialog,
    private _localization: LocalizationService,
    private _colDef: UsersColDef
  ) {
    this._colDef.actionClicked.pipe(takeUntilDestroyed()).subscribe((evt) => {
      if (!evt?.row) return;
      if (evt.type === 'detail') this.openDetail(evt.row);
      if (evt.type === 'resume') this.openResume(evt.row);
      if (evt.type === 'delete') void this.deleteUser(evt.row);
    });
  }

  ngOnInit(): void {
    this.colDef = this._colDef.get() as any;
    this.load();
  }

  get userTypeFilterSelectOptions(): BaseFormSelectOption[] {
    return [
      { value: '', label: this._localization.translate('shared.all') },
      ...this.userTypeFilterOptions.map((userType) => ({
        value: userType,
        label: this.userTypeLabel(userType),
      })),
    ];
  }

  get statusFilterSelectOptions(): BaseFormSelectOption[] {
    return [
      { value: 'all', label: this._localization.translate('shared.all') },
      { value: 'active', label: this._localization.translate('shared.active') },
      {
        value: 'inactive',
        label: this._localization.translate('shared.inactive'),
      },
    ];
  }

  applyFilters(): void {
    this.page = 1;
    void this.filterDrawer?.close();
    void this.load();
  }

  async load(): Promise<void> {
    this.loading = true;
    const isActive =
      this.activeControl.value === 'active'
        ? true
        : this.activeControl.value === 'inactive'
        ? false
        : undefined;

    const result = await this._usersService.getUsers({
      search: this.searchControl.value || undefined,
      userType: this.userTypeControl.value || undefined,
      isActive,
      page: this.page,
      pageSize: this.pageSize,
    });

    if (result.success && result.data) {
      this.rows = result.data.items.map((u) => ({
        ...u,
        fullName: `${u.firstName} ${u.lastName}`.trim(),
        userTypeText: this.translateUserType(u.userType),
        isActiveText: u.isActive
          ? this._localization.translate('shared.active')
          : this._localization.translate('shared.inactive'),
      }));
      this.totalCount = result.data.totalCount;
      this.list$.next(this.rows);
    } else {
      this.rows = [];
      this.list$.next([]);
    }
    this.loading = false;
  }

  onPageChange(page: number): void {
    this.page = page;
    this.load();
  }

  onRowClicked(event: RowClickedEvent<UserRow>): void {
    if (event.data) this.openDetail(event.data);
  }

  openDetail(user: UserDto): void {
    const ref = this._dialog.open(UserDetailComponent, {
      data: { user },
      width: '560px',
      maxWidth: '95vw',
      panelClass: BASE_DIALOG_PANEL_CLASS,
    });
    ref.afterClosed().subscribe((changed) => {
      if (changed) this.load();
    });
  }

  openResume(user: UserRow): void {
    if (!this.canViewResume(user)) return;
    this._dialog.open(UserResumeDialogComponent, {
      data: { userId: user.id, fullName: user.fullName || user.mobileNumber },
      width: '860px',
      maxWidth: '95vw',
      maxHeight: '95vh',
      panelClass: BASE_DIALOG_PANEL_CLASS,
    });
  }

  canDelete(user: UserDto): boolean {
    return user.userType !== 'Administrator' && user.userType !== 'AdminLab';
  }

  canViewResume(user: UserDto): boolean {
    return user.userType === 'User';
  }

  isSystemAdministrator(user: UserDto): boolean {
    return user.userType === 'Administrator';
  }

  userTypeLabel(userType: UserType): string {
    return this.translateUserType(userType);
  }

  async deleteUser(user: UserDto): Promise<void> {
    const message = this._localization.translate(
      'modules.admin.users.confirmDelete',
      {
        firstName: user.firstName,
        lastName: user.lastName,
      }
    );
    if (!confirm(message)) return;
    await this._usersService.deleteUser(user.id);
    this.load();
  }

  private translateUserType(userType: UserType): string {
    return this._localization.translate(`enum.userType.${userType}`);
  }
}
