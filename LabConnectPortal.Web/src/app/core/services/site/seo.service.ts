import { Injectable, inject } from '@angular/core';
import { Meta, Title } from '@angular/platform-browser';
import { PublicPostDetailDto } from '@modules/admin/content/content.types';
import { SiteSettingsPublicDto } from '@modules/admin/settings/site-settings.types';
import { PublicSiteService } from './public-site.service';

@Injectable({ providedIn: 'root' })
export class SeoService {
  private _title = inject(Title);
  private _meta = inject(Meta);

  applySettings(settings: SiteSettingsPublicDto): void {
    const pageTitle = settings.metaTitle?.trim() || settings.siteTitle?.trim() || 'LabConnect Portal';
    this._title.setTitle(pageTitle);

    this.setMetaTag('description', settings.metaDescription);
    this.setMetaTag('keywords', settings.metaKeywords);
    if (settings.metaViewport?.trim()) {
      this.setMetaTag('viewport', settings.metaViewport.trim());
    }

    this.setLinkTag('canonical', settings.metaCanonical);
    this.applyFavicon(settings.hasLogo);
  }

  applyPost(post: PublicPostDetailDto): void {
    const pageTitle = post.browserTitle?.trim() || post.title?.trim() || 'LabConnect Portal';
    this._title.setTitle(pageTitle);

    this.setMetaTag('description', post.metaDescription);
    this.setMetaTag('keywords', post.metaKeywords);
    this.applyCustomMetaTags(post.customMetaTags);
  }

  private applyFavicon(hasLogo: boolean): void {
    const href = hasLogo
      ? `${PublicSiteService.logoUrl()}?v=${Date.now()}`
      : 'favicon.ico';

    document
      .querySelectorAll<HTMLLinkElement>("link[rel='icon'], link[rel='shortcut icon']")
      .forEach((link) => link.remove());

    const link = document.createElement('link');
    link.setAttribute('rel', 'icon');
    link.setAttribute('href', href);
    if (!hasLogo) {
      link.setAttribute('type', 'image/x-icon');
    }
    document.head.appendChild(link);
  }

  private applyCustomMetaTags(raw?: string | null): void {
    document.querySelectorAll('[data-post-custom-meta="true"]').forEach((node) => node.remove());
    const value = raw?.trim();
    if (!value) return;

    const template = document.createElement('template');
    template.innerHTML = value;
    template.content.querySelectorAll('meta, link').forEach((node) => {
      const clone = node.cloneNode(true) as HTMLElement;
      clone.setAttribute('data-post-custom-meta', 'true');
      document.head.appendChild(clone);
    });
  }

  private setMetaTag(name: string, content?: string | null): void {
    const value = content?.trim();
    if (!value) {
      if (this._meta.getTag(`name='${name}'`)) {
        this._meta.removeTag(`name='${name}'`);
      }
      return;
    }

    if (this._meta.getTag(`name='${name}'`)) {
      this._meta.updateTag({ name, content: value });
    } else {
      this._meta.addTag({ name, content: value });
    }
  }

  private setLinkTag(rel: string, href?: string | null): void {
    const value = href?.trim();
    const selector = `link[rel='${rel}']`;
    const existing = document.querySelector(selector);

    if (!value) {
      existing?.remove();
      return;
    }

    if (existing) {
      existing.setAttribute('href', value);
      return;
    }

    const link = document.createElement('link');
    link.setAttribute('rel', rel);
    link.setAttribute('href', value);
    document.head.appendChild(link);
  }
}
