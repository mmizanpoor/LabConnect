import { SystemEntity } from '@core/system-entity/system-entity';

export interface AdminNavLink {
  route: string;
  icon: string;
  labelKey: string;
  exact?: boolean;
  /** SystemEntity GUID for permission gating (Admin users). */
  systemEntity?: string;
  /** Visible only to full Administrator (not limited Admin). */
  administratorOnly?: boolean;
}

export interface AdminNavGroup {
  id: string;
  icon: string;
  labelKey: string;
  items: AdminNavLink[];
}

export interface AdminNavStandalone {
  route: string;
  icon: string;
  labelKey: string;
  exact?: boolean;
  systemEntity?: string;
  administratorOnly?: boolean;
}

export interface AdminRouteTitleOverride {
  match: (url: string) => boolean;
  labelKey: string;
}

export const ADMIN_DASHBOARD_LINK: AdminNavLink = {
  route: '/admin/dashboard',
  icon: 'dashboard',
  labelKey: 'modules.admin.layout.dashboard',
};

export const ADMIN_ROUTE_TITLE_OVERRIDES: AdminRouteTitleOverride[] = [
  {
    match: (url) => url === '/admin/change-username',
    labelKey: 'modules.profile.accountSecurity.changeUsernameTitle',
  },
  {
    match: (url) => url === '/admin/change-password',
    labelKey: 'modules.profile.accountSecurity.changePasswordTitle',
  },
  {
    match: (url) => /^\/admin\/shops\/[^/]+$/.test(url),
    labelKey: 'modules.admin.shops.detailTitle',
  },
  {
    match: (url) => /^\/admin\/laboratories\/[^/]+$/.test(url),
    labelKey: 'modules.admin.laboratories.detailTitle',
  },
  {
    match: (url) => /^\/admin\/special-offers\/[^/]+$/.test(url),
    labelKey: 'modules.admin.specialOffers.detailTitle',
  },
  {
    match: (url) => url === '/admin/posts/new',
    labelKey: 'modules.admin.posts.add',
  },
  {
    match: (url) =>
      /^\/admin\/posts\/[^/]+$/.test(url) && url !== '/admin/posts/new',
    labelKey: 'modules.admin.posts.editTitle',
  },
  {
    match: (url) => url === '/admin/content/news/new',
    labelKey: 'modules.admin.layout.contentNews',
  },
  {
    match: (url) =>
      /^\/admin\/content\/news\/[^/]+$/.test(url) &&
      url !== '/admin/content/news/new',
    labelKey: 'modules.admin.layout.contentNews',
  },
  {
    match: (url) => url === '/admin/content/articles/new',
    labelKey: 'modules.admin.layout.contentArticles',
  },
  {
    match: (url) =>
      /^\/admin\/content\/articles\/[^/]+$/.test(url) &&
      url !== '/admin/content/articles/new',
    labelKey: 'modules.admin.layout.contentArticles',
  },
  {
    match: (url) => url === '/admin/content/documents/new',
    labelKey: 'modules.admin.layout.contentDocuments',
  },
  {
    match: (url) =>
      /^\/admin\/content\/documents\/[^/]+$/.test(url) &&
      url !== '/admin/content/documents/new',
    labelKey: 'modules.admin.layout.contentDocuments',
  },
  {
    match: (url) => url === '/admin/ads/new',
    labelKey: 'modules.admin.advertisements.add',
  },
  {
    match: (url) =>
      /^\/admin\/ads\/[^/]+$/.test(url) && url !== '/admin/ads/new',
    labelKey: 'modules.admin.advertisements.editTitle',
  },
  {
    match: (url) => url === '/admin/ad-positions/new',
    labelKey: 'modules.admin.adCommerce.positions.add',
  },
  {
    match: (url) =>
      /^\/admin\/ad-positions\/[^/]+$/.test(url) &&
      url !== '/admin/ad-positions/new',
    labelKey: 'modules.admin.adCommerce.positions.editTitle',
  },
  {
    match: (url) => url === '/admin/ad-orders/new',
    labelKey: 'modules.admin.adCommerce.orders.add',
  },
  {
    match: (url) =>
      /^\/admin\/ad-orders\/[^/]+$/.test(url) && url !== '/admin/ad-orders/new',
    labelKey: 'modules.admin.adCommerce.orders.editTitle',
  },
  {
    match: (url) => url === '/admin/slider-groups/new',
    labelKey: 'modules.admin.sliderGroups.add',
  },
  {
    match: (url) =>
      /^\/admin\/slider-groups\/[^/]+$/.test(url) &&
      url !== '/admin/slider-groups/new',
    labelKey: 'modules.admin.sliderGroups.edit',
  },
  {
    match: (url) => url === '/admin/company-regulations/new',
    labelKey: 'modules.admin.companyRegulations.add',
  },
  {
    match: (url) =>
      /^\/admin\/company-regulations\/[^/]+$/.test(url) &&
      url !== '/admin/company-regulations/new',
    labelKey: 'modules.admin.companyRegulations.editTitle',
  },
  {
    match: (url) => url === '/admin/products/new',
    labelKey: 'modules.admin.products.add',
  },
  {
    match: (url) => /^\/admin\/products\/[^/]+\/edit$/.test(url),
    labelKey: 'modules.admin.products.editTitle',
  },
  {
    match: (url) =>
      /^\/admin\/products\/[^/]+$/.test(url) && url !== '/admin/products/new',
    labelKey: 'modules.admin.products.reviewTitle',
  },
];

