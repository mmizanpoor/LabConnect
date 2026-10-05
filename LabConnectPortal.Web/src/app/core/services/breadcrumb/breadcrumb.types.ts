export interface BreadcrumbItem {
  label?: string;
  i18nKey?: string;
  url?: string | null;
}

export interface BreadcrumbConfigItem {
  i18nKey?: string;
  label?: string;
  url?: string;
  dynamic?: boolean;
}

export type BreadcrumbRouteEntry =
  | 'skip'
  | 'dynamic'
  | string
  | BreadcrumbConfigItem[];

export const BREADCRUMB_DATA_KEY = 'breadcrumb';
