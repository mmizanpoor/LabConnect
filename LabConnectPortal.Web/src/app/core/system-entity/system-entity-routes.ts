import { SystemEntity } from './system-entity';

/** Longest-prefix match from current app URL to a SystemEntity GUID. */
const ROUTE_ENTITY_PREFIXES: ReadonlyArray<{ prefix: string; entity: string }> = [
  // Profile
  { prefix: '/profile/agreement-settings', entity: SystemEntity.LabAgreement },
  { prefix: '/profile/api-keys', entity: SystemEntity.CenterProfile },
  { prefix: '/profile/agreements', entity: SystemEntity.LabAgreement },
  { prefix: '/profile/dashboard', entity: SystemEntity.LabAgreement },
  { prefix: '/profile/lab-users', entity: SystemEntity.LabUser },
  { prefix: '/profile/device-groups', entity: SystemEntity.DeviceGroup },
  { prefix: '/profile/kit-groups', entity: SystemEntity.KitGroup },
  { prefix: '/profile/test-infos', entity: SystemEntity.TestInfo },
  { prefix: '/profile/special-offers', entity: SystemEntity.SpecialOffer },
  { prefix: '/profile/center', entity: SystemEntity.CenterProfile },
  { prefix: '/profile/locations', entity: SystemEntity.Location },
  { prefix: '/profile/job-postings', entity: SystemEntity.JobPosting },
  { prefix: '/profile/product-orders', entity: SystemEntity.ProductOrder },
  { prefix: '/profile/product-listings', entity: SystemEntity.Product },
  { prefix: '/profile/resume-applications', entity: SystemEntity.Profile },
  { prefix: '/profile/resume', entity: SystemEntity.Resume },
  { prefix: '/profile/messages', entity: SystemEntity.Message },
  { prefix: '/profile/activity-logs', entity: SystemEntity.ActivityLog },
  { prefix: '/profile/login-report', entity: SystemEntity.Profile },
  { prefix: '/profile/change-username', entity: SystemEntity.Profile },
  { prefix: '/profile/change-password', entity: SystemEntity.Profile },
  { prefix: '/profile', entity: SystemEntity.Profile },

  // Public catalog / job board (authenticated resume apply still hits UserProfile APIs)
  { prefix: '/products', entity: SystemEntity.Product },
  { prefix: '/jobs', entity: SystemEntity.JobPosting },

  // Storefront (authenticated cart/checkout/orders without profile/admin entity context)
  { prefix: '/cart', entity: SystemEntity.ProductOrder },
  { prefix: '/checkout', entity: SystemEntity.ProductOrder },
  { prefix: '/orders', entity: SystemEntity.ProductOrder },

  // Admin
  { prefix: '/admin/dashboard', entity: SystemEntity.Dashboard },
  { prefix: '/admin/site-users', entity: SystemEntity.SiteUser },
  { prefix: '/admin/api-keys', entity: SystemEntity.User },
  { prefix: '/admin/users', entity: SystemEntity.User },
  { prefix: '/admin/shops', entity: SystemEntity.Shop },
  { prefix: '/admin/laboratories', entity: SystemEntity.Laboratory },
  { prefix: '/admin/product-categories', entity: SystemEntity.ProductCategory },
  { prefix: '/admin/product-attributes', entity: SystemEntity.ProductAttribute },
  { prefix: '/admin/brands', entity: SystemEntity.Brand },
  { prefix: '/admin/products', entity: SystemEntity.Product },
  { prefix: '/admin/content-groups', entity: SystemEntity.ContentGroup },
  { prefix: '/admin/posts', entity: SystemEntity.Post },
  { prefix: '/admin/content/news', entity: SystemEntity.ContentNews },
  { prefix: '/admin/content/articles', entity: SystemEntity.ContentArticles },
  { prefix: '/admin/content/documents', entity: SystemEntity.ContentDocuments },
  { prefix: '/admin/ad-orders', entity: SystemEntity.ContentAds },
  { prefix: '/admin/category-prices', entity: SystemEntity.ContentAds },
  { prefix: '/admin/ad-prices', entity: SystemEntity.ContentAds },
  { prefix: '/admin/ad-positions', entity: SystemEntity.ContentAds },
  { prefix: '/admin/ads', entity: SystemEntity.ContentAds },
  { prefix: '/admin/slider-groups', entity: SystemEntity.SliderGroup },
  { prefix: '/admin/receptions', entity: SystemEntity.Reception },
  { prefix: '/admin/special-offers', entity: SystemEntity.SpecialOffer },
  { prefix: '/admin/activity-logs', entity: SystemEntity.ActivityLog },
  { prefix: '/admin/login-reports', entity: SystemEntity.LoginReport },
  { prefix: '/admin/login-report', entity: SystemEntity.Profile },
  { prefix: '/admin/messages', entity: SystemEntity.Message },
  { prefix: '/admin/site-services', entity: SystemEntity.Settings },
  { prefix: '/admin/site-charge-services', entity: SystemEntity.Settings },
  { prefix: '/admin/settings', entity: SystemEntity.Settings },
  { prefix: '/admin/account', entity: SystemEntity.Profile },
  { prefix: '/admin/change-username', entity: SystemEntity.Profile },
  { prefix: '/admin/change-password', entity: SystemEntity.Profile },
  { prefix: '/admin', entity: SystemEntity.Dashboard },
];

export function resolveSystemEntityFromUrl(url: string): string | null {
  const path = (url.split('?')[0] || '').replace(/\/+$/, '') || '/';

  let best: { prefix: string; entity: string } | null = null;
  for (const entry of ROUTE_ENTITY_PREFIXES) {
    const matches =
      path === entry.prefix || path.startsWith(`${entry.prefix}/`);
    if (matches && (!best || entry.prefix.length > best.prefix.length)) {
      best = entry;
    }
  }

  return best?.entity ?? null;
}
