import { Component, OnInit, inject } from '@angular/core';
import { DomSanitizer, SafeHtml } from '@angular/platform-browser';
import { MatIconModule } from '@angular/material/icon';
import { TranslocoPipe } from '@jsverse/transloco';
import { SiteContextService } from '@core/services/site/site-context.service';
import { SiteSettingsPublicDto } from '@modules/admin/settings/site-settings.types';

@Component({
  selector: 'app-contact-us',
  standalone: true,
  imports: [MatIconModule, TranslocoPipe],
  templateUrl: './contact-us.component.html',
})
export class ContactUsComponent implements OnInit {
  private _siteContext = inject(SiteContextService);
  private _sanitizer = inject(DomSanitizer);

  loading = true;
  settings: SiteSettingsPublicDto | null = null;
  googleMapHtml: SafeHtml | null = null;

  get hasContactInfo(): boolean {
    if (!this.settings) return false;

    return !!(
      this.settings.footerAddress?.trim()
      || this.settings.supportLandline?.trim()
      || this.settings.supportMobile?.trim()
      || this.settings.footerEmail?.trim()
    );
  }

  get hasGoogleMap(): boolean {
    return !!this.googleMapHtml;
  }

  async ngOnInit(): Promise<void> {
    try {
      this.settings = await this._siteContext.ensureLoaded();
      if (this.settings?.googleMapEmbedCode?.trim())
        this.googleMapHtml = this._sanitizer.bypassSecurityTrustHtml(this.settings.googleMapEmbedCode);
    } finally {
      this.loading = false;
    }
  }
}
