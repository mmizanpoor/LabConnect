import { SiteSettingsDto, SiteUsefulLinkCommand, UpdateSiteSettingsCommand } from './site-settings.types';

export function toUpdateSiteSettingsCommand(
  settings: SiteSettingsDto,
  patch: Partial<UpdateSiteSettingsCommand> = {}
): UpdateSiteSettingsCommand {
  return {
    siteTitle: settings.siteTitle,
    tagline: settings.tagline,
    supportLandline: settings.supportLandline,
    supportMobile: settings.supportMobile,
    enamadEmbedCode: settings.enamadEmbedCode ?? null,
    enamadLinkUrl: settings.enamadLinkUrl ?? null,
    googleMapEmbedCode: settings.googleMapEmbedCode ?? null,
    metaTitle: settings.metaTitle,
    metaDescription: settings.metaDescription,
    metaKeywords: settings.metaKeywords,
    metaViewport: settings.metaViewport ?? null,
    metaCanonical: settings.metaCanonical ?? null,
    footerAboutText: settings.footerAboutText,
    footerAddress: settings.footerAddress,
    footerEmail: settings.footerEmail,
    footerCopyrightText: settings.footerCopyrightText,
    usefulLinks: (settings.usefulLinks ?? []).map(
      (link, index): SiteUsefulLinkCommand => ({
        id: link.id,
        title: link.title,
        url: link.url,
        sortOrder: link.sortOrder ?? index,
      })
    ),
    ...patch,
  };
}