export const ADMIN_NAV_GROUPS: AdminNavGroup[] = [
  {
    id: 'users',
    icon: 'groups',
    labelKey: 'modules.admin.layout.groups.users',
    items: [
      {
        route: '/admin/users',
        icon: 'people',
        labelKey: 'modules.admin.layout.users',
        systemEntity: SystemEntity.User,
      },
      {
        route: '/admin/api-keys',
        icon: 'key',
        labelKey: 'shared.apiKeys.adminTitle',
        systemEntity: SystemEntity.User,
      },
      {
        route: '/admin/site-users',
        icon: 'admin_panel_settings',
        labelKey: 'modules.admin.layout.siteUsers',
        systemEntity: SystemEntity.SiteUser,
        administratorOnly: true,
      },
      {
        route: '/admin/shops',
        icon: 'storefront',
        labelKey: 'modules.admin.layout.shops',
        systemEntity: SystemEntity.Shop,
      },
      {
        route: '/admin/laboratories',
        icon: 'science',
        labelKey: 'modules.admin.layout.laboratories',
        systemEntity: SystemEntity.Laboratory,
      },
    ],
  },
  {
    id: 'products',
    icon: 'inventory_2',
    labelKey: 'modules.admin.layout.groups.products',
    items: [
      {
        route: '/admin/product-categories',
        icon: 'category',
        labelKey: 'modules.admin.layout.productCategories',
        systemEntity: SystemEntity.ProductCategory,
      },
      {
        route: '/admin/product-attributes',
        icon: 'tune',
        labelKey: 'modules.admin.layout.productAttributes',
        systemEntity: SystemEntity.ProductAttribute,
      },
      {
        route: '/admin/brands',
        icon: 'label',
        labelKey: 'modules.admin.layout.brands',
        systemEntity: SystemEntity.Brand,
      },
      {
        route: '/admin/products',
        icon: 'shopping_bag',
        labelKey: 'modules.admin.layout.products',
        systemEntity: SystemEntity.Product,
      },
    ],
  },
  {
    id: 'content',
    icon: 'article',
    labelKey: 'modules.admin.layout.groups.content',
    items: [
      {
        route: '/admin/content-groups',
        icon: 'folder_open',
        labelKey: 'modules.admin.layout.contentGroups',
        systemEntity: SystemEntity.ContentGroup,
      },
      {
        route: '/admin/posts',
        icon: 'description',
        labelKey: 'modules.admin.layout.posts',
        systemEntity: SystemEntity.Post,
      },
      {
        route: '/admin/content/news',
        icon: 'newspaper',
        labelKey: 'modules.admin.layout.contentNews',
        systemEntity: SystemEntity.ContentNews,
      },
      {
        route: '/admin/content/articles',
        icon: 'menu_book',
        labelKey: 'modules.admin.layout.contentArticles',
        systemEntity: SystemEntity.ContentArticles,
      },
      {
        route: '/admin/content/documents',
        icon: 'folder_shared',
        labelKey: 'modules.admin.layout.contentDocuments',
        systemEntity: SystemEntity.ContentDocuments,
      },
      {
        route: '/admin/ads',
        icon: 'campaign',
        labelKey: 'modules.admin.layout.contentAds',
        systemEntity: SystemEntity.ContentAds,
      },
      {
        route: '/admin/slider-groups',
        icon: 'view_carousel',
        labelKey: 'modules.admin.layout.sliderGroups',
        systemEntity: SystemEntity.SliderGroup,
      },
      {
        route: '/admin/company-regulations',
        icon: 'gavel',
        labelKey: 'modules.admin.layout.companyRegulations',
        systemEntity: SystemEntity.CompanyRegulation,
      },
    ],
  },
  {
    id: 'ads',
    icon: 'campaign',
    labelKey: 'modules.admin.layout.groups.ads',
    items: [
      {
        route: '/admin/ad-positions',
        icon: 'view_quilt',
        labelKey: 'modules.admin.layout.adPositions',
        systemEntity: SystemEntity.ContentAds,
      },
      {
        route: '/admin/ad-prices',
        icon: 'payments',
        labelKey: 'modules.admin.layout.adPrices',
        systemEntity: SystemEntity.ContentAds,
      },
      {
        route: '/admin/ad-orders',
        icon: 'receipt_long',
        labelKey: 'modules.admin.layout.adOrders',
        systemEntity: SystemEntity.ContentAds,
      },
      {
        route: '/admin/category-prices',
        icon: 'sell',
        labelKey: 'modules.admin.layout.categoryPrices',
        systemEntity: SystemEntity.ContentAds,
      },
    ],
  },
  {
    id: 'settings',
    icon: 'settings',
    labelKey: 'modules.admin.layout.groups.settings',
    items: [
      {
        route: '/admin/settings',
        icon: 'tune',
        labelKey: 'modules.admin.layout.settings',
        exact: true,
        systemEntity: SystemEntity.Settings,
      },
      {
        route: '/admin/settings/seo',
        icon: 'travel_explore',
        labelKey: 'modules.admin.layout.settingsSeo',
        systemEntity: SystemEntity.Settings,
      },
      {
        route: '/admin/settings/useful-links',
        icon: 'link',
        labelKey: 'modules.admin.layout.settingsUsefulLinks',
        systemEntity: SystemEntity.Settings,
      },
      {
        route: '/admin/site-services',
        icon: 'home_repair_service',
        labelKey: 'modules.admin.layout.siteServices',
        systemEntity: SystemEntity.SiteService,
      },
      {
        route: '/admin/site-charge-services',
        icon: 'payments',
        labelKey: 'modules.admin.layout.siteChargeServices',
        systemEntity: SystemEntity.Settings,
      },
    ],
  },
];

export const ADMIN_NAV_STANDALONE: AdminNavStandalone[] = [
  {
    route: '/admin/receptions',
    icon: 'biotech',
    labelKey: 'modules.admin.layout.receptions',
    administratorOnly: true,
  },
  {
    route: '/admin/special-offers',
    icon: 'local_offer',
    labelKey: 'modules.admin.layout.specialOffers',
    systemEntity: SystemEntity.SpecialOffer,
  },
  {
    route: '/admin/activity-logs',
    icon: 'history',
    labelKey: 'modules.admin.layout.activityLogs',
    systemEntity: SystemEntity.ActivityLog,
  },
  {
    route: '/admin/login-reports',
    icon: 'login',
    labelKey: 'modules.admin.layout.loginReports',
    systemEntity: SystemEntity.LoginReport,
  },
  {
    route: '/admin/messages',
    icon: 'mail_outline',
    labelKey: 'modules.admin.layout.messages',
    systemEntity: SystemEntity.Message,
  },
  {
    route: '/admin/account',
    icon: 'person',
    labelKey: 'modules.admin.layout.account',
  },
];
