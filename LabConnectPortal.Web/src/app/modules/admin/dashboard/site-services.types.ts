export interface SiteServiceDto {
  siteServiceId: number;
  title: string;
  imagePath?: string | null;
  linkUrl?: string | null;
  isActive: boolean;
  sortOrder: number;
}

