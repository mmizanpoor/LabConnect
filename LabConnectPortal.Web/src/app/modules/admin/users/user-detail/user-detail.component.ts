import { Component, Inject } from '@angular/core';
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
import { UsersService } from '../users.service';
import { UserDto, UserType } from '../users.types';
import { SystemEntity } from '@core/system-entity/system-entity';
import { bindSystemEntity } from '@core/system-entity/bind-system-entity';

@Component({
  selector: 'app-user-detail',
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
  templateUrl: './user-detail.component.html',
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
export class UserDetailComponent {
  readonly entity = SystemEntity.User;
  private readonly _systemEntityBinding = bindSystemEntity(SystemEntity.User);

  isActiveControl: FormControl<boolean>;
  saving = false;
  readonly isReadOnly: boolean;

  constructor(
    @Inject(MAT_DIALOG_DATA) public data: { user: UserDto },
    private _dialogRef: MatDialogRef<UserDetailComponent>,
    private _usersService: UsersService,
    private _localization: LocalizationService
  ) {
    this.isReadOnly = data.user.userType === 'Administrator';
    this.isActiveControl = new FormControl(
      { value: data.user.isActive, disabled: this.isReadOnly },
      { nonNullable: true }
    );
  }

  get fullName(): string {
    return `${this.data.user.firstName} ${this.data.user.lastName}`.trim();
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
    return themes[this.data.user.userType] ?? themes.User;
  }

  get createdAtText(): string {
    if (!this.data.user.createdAt) return '—';
    const date = new Date(this.data.user.createdAt);
    if (Number.isNaN(date.getTime())) return this.data.user.createdAt;
    return new Intl.DateTimeFormat('fa-IR', {
      year: 'numeric',
      month: 'long',
      day: 'numeric',
    }).format(date);
  }

  async save(): Promise<void> {
    if (this.isReadOnly) return;
    this.saving = true;
    await this._usersService.toggleActive(
      this.data.user.id,
      this.isActiveControl.value
    );
    this.saving = false;
    this._dialogRef.close(true);
  }

  cancel(): void {
    this._dialogRef.close();
  }

  userTypeLabel(userType: UserType): string {
    return this._localization.translate(`enum.userType.${userType}`);
  }
}
