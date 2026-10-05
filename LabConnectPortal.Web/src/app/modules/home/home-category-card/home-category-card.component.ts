import { Component, Input } from '@angular/core';
import { RouterLink } from '@angular/router';
import { PublicCategoriesService } from '../../products/public-categories.service';
import { PublicCategoryGroupCardDto } from '../../products/products.types';

@Component({
  selector: 'app-home-category-card',
  standalone: true,
  imports: [RouterLink],
  templateUrl: './home-category-card.component.html',
  styleUrl: './home-category-card.component.scss',
})
export class HomeCategoryCardComponent {
  @Input({ required: true }) category!: PublicCategoryGroupCardDto;

  imageUrl(): string {
    return PublicCategoriesService.categoryHomePageImageUrl(
      this.category.homePageImagePath,
      240
    );
  }
}
