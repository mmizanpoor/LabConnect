import { Component, OnDestroy, OnInit } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { MatButtonModule } from '@angular/material/button';
import { MatDialog } from '@angular/material/dialog';
import { MatIconModule } from '@angular/material/icon';
import { TranslocoPipe } from '@jsverse/transloco';
import { firstValueFrom } from 'rxjs';
import { LocalizationService } from '@core/services/localization/localization.service';
import { BaseBackButtonComponent } from '@modules/base/components/base-back-button/base-back-button.component';
import { BaseButtonComponent } from '@modules/base/components/base-button/base-button.component';
import { RejectProfileDialogComponent } from '@modules/admin/shared/reject-profile-dialog/reject-profile-dialog.component';
import { BASE_DIALOG_PANEL_CLASS } from '@modules/base/components/base-dialog/base-dialog.component';
import { ShopsService } from '../shops.service';
import { CenterProfileDto } from '../shops.types';
import { SystemEntity } from '@core/system-entity/system-entity';
import { bindSystemEntity } from '@core/system-entity/bind-system-entity';

@Component({
  selector: 'app-shop-detail',
  standalone: true,
  imports: [
    MatButtonModule,
    MatIconModule,
    TranslocoPipe,
    BaseBackButtonComponent,
    BaseButtonComponent,
  ],
  templateUrl: './shop-detail.component.html',
})
export class ShopDetailComponent implements OnInit, OnDestroy {
  readonly entity = SystemEntity.Shop;
  private readonly _systemEntityBinding = bindSystemEntity(SystemEntity.Shop);

  loading = false;
  approving = false;
  rejecting = false;
  revoking = false;
  error = '';
  success = '';
  profile: CenterProfileDto | null = null;

  logoUrl = '';
  nationalCardUrl = '';
  licenseUrl = '';
  officialImageUrl = '';
  tradeCardUrl = '';

  get canApprove(): boolean {
    return !!this.profile?.isComplete && !this.profile?.isApproved;
  }

  get canReject(): boolean {
    return this.canApprove;
  }

  get canRevokeApproval(): boolean {
    return !!this.profile?.isApproved;
  }

  get approvedAtText(): string {
    if (!this.profile?.approvedAt) return '';
    const date = new Date(this.profile.approvedAt);
    if (Number.isNaN(date.getTime())) return '';
    return this._localization.translate('modules.admin.shops.approvedAt', {
      date: new Intl.DateTimeFormat('fa-IR', {
        year: 'numeric',
        month: 'long',
        day: 'numeric',
      }).format(date),
    });
  }

  constructor(
    private _route: ActivatedRoute,
    private _shopsService: ShopsService,
    private _localization: LocalizationService,
    private _dialog: MatDialog,
  ) {}

  async ngOnInit(): Promise<void> {
    const id = this._route.snapshot.paramMap.get('id');
    if (!id) return;
    await this.load(id);
  }

  ngOnDestroy(): void {
    this.revokeUrls();
  }

  async reject(): Promise<void> {
    if (!this.profile || !this.canReject) return;
    const ref = this._dialog.open(RejectProfileDialogComponent, {
      width: '420px',
      panelClass: BASE_DIALOG_PANEL_CLASS,
    });
    const reason = await firstValueFrom(ref.afterClosed());
    if (!reason) return;

    this.rejecting = true;
    this.error = '';
    this.success = '';
    try {
      const result = await this._shopsService.reject(this.profile.id, reason);
      if (!result.success) {
        throw new Error(result.message ?? this._localization.translate('modules.admin.shops.errors.rejectFailed'));
      }
      this.success = this._localization.translate('modules.admin.shops.success.rejected');
      await this.load(this.profile.id);
    } catch (e: unknown) {
      this.error = e instanceof Error ? e.message : this._localization.translate('modules.admin.shops.errors.rejectFailed');
    } finally {
      this.rejecting = false;
    }
  }

