import { Component, DestroyRef, OnInit, inject } from '@angular/core';
import { Location } from '@angular/common';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { NavigationEnd, Router, RouterOutlet } from '@angular/router';
import { MatIconModule } from '@angular/material/icon';
import { TranslocoPipe } from '@jsverse/transloco';
import { filter } from 'rxjs';
import { AzmoonProContextService } from '@core/services/site/azmoon-pro-context.service';
import { SiteContextService } from '@core/services/site/site-context.service';
import { SiteFooterComponent } from '@modules/base/components/site-footer/site-footer.component';
import { SiteHeaderComponent } from '@modules/base/components/site-header/site-header.component';
import { SiteBreadcrumbComponent } from '@modules/base/components/site-breadcrumb/site-breadcrumb.component';

@Component({
  selector: 'app-public-layout',
  standalone: true,
  imports: [
    RouterOutlet,
    MatIconModule,
    TranslocoPipe,
    SiteHeaderComponent,
    SiteBreadcrumbComponent,
    SiteFooterComponent,
  ],
  templateUrl: './public-layout.component.html',
})
export class PublicLayoutComponent implements OnInit {
  private _siteContext = inject(SiteContextService);
  private _azmoonPro = inject(AzmoonProContextService);
  private _router = inject(Router);
  private _location = inject(Location);
  private _destroyRef = inject(DestroyRef);

  hideChrome = this._azmoonPro.shouldHidePublicChrome();
  canGoBack = false;
  private historyStack: string[] = [];

  ngOnInit(): void {
    void this._siteContext.ensureLoaded();
    this._azmoonPro.hideChrome$
      .pipe(takeUntilDestroyed(this._destroyRef))
      .subscribe(() => this.syncHideChrome());

    this.trackHistory(this._router.url);
    this._router.events
      .pipe(
        filter((event): event is NavigationEnd => event instanceof NavigationEnd),
        takeUntilDestroyed(this._destroyRef),
      )
      .subscribe((event) => {
        this.syncHideChrome();
        this.trackHistory(event.urlAfterRedirects);
      });
  }

  goBack(): void {
    if (!this.canGoBack) return;
    if (this.historyStack.length > 1) {
      this._location.back();
      return;
    }

    // Deep-link from AzmoonPro to /products/:id — leave product detail.
    if (this.isProductDetailPath(this.normalizePath(this._router.url))) {
      void this._router.navigate(['/products'], {
        queryParamsHandling: 'preserve',
      });
      return;
    }

    this._location.back();
  }

  private trackHistory(url: string): void {
    const path = this.normalizePath(url);
    const last = this.historyStack.at(-1);
    const previous = this.historyStack.at(-2);

    if (previous === path) {
      this.historyStack.pop();
    } else if (last !== path) {
      this.historyStack.push(path);
    }

    this.refreshCanGoBack(path);
  }

  private syncHideChrome(): void {
    const hide = this._azmoonPro.shouldHidePublicChrome();
    this.hideChrome = hide;
    if (!hide) {
      this.historyStack = [];
      this.canGoBack = false;
      return;
    }
    this.refreshCanGoBack();
  }

  private refreshCanGoBack(path?: string): void {
    const current = path ?? this.normalizePath(this._router.url);
    this.canGoBack =
      this.hideChrome &&
      (this.historyStack.length > 1 || this.isProductDetailPath(current));
  }

  private isProductDetailPath(path: string): boolean {
    return /^\/products\/[^/]+$/.test(path);
  }

  private normalizePath(url: string): string {
    return url.split('?')[0].split('#')[0];
  }
}
