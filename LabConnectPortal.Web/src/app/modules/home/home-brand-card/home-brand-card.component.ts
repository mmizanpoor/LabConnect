import { Component, Input } from '@angular/core';
import { RouterLink } from '@angular/router';
import { PublicBrandsService } from '../../products/public-brands.service';
import { PublicBrandCardDto } from '../../products/products.types';

@Component({
  selector: 'app-home-brand-card',
  standalone: true,
  imports: [RouterLink],
  templateUrl: './home-brand-card.component.html',
  host: {
    class: 'block min-w-0',
  },
})
export class HomeBrandCardComponent {
  @Input({ required: true }) brand!: PublicBrandCardDto;

  imageUrl(): string {
    return PublicBrandsService.brandImageUrl(this.brand.imagePath, 240);
  }
}
