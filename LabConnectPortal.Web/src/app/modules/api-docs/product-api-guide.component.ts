import { ChangeDetectionStrategy, Component } from '@angular/core';

@Component({
  selector: 'app-product-api-guide',
  standalone: true,
  templateUrl: './product-api-guide.component.html',
  styleUrl: './product-api-guide.component.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
  host: {
    dir: 'rtl',
  },
})
export class ProductApiGuideComponent {}
