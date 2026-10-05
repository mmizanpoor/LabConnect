import { DecimalPipe } from '@angular/common';
import { Component, OnInit } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { DomSanitizer, SafeHtml } from '@angular/platform-browser';
import { MatIconModule } from '@angular/material/icon';
import { MatTableModule } from '@angular/material/table';
import { TranslocoPipe } from '@jsverse/transloco';
import jMoment from 'moment-jalaali';
import { LocalizationService } from '@core/services/localization/localization.service';
import { BaseBackButtonComponent } from '@modules/base/components/base-back-button/base-back-button.component';
import { AdminSpecialOffersService } from './admin-special-offers.service';
import { AdminSpecialOfferDetail } from './admin-special-offers.types';
import { SystemEntity } from '@core/system-entity/system-entity';
import { bindSystemEntity } from '@core/system-entity/bind-system-entity';

@Component({
  selector: 'app-admin-special-offer-detail',
  standalone: true,
  imports: [BaseBackButtonComponent, MatIconModule, MatTableModule, TranslocoPipe, DecimalPipe],
  templateUrl: './admin-special-offer-detail.component.html',
})
export class AdminSpecialOfferDetailComponent implements OnInit {
  readonly entity = SystemEntity.SpecialOffer;
  private readonly _systemEntityBinding = bindSystemEntity(SystemEntity.SpecialOffer);

  loading = false;
  error = '';
  offer: AdminSpecialOfferDetail | null = null;
  displayedColumns = ['cpnCode', 'nationalCode', 'fullName', 'shortName', 'discount', 'maxSamples'];
  /** Visible body rows before the tests table scrolls (header row is extra). */
  readonly testsVisibleRowCount = 15;

  constructor(
    private _route: ActivatedRoute,
    private _service: AdminSpecialOffersService,
    private _localization: LocalizationService,
    private _sanitizer: DomSanitizer,
  ) {}

  ngOnInit(): void {
    void this.load();
  }

  async load(): Promise<void> {
    const id = Number(this._route.snapshot.paramMap.get('id'));
    if (!id) {
      this.error = this._localization.translate('modules.admin.specialOffers.errors.notFound');
      return;
    }

    this.loading = true;
    this.error = '';
    try {
      const result = await this._service.getById(id);
      if (!result.success || !result.data) {
        throw new Error(
          result.message ?? this._localization.translate('modules.admin.specialOffers.errors.notFound'),
        );
      }
      this.offer = result.data;
    } catch (e: unknown) {
      this.error =
        e instanceof Error
          ? e.message
          : this._localization.translate('modules.admin.specialOffers.errors.loadFailed');
    } finally {
      this.loading = false;
    }
  }

  formatDate(value: string): string {
    const parsed = jMoment(value).locale('fa');
    return parsed.isValid() ? parsed.format('jYYYY/jMM/jDD') : '—';
  }

  statusLabel(): string {
    if (!this.offer) return '—';
    if (this.offer.isExpired) {
      return this._localization.translate('modules.profile.specialOffers.status.expired');
    }
    if (this.offer.isActive) {
      return this._localization.translate('modules.profile.specialOffers.status.active');
    }
    return this._localization.translate('modules.profile.specialOffers.status.inactive');
  }

  fullBodyHtml(): SafeHtml {
    return this._sanitizer.bypassSecurityTrustHtml(this.offer?.fullBody ?? '');
  }
}
