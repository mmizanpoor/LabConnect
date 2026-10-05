import { Component, Inject, inject } from '@angular/core';
import { MAT_DIALOG_DATA, MatDialogModule, MatDialogRef } from '@angular/material/dialog';
import { MatIconModule } from '@angular/material/icon';
import { TranslocoPipe } from '@jsverse/transloco';
import { BaseButtonComponent } from '@modules/base/components/base-button/base-button.component';
import { BaseDialogComponent } from '@modules/base/components/base-dialog/base-dialog.component';

export type UsernameChangeChannel = 'Email' | 'Mobile';

export interface UsernameChangeChannelDialogData {
  hasEmail: boolean;
  hasMobile: boolean;
  email?: string;
  mobileNumber?: string;
  titleKey?: string;
  hintKey?: string;
  subhintKey?: string;
}

@Component({
  selector: 'app-username-change-channel-dialog',
  standalone: true,
  imports: [MatDialogModule, MatIconModule, TranslocoPipe, BaseButtonComponent, BaseDialogComponent],
  template: `
    <base-dialog [title]="titleKey | transloco">
      <div class="uchg-dialog" dir="rtl">
        <div class="uchg-dialog__intro">
          <span class="uchg-dialog__badge" aria-hidden="true">
            <mat-icon>verified_user</mat-icon>
          </span>
          <div class="uchg-dialog__intro-text">
            <p class="uchg-dialog__hint">
              {{ hintKey | transloco }}
            </p>
            <p class="uchg-dialog__subhint">
              {{ subhintKey | transloco }}
            </p>
          </div>
        </div>

        <div class="uchg-dialog__options">
          @if (data.hasMobile) {
            <button type="button" class="uchg-option" (click)="select('Mobile')">
              <span class="uchg-option__icon uchg-option__icon--mobile" aria-hidden="true">
                <mat-icon>smartphone</mat-icon>
              </span>
              <span class="uchg-option__body">
                <span class="uchg-option__title">
                  {{ 'modules.profile.accountSecurity.usernameOtpViaMobile' | transloco }}
                </span>
                <span class="uchg-option__meta">{{ maskedMobile }}</span>
              </span>
              <span class="uchg-option__chevron" aria-hidden="true">
                <mat-icon>chevron_left</mat-icon>
              </span>
            </button>
          }

          @if (data.hasEmail) {
            <button type="button" class="uchg-option" (click)="select('Email')">
              <span class="uchg-option__icon uchg-option__icon--email" aria-hidden="true">
                <mat-icon>mail</mat-icon>
              </span>
              <span class="uchg-option__body">
                <span class="uchg-option__title">
                  {{ 'modules.profile.accountSecurity.usernameOtpViaEmail' | transloco }}
                </span>
                <span class="uchg-option__meta">{{ maskedEmail }}</span>
              </span>
              <span class="uchg-option__chevron" aria-hidden="true">
                <mat-icon>chevron_left</mat-icon>
              </span>
            </button>
          }
        </div>
      </div>

      <div baseDialogActions class="uchg-dialog__actions">
        <base-button color="soft" size="sm" (click)="close()">
          {{ 'shared.cancel' | transloco }}
        </base-button>
      </div>
    </base-dialog>
  `,
  styleUrl: './username-change-channel-dialog.component.scss',
})
export class UsernameChangeChannelDialogComponent {
  private _dialogRef = inject(
    MatDialogRef<UsernameChangeChannelDialogComponent, UsernameChangeChannel | null>,
  );

  readonly titleKey: string;
  readonly hintKey: string;
  readonly subhintKey: string;

  constructor(
    @Inject(MAT_DIALOG_DATA) readonly data: UsernameChangeChannelDialogData,
  ) {
    this.titleKey =
      data.titleKey ?? 'modules.profile.accountSecurity.usernameOtpChannelTitle';
    this.hintKey =
      data.hintKey ?? 'modules.profile.accountSecurity.usernameOtpChannelHint';
    this.subhintKey =
      data.subhintKey ?? 'modules.profile.accountSecurity.usernameOtpChannelSubhint';
  }

  get maskedMobile(): string {
    const mobile = (this.data.mobileNumber ?? '').trim();
    if (mobile.length < 4) return mobile;
    return `${mobile.slice(0, 4)}***${mobile.slice(-2)}`;
  }

  get maskedEmail(): string {
    const email = (this.data.email ?? '').trim();
    const at = email.indexOf('@');
    if (at <= 1) return email;
    const name = email.slice(0, at);
    const domain = email.slice(at);
    const visible = name.slice(0, Math.min(2, name.length));
    return `${visible}***${domain}`;
  }

  select(channel: UsernameChangeChannel): void {
    this._dialogRef.close(channel);
  }

  close(): void {
    this._dialogRef.close(null);
  }
}
