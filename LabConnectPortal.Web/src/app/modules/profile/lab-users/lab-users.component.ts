import {
  AfterViewInit,
  Component,
  OnInit,
  ViewChild,
  inject,
} from '@angular/core';
import { FormControl, ReactiveFormsModule } from '@angular/forms';
import { MatDialog, MatDialogModule } from '@angular/material/dialog';
import { MatIconModule } from '@angular/material/icon';
import { MatDrawer, MatSidenavModule } from '@angular/material/sidenav';
import { TranslocoPipe } from '@jsverse/transloco';
import jMoment from 'moment-jalaali';
import { BehaviorSubject, firstValueFrom } from 'rxjs';
import { ColDef } from 'ag-grid-community';
import { LocalizationService } from '@core/services/localization/localization.service';
import { BaseButtonComponent } from '@modules/base/components/base-button/base-button.component';
import { BaseGridComponent } from '@modules/base/components/base-grid/base-grid.component';
import {
  BaseFormFieldComponent,
  BaseFormSelectOption,
} from '@modules/base/components/base-form-field/base-form-field.component';
import {
  AddLabUserDialogComponent,
  AddLabUserDialogResult,
} from './add-lab-user-dialog.component';
import { RemoveLabUserDialogComponent } from './remove-lab-user-dialog.component';
import { LabUserPermissionsDialogComponent } from './lab-user-permissions-dialog.component';
import { LabUsersService } from './lab-users.service';
import { LabMemberDto } from './lab-users.types';
import { LabUsersColDef, LabUserAction } from './lab-users.coldef';
import { BASE_DIALOG_PANEL_CLASS } from '@modules/base/components/base-dialog/base-dialog.component';
import { SystemEntity } from '@core/system-entity/system-entity';
import { bindSystemEntity } from '@core/system-entity/bind-system-entity';
import { LabPermissionService } from '@core/services/auth/lab-permission.service';
import { AuthUtils } from '@core/services/auth/auth.utils';

jMoment.loadPersian({ dialect: 'persian-modern', usePersianDigits: true });

type LabUsersActiveFilter = 'all' | 'active' | 'inactive';

@Component({
  selector: 'app-lab-users',
  standalone: true,
  imports: [
    ReactiveFormsModule,
    MatDialogModule,
    MatIconModule,
    MatSidenavModule,
    TranslocoPipe,
    BaseButtonComponent,
    BaseFormFieldComponent,
    BaseGridComponent,
  ],
  templateUrl: './lab-users.component.html',
  styleUrl: './lab-users.component.scss',
})
export class LabUsersComponent implements OnInit, AfterViewInit {
  readonly entity = SystemEntity.LabUser;
  private readonly _systemEntityBinding = bindSystemEntity(SystemEntity.LabUser);

  @ViewChild('filterDrawer') filterDrawer?: MatDrawer;

  private _dialog = inject(MatDialog);

  loading = false;
  error = '';
  success = '';
  members: LabMemberDto[] = [];
  private _allMembers: LabMemberDto[] = [];
  mobileFilter = new FormControl('');
  activeFilter = new FormControl<LabUsersActiveFilter>('all');
  removingMemberId: string | null = null;
  colDef: ColDef[] = [];
  readonly list$ = new BehaviorSubject<LabMemberDto[]>([]);

  constructor(
    private _labUsersService: LabUsersService,
    private _localization: LocalizationService,
    private _colDef: LabUsersColDef,
    private _labPermission: LabPermissionService,
    private _authUtils: AuthUtils,
  ) {}

  get canCreate(): boolean {
    return this._labPermission.can(this.entity, 'create');
  }

  get canDelete(): boolean {
    return this._labPermission.can(this.entity, 'delete');
  }

  get canManagePermissions(): boolean {
    return this._authUtils.isAdminLab();
  }

  async ngOnInit(): Promise<void> {
    this.colDef = this._colDef.get(
      (t) => this.translateUserType(t),
      (v) => this.formatJalaliDate(v),
      (row) => this.removingMemberId === row.id,
      this.canDelete,
      this.canManagePermissions,
    );
    this._colDef.actionClicked.subscribe((evt: LabUserAction) => {
      if (!evt?.row) return;
      if (evt.type === 'remove') void this.removeMember(evt.row);
      if (evt.type === 'permissions') void this.openPermissionsDialog(evt.row);
    });
    await this.loadMembers();
  }

