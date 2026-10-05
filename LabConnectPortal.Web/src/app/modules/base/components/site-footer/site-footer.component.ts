import { Component, OnInit, inject } from '@angular/core';
import { DomSanitizer, SafeHtml } from '@angular/platform-browser';
import { RouterLink } from '@angular/router';
import { TranslocoPipe } from '@jsverse/transloco';
import { SiteContextService } from '@core/services/site/site-context.service';
import { PublicSiteService } from '@core/services/site/public-site.service';
import { SiteSettingsPublicDto, SiteUsefulLinkDto } from '@modules/admin/settings/site-settings.types';

@Component({
  selector: 'app-site-footer',
  standalone: true,
  imports: [RouterLink, TranslocoPipe],
  templateUrl: './site-footer.component.html',
})
export class SiteFooterComponent implements OnInit {
  private _siteContext = inject(SiteContextService);
  private _sanitizer = inject(DomSanitizer);

  settings: SiteSettingsPublicDto | null = null;
  enamadHtml: SafeHtml | null = null;
  footerLogoUrl: string | null = null;

  get hasTrustBadge(): boolean {
    return !!(this.enamadHtml || this.settings?.enamadLinkUrl?.trim());
  }

  async ngOnInit(): Promise<void> {
    this.settings = await this._siteContext.ensureLoaded();
    if (this.settings?.enamadEmbedCode?.trim()) {
      this.enamadHtml = this._sanitizer.bypassSecurityTrustHtml(this.settings.enamadEmbedCode);
    }

    if (this.settings?.hasFooterLogo) {
      this.footerLogoUrl = PublicSiteService.footerLogoUrl();
    }
  }

  isExternalLink(url: string): boolean {
    return /^https?:\/\//i.test(url.trim());
  }

  trackLink(_index: number, link: SiteUsefulLinkDto): string {
    return link.id;
  }
}
