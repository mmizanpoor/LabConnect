import { Component, Input } from '@angular/core';
import { RouterLink } from '@angular/router';
import { TranslocoPipe } from '@jsverse/transloco';

@Component({
  selector: 'app-home-section-header',
  standalone: true,
  imports: [RouterLink, TranslocoPipe],
  templateUrl: './home-section-header.component.html',
})
export class HomeSectionHeaderComponent {
  @Input() titleKey?: string;
  @Input() title?: string;
  @Input({ required: true }) viewAllLabelKey!: string;
  @Input({ required: true }) viewAllLink!: string;
  @Input() viewAllQueryParams?: Record<string, string | number | boolean | null | undefined>;
  @Input() queryParamsHandling: 'merge' | 'preserve' | '' = 'merge';
}
