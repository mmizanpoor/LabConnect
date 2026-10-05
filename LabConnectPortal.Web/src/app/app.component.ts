import { DOCUMENT } from '@angular/common';
import { Component, OnInit, inject } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { SiteContextService } from '@core/services/site/site-context.service';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [RouterOutlet],
  template: '<router-outlet />',
  styles: ':host { display: block; height: 100%; }',
})
export class AppComponent implements OnInit {
  private _document = inject(DOCUMENT);
  private _siteContext = inject(SiteContextService);

  ngOnInit(): void {
    this._document.documentElement.setAttribute('dir', 'rtl');
    this._document.documentElement.setAttribute('lang', 'fa');
    void this._siteContext.ensureLoaded();
  }
}
