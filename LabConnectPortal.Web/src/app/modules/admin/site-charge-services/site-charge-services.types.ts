export enum SiteChargeServiceCode {
  Sms = 1,
}

export enum SiteChargePricingMode {
  Fixed = 0,
  Range = 1,
  Package = 2,
}

export interface SiteChargeServiceAdminDto {
  siteChargeServiceId: number;
  code: SiteChargeServiceCode;
  title: string;
  description?: string | null;
  pricingMode: SiteChargePricingMode;
  isActive: boolean;
  updatedAt: string;
  prices: SiteChargeServicePriceAdminDto[];
}

export interface SiteChargeServicePriceAdminDto {
  siteChargeServicePriceId: number;
  title?: string | null;
  minQuantity?: number | null;
  maxQuantity?: number | null;
  packageQuantity?: number | null;
  price: number;
  sortOrder: number;
}

export interface SaveSiteChargeServiceCommand {
  siteChargeServiceId?: number | null;
  code: SiteChargeServiceCode;
  title: string;
  description?: string | null;
  pricingMode: SiteChargePricingMode;
  isActive: boolean;
  prices: SaveSiteChargeServicePriceItem[];
}

export interface SaveSiteChargeServicePriceItem {
  title?: string | null;
  minQuantity?: number | null;
  maxQuantity?: number | null;
  packageQuantity?: number | null;
  price: number;
  sortOrder: number;
}

export const SITE_CHARGE_SERVICE_CODE_LABELS: Record<SiteChargeServiceCode, string> = {
  [SiteChargeServiceCode.Sms]: 'modules.admin.siteChargeServices.codes.sms',
};

export const SITE_CHARGE_PRICING_MODE_LABELS: Record<SiteChargePricingMode, string> = {
  [SiteChargePricingMode.Fixed]: 'modules.admin.siteChargeServices.pricingModes.fixed',
  [SiteChargePricingMode.Range]: 'modules.admin.siteChargeServices.pricingModes.range',
  [SiteChargePricingMode.Package]: 'modules.admin.siteChargeServices.pricingModes.package',
};

export const SITE_CHARGE_SERVICE_CODES = Object.values(SiteChargeServiceCode).filter(
  (v): v is SiteChargeServiceCode => typeof v === 'number',
);

export const SITE_CHARGE_PRICING_MODES = Object.values(SiteChargePricingMode).filter(
  (v): v is SiteChargePricingMode => typeof v === 'number',
);

/** Normalize API enum that may arrive as number or string name. */
export function toSiteChargeServiceCode(value: SiteChargeServiceCode | string | number): SiteChargeServiceCode {
  if (typeof value === 'number') return value as SiteChargeServiceCode;
  if (typeof value === 'string' && value in SiteChargeServiceCode) {
    return SiteChargeServiceCode[value as keyof typeof SiteChargeServiceCode] as SiteChargeServiceCode;
  }
  return Number(value) as SiteChargeServiceCode;
}

export function toSiteChargePricingMode(value: SiteChargePricingMode | string | number): SiteChargePricingMode {
  if (typeof value === 'number') return value as SiteChargePricingMode;
  if (typeof value === 'string' && value in SiteChargePricingMode) {
    return SiteChargePricingMode[value as keyof typeof SiteChargePricingMode] as SiteChargePricingMode;
  }
  return Number(value) as SiteChargePricingMode;
}
