import { Component, Inject, OnInit } from '@angular/core';
import { FormControl, ReactiveFormsModule } from '@angular/forms';
import {
  MAT_DIALOG_DATA,
  MatDialogModule,
  MatDialogRef,
} from '@angular/material/dialog';
import { MatButtonModule } from '@angular/material/button';
import { BaseCheckboxComponent } from '@modules/base/components/base-checkbox/base-checkbox.component';
import { MatIconModule } from '@angular/material/icon';
import { TranslocoPipe } from '@jsverse/transloco';
import { LocalizationService } from '@core/services/localization/localization.service';
import { BaseButtonComponent } from '@modules/base/components/base-button/base-button.component';
import { BaseDialogComponent } from '@modules/base/components/base-dialog/base-dialog.component';
import { SystemEntity } from '@core/system-entity/system-entity';
import { bindSystemEntity } from '@core/system-entity/bind-system-entity';
import { UsersService } from '../users/users.service';
import { UserDto, UserType } from '../users/users.types';
import { SiteUsersService } from './site-users.service';
import { SiteMemberDto } from './site-users.types';

export interface SiteUserDetailDialogData {
  member: SiteMemberDto;
}

@Component({
  selector: 'app-site-user-detail',
  standalone: true,
  imports: [
    ReactiveFormsModule,
    MatDialogModule,
    MatButtonModule,
    MatIconModule,
    BaseCheckboxComponent,
    TranslocoPipe,
    BaseButtonComponent,
    BaseDialogComponent,
  ],
  templateUrl: './site-user-detail.component.html',
  styles: [
    `
      :host ::ng-deep .account-status-checkbox--active.mat-mdc-checkbox {
        --mdc-checkbox-selected-focus-icon-color: #059669;
        --mdc-checkbox-selected-hover-icon-color: #059669;
        --mdc-checkbox-selected-icon-color: #059669;
        --mdc-checkbox-selected-pressed-icon-color: #059669;
        --mdc-checkbox-selected-checkmark-color: #fff;
      }

      :host ::ng-deep .account-status-checkbox--active .mdc-label {
        font-weight: 600;
        color: #047857;
      }
    `,
  ],
})
export class SiteUserDetailComponent implements OnInit {
  readonly entity = SystemEntity.SiteUser;
  private readonly _systemEntityBinding = bindSystemEntity(SystemEntity.SiteUser);

  user: UserDto;
  loading = true;
  loadError = '';
  isActiveControl: FormControl<boolean>;
  saving = false;

  constructor(
    @Inject(MAT_DIALOG_DATA) public data: SiteUserDetailDialogData,
    private _dialogRef: MatDialogRef<SiteUserDetailComponent, boolean>,
    private _usersService: UsersService,
    private _siteUsersService: SiteUsersService,
    private _localization: LocalizationService
  ) {
    this.user = this.mapMemberToUser(data.member);
    this.isActiveControl = new FormControl(data.member.isActive, {
      nonNullable: true,
    });
  }

  async ngOnInit(): Promise<void> {
    try {
      const result = await this._usersService.getById(this.data.member.id);
      if (result.success && result.data) {
        this.user = result.data;
        this.isActiveControl.setValue(result.data.isActive);
      }
    } catch (e: unknown) {
      this.loadError =
        e instanceof Error
          ? e.message
          : this._localization.translate(
              'modules.admin.siteUsers.errors.loadFailed'
            );
    } finally {
      this.loading = false;
    }
  }

  get fullName(): string {
    return `${this.user.firstName} ${this.user.lastName}`.trim();
  }

  get userTypeThemeClass(): string {
    const themes: Record<UserType, string> = {
      User: 'border-blue-200 bg-gradient-to-br from-blue-50 via-teal-50 to-slate-50',
      UserLab:
        'border-violet-200 bg-gradient-to-br from-violet-50 via-violet-100 to-slate-50',
      Store:
        'border-orange-200 bg-gradient-to-br from-orange-50 via-orange-100 to-slate-50',
      AdminLab:
        'border-indigo-200 bg-gradient-to-br from-indigo-50 via-indigo-100 to-slate-50',
      Administrator:
        'border-rose-200 bg-gradient-to-br from-rose-50 via-rose-100 to-slate-50',
      Admin:
        'border-cyan-200 bg-gradient-to-br from-cyan-50 via-cyan-100 to-slate-50',
    };
    return themes[this.user.userType] ?? themes.Admin;
  }

  get createdAtText(): string {
    if (!this.user.createdAt) return '—';
    const date = new Date(this.user.createdAt);
    if (Number.isNaN(date.getTime())) return this.user.createdAt;
    return new Intl.DateTimeFormat('fa-IR', {
      year: 'numeric',
      month: 'long',
      day: 'numeric',
    }).format(date);
  }

  async save(): Promise<void> {
    this.saving = true;
    this.loadError = '';
    try {
      const updated = await this._siteUsersService.toggleMemberActive({
        memberId: this.user.id,
        isActive: this.isActiveControl.value,
      });
      this.user = { ...this.user, isActive: updated.isActive };
      this._dialogRef.close(true);
    } catch (e: unknown) {
      this.loadError =
        e instanceof Error
          ? e.message
          : this._localization.translate(
              'modules.admin.siteUsers.errors.toggleActiveFailed'
            );
    } finally {
      this.saving = false;
    }
  }

  cancel(): void {
    this._dialogRef.close();
  }

  userTypeLabel(userType: string): string {
    return this._localization.translate(`enum.userType.${userType}`);
  }

  private mapMemberToUser(member: SiteMemberDto): UserDto {
    return {
      id: member.id,
      userType: (member.userType as UserType) || 'Admin',
      firstName: member.firstName,
      lastName: member.lastName,
      username: member.mobileNumber,
      mobileNumber: member.mobileNumber,
      isActive: member.isActive,
      address: '',
      phone: '',
      email: '',
      emailConfirmed: false,
      mobileConfirmed: member.mobileConfirmed,
      createdAt: member.createdAt,
    };
  }
}
