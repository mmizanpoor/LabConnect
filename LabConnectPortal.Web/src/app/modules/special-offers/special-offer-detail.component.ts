import { Component, OnInit } from '@angular/core';
import { FormControl, ReactiveFormsModule } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { DomSanitizer, SafeHtml } from '@angular/platform-browser';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatTableModule } from '@angular/material/table';
import { TranslocoPipe } from '@jsverse/transloco';
import jMoment from 'moment-jalaali';
import { AuthUtils } from '@core/services/auth/auth.utils';
import { LocalizationService } from '@core/services/localization/localization.service';
import { PublicSpecialOffersService } from './public-special-offers.service';
import { PublicSpecialOfferDetailDto } from '../profile/special-offers/special-offers.types';

@Component({
  selector: 'app-special-offer-detail',
  standalone: true,
  imports: [ReactiveFormsModule, MatButtonModule, MatIconModule, MatTableModule, TranslocoPipe],
  templateUrl: './special-offer-detail.component.html',
})
export class SpecialOfferDetailComponent implements OnInit {
  loading = false;
  submitting = false;
  error = '';
  submitError = '';
  submitSuccess = '';
  offer: PublicSpecialOfferDetailDto | null = null;
  descriptionControl = new FormControl('', { nonNullable: true });
  displayedColumns = ['cpnCode', 'nationalCode', 'fullName', 'shortName', 'discount', 'maxSamples'];

  constructor(
    private _route: ActivatedRoute,
    private _router: Router,
    private _service: PublicSpecialOffersService,
    private _localization: LocalizationService,
    private _sanitizer: DomSanitizer,
    public authUtils: AuthUtils,
  ) {}

  get showDescriptionField(): boolean {
    return !!this.offer?.canSubmitRequest && !this.offer?.hasSubmittedRequest;
  }

  get isSubmitDisabled(): boolean {
    if (!this.offer || this.submitting || this.offer.hasSubmittedRequest || this.offer.isOwnLabOffer) return true;
    if (!this.authUtils.isAuthenticated) return false;
    return !this.offer.canSubmitRequest;
  }

  get submitButtonLabelKey(): string {
    if (this.offer?.hasSubmittedRequest) return 'modules.specialOffers.alreadySubmitted';
    if (this.offer?.isOwnLabOffer) return 'modules.specialOffers.submitOwnOfferNotAllowed';
    if (!this.authUtils.isAuthenticated) return 'modules.specialOffers.submitRequest';
    if (!this.offer?.canSubmitRequest) return 'modules.specialOffers.submitNotAvailable';
    return 'modules.specialOffers.submitRequest';
  }

  get proposerLabName(): string {
    const firstName = this.offer?.proposerFirstName?.trim();
    if (firstName) return firstName;

    const labName = this.offer?.labName?.trim();
    if (labName) return labName;

    const labCodeNew = this.offer?.labCodeNew;
    return labCodeNew ? String(labCodeNew) : '—';
  }

  ngOnInit(): void {
    void this.load();
  }

  async load(): Promise<void> {
    const id = Number(this._route.snapshot.paramMap.get('id'));
    if (!id) {
      this.error = this._localization.translate('modules.specialOffers.errors.notFound');
      return;
    }

    this.loading = true;
    this.error = '';
    try {
      const result = await this._service.getDetail(id);
      if (!result.success || !result.data) {
        throw new Error(result.message ?? this._localization.translate('modules.specialOffers.errors.notFound'));
      }
      this.offer = result.data;
    } catch (e: unknown) {
      this.error = e instanceof Error ? e.message : this._localization.translate('modules.specialOffers.errors.notFound');
    } finally {
      this.loading = false;
    }
  }

  fullBodyHtml(): SafeHtml {
    return this._sanitizer.bypassSecurityTrustHtml(this.offer?.fullBody ?? '');
  }

  formatDate(value: string): string {
    const parsed = jMoment(value).locale('fa');
    return parsed.isValid() ? parsed.format('jYYYY/jMM/jDD') : '—';
  }

  daysRemaining(): number | null {
    if (!this.offer) return null;
    const end = jMoment(this.offer.endDate).startOf('day');
    const today = jMoment().startOf('day');
    return end.diff(today, 'days');
  }

  async onSubmitRequest(): Promise<void> {
    if (!this.offer) return;

    if (!this.authUtils.isAuthenticated) {
      void this._router.navigate(['/auth/login'], {
        queryParams: { returnUrl: `/special-offers/${this.offer.id}` },
      });
      return;
    }

    if (this.offer.hasSubmittedRequest) {
      this.submitError = this._localization.translate('modules.specialOffers.alreadySubmitted');
      return;
    }

    if (this.offer.isOwnLabOffer) {
      this.submitError = this._localization.translate('modules.specialOffers.submitOwnOfferNotAllowed');
      return;
    }

    if (!this.offer.canSubmitRequest) {
      this.submitError = this._localization.translate('modules.specialOffers.submitNotAvailable');
      return;
    }

    const description = this.descriptionControl.value.trim();
    if (description.length > 2000) {
      this.submitError = this._localization.translate('modules.specialOffers.errors.descriptionTooLong');
      return;
    }

    this.submitting = true;
    this.submitError = '';
    this.submitSuccess = '';
    try {
      const result = await this._service.submitRequest(this.offer.id, { description });
      if (!result.success) {
        throw new Error(result.message ?? this._localization.translate('modules.specialOffers.errors.submitFailed'));
      }
      this.offer = { ...this.offer, hasSubmittedRequest: true, canSubmitRequest: false };
      this.submitSuccess = this._localization.translate('modules.specialOffers.submitSuccess');
    } catch (e: unknown) {
      this.submitError =
        e instanceof Error ? e.message : this._localization.translate('modules.specialOffers.errors.submitFailed');
    } finally {
      this.submitting = false;
    }
  }
}