  ngAfterViewInit(): void {
    queueMicrotask(() => this.filterDrawer?.open());
  }

  get activeFilterOptions(): BaseFormSelectOption[] {
    return [
      { value: 'all', label: this._localization.translate('shared.all') },
      { value: 'active', label: this._localization.translate('shared.active') },
      {
        value: 'inactive',
        label: this._localization.translate('shared.inactive'),
      },
    ];
  }

  async openAddUserDialog(): Promise<void> {
    if (!this.canCreate) return;
    this.error = '';
    const dialogRef = this._dialog.open(AddLabUserDialogComponent, {
      width: '440px',
      maxWidth: '92vw',
      autoFocus: false,
      disableClose: false,
      panelClass: BASE_DIALOG_PANEL_CLASS,
      data: {
        existingMobiles: this._allMembers.map((member) => member.mobileNumber),
      },
    });

    const result = (await firstValueFrom(dialogRef.afterClosed())) as
      | AddLabUserDialogResult
      | undefined;
    if (!result?.successMessage) return;

    this.success = result.successMessage;
    await this.loadMembers();
  }

  async loadMembers(): Promise<void> {
    this.loading = true;
    this.error = '';
    try {
      this._allMembers = await this._labUsersService.getMembers();
      this.refreshFilteredList();
    } catch (e: unknown) {
      this.error =
        e instanceof Error
          ? e.message
          : this._localization.translate(
              'modules.profile.labUsers.errors.loadFailed'
            );
    } finally {
      this.loading = false;
    }
  }

  applyFilters(): void {
    this.refreshFilteredList();
    void this.filterDrawer?.close();
  }

  private refreshFilteredList(): void {
    const mobile = this.mobileFilter.value?.trim() ?? '';
    const active = this.activeFilter.value ?? 'all';

    this.members = this._allMembers.filter((member) => {
      const matchesMobile = !mobile || member.mobileNumber.includes(mobile);
      const matchesActive =
        active === 'all' ||
        (active === 'active' && member.isActive) ||
        (active === 'inactive' && !member.isActive);
      return matchesMobile && matchesActive;
    });
    this.list$.next(this.members);
  }

  translateUserType(userType: string): string {
    return this._localization.translateEnum('userType', userType);
  }

  formatJalaliDate(value: string): string {
    if (!value) return '—';
    const formatted = jMoment(value).format('jYYYY/jMM/jDD');
    return formatted === 'Invalid date' ? value : formatted;
  }

  async openPermissionsDialog(member: LabMemberDto): Promise<void> {
    if (!this.canManagePermissions) return;
    const dialogRef = this._dialog.open(LabUserPermissionsDialogComponent, {
      autoFocus: false,
      panelClass: [BASE_DIALOG_PANEL_CLASS, 'lab-user-permissions-dialog-panel'],
      data: { member },
    });
    const saved = await firstValueFrom(dialogRef.afterClosed());
    if (saved) {
      this.success = this._localization.translate(
        'modules.profile.labUsers.permissions.success.saved'
      );
    }
  }

  async removeMember(member: LabMemberDto): Promise<void> {
    if (!this.canDelete) return;
    if (member.userType === 'AdminLab') {
      this.error = this._localization.translate(
        'modules.profile.labUsers.errors.cannotManageAdmin'
      );
      return;
    }

    const dialogRef = this._dialog.open(RemoveLabUserDialogComponent, {
      width: '480px',
      maxWidth: '94vw',
      autoFocus: false,
      panelClass: BASE_DIALOG_PANEL_CLASS,
      data: { mobile: member.mobileNumber },
    });
    const confirmed = await firstValueFrom(dialogRef.afterClosed());
    if (!confirmed) return;

    this.error = '';
    this.success = '';
    this.removingMemberId = member.id;
    try {
      await this._labUsersService.removeMember({ memberId: member.id });
      this._allMembers = this._allMembers.filter(
        (item) => item.id !== member.id
      );
      this.refreshFilteredList();
      this.success = this._localization.translate(
        'modules.profile.labUsers.success.removed'
      );
    } catch (e: unknown) {
      this.error =
        e instanceof Error
          ? e.message
          : this._localization.translate(
              'modules.profile.labUsers.errors.removeFailed'
            );
    } finally {
      this.removingMemberId = null;
    }
  }
}
