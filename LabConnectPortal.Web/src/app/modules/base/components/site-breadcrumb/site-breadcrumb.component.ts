import { AsyncPipe } from '@angular/common';
import { Component, inject } from '@angular/core';
import { RouterLink } from '@angular/router';
import { TranslocoPipe } from '@jsverse/transloco';
import { BreadcrumbService } from '@core/services/breadcrumb/breadcrumb.service';
import { BreadcrumbItem } from '@core/services/breadcrumb/breadcrumb.types';

@Component({
  selector: 'app-site-breadcrumb',
  standalone: true,
  imports: [AsyncPipe, RouterLink, TranslocoPipe],
  templateUrl: './site-breadcrumb.component.html',
})
export class SiteBreadcrumbComponent {
  private _breadcrumb = inject(BreadcrumbService);

  items$ = this._breadcrumb.items$;

  displayLabel(item: BreadcrumbItem): string {
    return item.label ?? '';
  }
}
