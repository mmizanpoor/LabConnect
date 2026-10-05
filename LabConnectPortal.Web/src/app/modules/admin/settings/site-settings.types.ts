export interface SiteUsefulLinkDto {
  id: string;
  title: string;
  url: string;
  sortOrder: number;
}

export interface SiteUsefulLinkCommand {
  id?: string | null;
  title: string;
  url: string;
  sortOrder: number;
}

export interface SiteSettingsDto {
  id: number;
  siteTitle: string;
  tagline: string;
  hasLogo: boolean;
  hasFooterLogo: boolean;
  supportLandline: string;
  supportMobile: string;
  enamadEmbedCode?: string | null;
  enamadLinkUrl?: string | null;
  googleMapEmbedCode?: string | null;
  metaTitle: string;
  metaDescription: string;
  metaKeywords: string;
  metaViewport?: string | null;
  metaCanonical?: string | null;
  footerAboutText: string;
  footerAddress: string;
  footerEmail: string;
  footerCopyrightText: string;
  usefulLinks: SiteUsefulLinkDto[];
  updatedAt: string;
}

export interface UpdateSiteSettingsCommand {
  siteTitle: string;
  tagline: string;
  supportLandline: string;
  supportMobile: string;
  enamadEmbedCode?: string | null;
  enamadLinkUrl?: string | null;
  googleMapEmbedCode?: string | null;
  metaTitle: string;
  metaDescription: string;
  metaKeywords: string;
  metaViewport?: string | null;
  metaCanonical?: string | null;
  footerAboutText: string;
  footerAddress: string;
  footerEmail: string;
  footerCopyrightText: string;
  usefulLinks: SiteUsefulLinkCommand[];
}

export interface SiteSettingsPublicDto {
  siteTitle: string;
  tagline: string;
  hasLogo: boolean;
  hasFooterLogo: boolean;
  supportLandline: string;
  supportMobile: string;
  enamadEmbedCode?: string | null;
  enamadLinkUrl?: string | null;
  googleMapEmbedCode?: string | null;
  metaTitle: string;
  metaDescription: string;
  metaKeywords: string;
  metaViewport?: string | null;
  metaCanonical?: string | null;
  footerAboutText: string;
  footerAddress: string;
  footerEmail: string;
  footerCopyrightText: string;
  usefulLinks: SiteUsefulLinkDto[];
}
