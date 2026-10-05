import { ChangeDetectorRef, Component, Input, inject } from '@angular/core';
import { RouterLink } from '@angular/router';
import { MatIconModule } from '@angular/material/icon';
import { TranslocoPipe } from '@jsverse/transloco';
import jMoment from 'moment-jalaali';
import { PublicSpecialOffersService } from '../../special-offers/public-special-offers.service';
import { PublicSpecialOfferCardDto } from '../../profile/special-offers/special-offers.types';
import { HomeScrollRowComponent } from '../home-scroll-row/home-scroll-row.component';

@Component({
  selector: 'app-home-special-offers',
  standalone: true,
  imports: [RouterLink, MatIconModule, TranslocoPipe, HomeScrollRowComponent],
  templateUrl: './home-special-offers.component.html',
  styleUrl: './home-special-offers.component.scss',
})
export class HomeSpecialOffersComponent {
  @Input({ required: true }) offers: PublicSpecialOfferCardDto[] = [];

  private _cdr = inject(ChangeDetectorRef);
  private _failedLogoIds = new Set<number>();

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

  showLabThumbnail(offer: PublicSpecialOfferCardDto): boolean {
    return !!offer.labName?.trim();
  }

  onLogoError(offerId: number): void {
    this._failedLogoIds.add(offerId);
    this._cdr.markForCheck();
  }
}
