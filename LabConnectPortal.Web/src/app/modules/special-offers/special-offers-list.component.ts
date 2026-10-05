import { ChangeDetectorRef, Component, DestroyRef, OnInit, inject } from '@angular/core';
import { FormControl, ReactiveFormsModule } from '@angular/forms';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { MatIconModule } from '@angular/material/icon';
import { TranslocoPipe } from '@jsverse/transloco';
import jMoment from 'moment-jalaali';
import { LocalizationService } from '@core/services/localization/localization.service';
import { PublicSpecialOffersService } from './public-special-offers.service';
import { PublicSpecialOfferCardDto } from '../profile/special-offers/special-offers.types';

@Component({
  selector: 'app-public-special-offers-list',
  standalone: true,
  imports: [ReactiveFormsModule, RouterLink, MatIconModule, TranslocoPipe],
  templateUrl: './special-offers-list.component.html',
})
export class PublicSpecialOffersListComponent implements OnInit {
  private _service = inject(PublicSpecialOffersService);
  private _localization = inject(LocalizationService);
  private _route = inject(ActivatedRoute);
  private _router = inject(Router);
  private _destroyRef = inject(DestroyRef);
  private _cdr = inject(ChangeDetectorRef);
  private _failedLogoIds = new Set<number>();

  loading = false;
  error = '';
  offers: PublicSpecialOfferCardDto[] = [];
  filteredOffers: PublicSpecialOfferCardDto[] = [];
  searchTerm = '';
  searchInput = new FormControl('', { nonNullable: true });

  ngOnInit(): void {
    this._route.queryParamMap.pipe(takeUntilDestroyed(this._destroyRef)).subscribe((params) => {
      this.searchTerm = params.get('search')?.trim() ?? '';
      this.searchInput.setValue(this.searchTerm, { emitEvent: false });
      this.applyFilter();
    });
    void this.load();
  }

  async load(): Promise<void> {
    this.loading = true;
    this.error = '';
    try {
      const result = await this._service.getActiveForHome();
      if (!result.success || !result.data) {
        throw new Error(result.message ?? this._localization.translate('modules.specialOffers.errors.notFound'));
      }
      this.offers = result.data;
      this.applyFilter();
    } catch (e: unknown) {
      this.error =
        e instanceof Error ? e.message : this._localization.translate('modules.specialOffers.errors.notFound');
    } finally {
      this.loading = false;
    }
  }

  onSearch(event: Event): void {
    event.preventDefault();
    const term = this.searchInput.value.trim();
    void this._router.navigate([], {
      relativeTo: this._route,
      queryParams: { search: term || null },
      queryParamsHandling: 'merge',
    });
  }

  clearSearchFilter(): void {
    this.searchInput.setValue('');
    void this._router.navigate([], {
      relativeTo: this._route,
      queryParams: { search: null },
      queryParamsHandling: 'merge',
    });
  }

  formatEndDate(value: string): string {
    const parsed = jMoment(value).locale('fa');
    return parsed.isValid() ? parsed.format('jYYYY/jMM/jDD') : '—';
  }

  daysRemaining(value: string): number {
    const end = jMoment(value).startOf('day');
    const today = jMoment().startOf('day');
    return Math.max(end.diff(today, 'days'), 0);
  }

  labLogoUrl(profileId: string): string {
    return PublicSpecialOffersService.labLogoUrl(profileId);
  }

  showLabLogo(offer: PublicSpecialOfferCardDto): boolean {
    return !!offer.hasLabLogo && !!offer.labProfileId && !this._failedLogoIds.has(offer.id);
  }

  onLogoError(offerId: number): void {
    this._failedLogoIds.add(offerId);
    this._cdr.markForCheck();
  }

  private applyFilter(): void {
    const term = this.searchTerm.trim().toLowerCase();
    if (!term) {
      this.filteredOffers = this.offers;
      return;
    }

    this.filteredOffers = this.offers.filter((offer) => {
      const haystack = [offer.title, offer.summary, offer.labName]
        .filter(Boolean)
        .join(' ')
        .toLowerCase();
      return haystack.includes(term);
    });
  }
}