  async approve(): Promise<void> {
    if (!this.profile || !this.canApprove) return;
    this.approving = true;
    this.error = '';
    this.success = '';
    try {
      const result = await this._shopsService.approve(this.profile.id);
      if (!result.success) {
        throw new Error(result.message ?? this._localization.translate('modules.admin.shops.errors.approveFailed'));
      }
      this.success = this._localization.translate('modules.admin.shops.success.approved');
      await this.load(this.profile.id);
    } catch (e: unknown) {
      this.error = e instanceof Error ? e.message : this._localization.translate('modules.admin.shops.errors.approveFailed');
    } finally {
      this.approving = false;
    }
  }

  async revokeApproval(): Promise<void> {
    if (!this.profile || !this.canRevokeApproval) return;
    const message = this._localization.translate('modules.admin.shops.confirmRevokeApproval', {
      name: this.profile.name,
    });
    if (!confirm(message)) return;

    this.revoking = true;
    this.error = '';
    this.success = '';
    try {
      const result = await this._shopsService.revokeApproval(this.profile.id);
      if (!result.success) {
        throw new Error(
          result.message ?? this._localization.translate('modules.admin.shops.errors.revokeFailed'),
        );
      }
      this.success = this._localization.translate('modules.admin.shops.success.revoked');
      await this.load(this.profile.id);
    } catch (e: unknown) {
      this.error =
        e instanceof Error ? e.message : this._localization.translate('modules.admin.shops.errors.revokeFailed');
    } finally {
      this.revoking = false;
    }
  }

  private async load(id: string): Promise<void> {
    this.loading = true;
    this.error = '';
    try {
      const result = await this._shopsService.getById(id);
      if (!result.success || !result.data) {
        throw new Error(result.message ?? this._localization.translate('modules.admin.shops.errors.loadFailed'));
      }
      this.profile = result.data;
      await this.loadPreviews();
    } catch (e: unknown) {
      this.error = e instanceof Error ? e.message : this._localization.translate('modules.admin.shops.errors.loadFailed');
    } finally {
      this.loading = false;
    }
  }

  private async loadPreviews(): Promise<void> {
    this.revokeUrls();
    if (!this.profile) return;
    const tasks: Promise<void>[] = [];
    if (this.profile.hasLogo) {
      tasks.push(
        this.blobUrl(this.profile.id, 'Logo')
          .then((url) => {
            this.logoUrl = url;
          })
          .catch(() => undefined),
      );
    }
    if (this.profile.hasNationalCard) {
      tasks.push(
        this.blobUrl(this.profile.id, 'NationalCard')
          .then((url) => {
            this.nationalCardUrl = url;
          })
          .catch(() => undefined),
      );
    }
    if (this.profile.hasLicense) {
      tasks.push(
        this.blobUrl(this.profile.id, 'License')
          .then((url) => {
            this.licenseUrl = url;
          })
          .catch(() => undefined),
      );
    }
    if (this.profile.hasOfficialImage) {
      tasks.push(
        this.blobUrl(this.profile.id, 'OfficialImage')
          .then((url) => {
            this.officialImageUrl = url;
          })
          .catch(() => undefined),
      );
    }
    if (this.profile.hasTradeCard) {
      tasks.push(
        this.blobUrl(this.profile.id, 'TradeCard')
          .then((url) => {
            this.tradeCardUrl = url;
          })
          .catch(() => undefined),
      );
    }
    await Promise.all(tasks);
  }

  private async blobUrl(profileId: string, kind: 'Logo' | 'NationalCard' | 'License' | 'OfficialImage' | 'TradeCard'): Promise<string> {
    const blob = await this._shopsService.getFileBlob(profileId, kind);
    return URL.createObjectURL(blob);
  }

  private revokeUrls(): void {
    for (const url of [this.logoUrl, this.nationalCardUrl, this.licenseUrl, this.officialImageUrl, this.tradeCardUrl]) {
      if (url) URL.revokeObjectURL(url);
    }
    this.logoUrl = '';
    this.nationalCardUrl = '';
    this.licenseUrl = '';
    this.officialImageUrl = '';
    this.tradeCardUrl = '';
  }
}
