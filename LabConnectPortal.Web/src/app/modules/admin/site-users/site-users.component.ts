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
  AddSiteUserDialogComponent,
  AddSiteUserDialogResult,
} from './add-site-user-dialog.component';
import { RemoveSiteUserDialogComponent } from './remove-site-user-dialog.component';
import { SiteUserPermissionsDialogComponent } from './site-user-permissions-dialog.component';
import { SiteUserDetailComponent } from './site-user-detail.component';
import { SiteUsersService } from './site-users.service';
import { SiteMemberDto } from './site-users.types';
import { SiteUsersColDef, SiteUserAction } from './site-users.coldef';
import { BASE_DIALOG_PANEL_CLASS } from '@modules/base/components/base-dialog/base-dialog.component';
import { SystemEntity } from '@core/system-entity/system-entity';
import { bindSystemEntity } from '@core/system-entity/bind-system-entity';

jMoment.loadPersian({ dialect: 'persian-modern', usePersianDigits: true });

type SiteUsersActiveFilter = 'all' | 'active' | 'inactive';

@Component({
  selector: 'app-site-users',
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
  templateUrl: './site-users.component.html',
  styleUrl: './site-users.component.scss',
})
export class SiteUsersComponent implements OnInit, AfterViewInit {
  readonly entity = SystemEntity.SiteUser;
  private readonly _systemEntityBinding = bindSystemEntity(SystemEntity.SiteUser);

  @ViewChild('filterDrawer') filterDrawer?: MatDrawer;

  private _dialog = inject(MatDialog);

  loading = false;
  error = '';
  success = '';
  members: SiteMemberDto[] = [];
  private _allMembers: SiteMemberDto[] = [];
  mobileFilter = new FormControl('');
  activeFilter = new FormControl<SiteUsersActiveFilter>('all');
  removingMemberId: string | null = null;
  colDef: ColDef[] = [];
  readonly list$ = new BehaviorSubject<SiteMemberDto[]>([]);

  constructor(
    private _siteUsersService: SiteUsersService,
    private _localization: LocalizationService,
    private _colDef: SiteUsersColDef
  ) {}

  get canCreate(): boolean {
    return true;
  }

  get canDelete(): boolean {
    return true;
  }

  get canManagePermissions(): boolean {
    return true;
  }

  async ngOnInit(): Promise<void> {
    this.colDef = this._colDef.get(
      (t) => this.translateUserType(t),
      (v) => this.formatJalaliDate(v),
      (row) => this.removingMemberId === row.id,
      this.canDelete,
      this.canManagePermissions
    );
    this._colDef.actionClicked.subscribe((evt: SiteUserAction) => {
      if (!evt?.row) return;
      if (evt.type === 'detail') void this.openDetail(evt.row);
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
    this.error = '';
    const dialogRef = this._dialog.open(AddSiteUserDialogComponent, {
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
      | AddSiteUserDialogResult
      | undefined;
    if (!result?.successMessage) return;

    this.success = result.successMessage;
    await this.loadMembers();
  }

  async loadMembers(): Promise<void> {
    this.loading = true;
    this.error = '';
    try {
      this._allMembers = await this._siteUsersService.getMembers();
      this.refreshFilteredList();
    } catch (e: unknown) {
      this.error =
        e instanceof Error
          ? e.message
          : this._localization.translate(
              'modules.admin.siteUsers.errors.loadFailed'
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

  async openPermissionsDialog(member: SiteMemberDto): Promise<void> {
    const dialogRef = this._dialog.open(SiteUserPermissionsDialogComponent, {
      autoFocus: false,
      panelClass: [BASE_DIALOG_PANEL_CLASS, 'lab-user-permissions-dialog-panel'],
      data: { member },
    });
    const saved = await firstValueFrom(dialogRef.afterClosed());
    if (saved) {
      this.success = this._localization.translate(
        'modules.admin.siteUsers.permissions.success.saved'
      );
    }
  }

  async openDetail(member: SiteMemberDto): Promise<void> {
    this.error = '';
    const dialogRef = this._dialog.open(SiteUserDetailComponent, {
      data: { member },
      width: '560px',
      maxWidth: '95vw',
      autoFocus: false,
      panelClass: BASE_DIALOG_PANEL_CLASS,
    });
    const changed = await firstValueFrom(dialogRef.afterClosed());
    if (changed) {
      await this.loadMembers();
    }
  }

  onRowClicked(event: { data?: SiteMemberDto }): void {
    if (event?.data) void this.openDetail(event.data);
  }

  async removeMember(member: SiteMemberDto): Promise<void> {
    const dialogRef = this._dialog.open(RemoveSiteUserDialogComponent, {
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
      await this._siteUsersService.removeMember({ memberId: member.id });
      this._allMembers = this._allMembers.filter(
        (item) => item.id !== member.id
      );
      this.refreshFilteredList();
      this.success = this._localization.translate(
        'modules.admin.siteUsers.success.removed'
      );
    } catch (e: unknown) {
      this.error =
        e instanceof Error
          ? e.message
          : this._localization.translate(
              'modules.admin.siteUsers.errors.removeFailed'
            );
    } finally {
      this.removingMemberId = null;
    }
  }
}
