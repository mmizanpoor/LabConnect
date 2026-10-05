export interface AdvertisementPositionDto {
  id: number;
  code: string;
  title: string;
  description: string;
  maxConcurrentSlots: number;
  maxDisplayCount?: number | null;
  isActive: boolean;
  createdAt: string;
}

export interface SaveAdvertisementPositionCommand {
  code: string;
  title: string;
  description: string;
  maxConcurrentSlots: number;
  maxDisplayCount?: number | null;
  isActive: boolean;
}

export interface UpdateAdvertisementPositionCommand extends SaveAdvertisementPositionCommand {
  id: number;
}

export interface AdvertisementDurationDto {
  id: number;
  code: string;
  title: string;
  daysCount: number;
  sortOrder: number;
}

export interface AdvertisementPriceDto {
  id: string;
  advertisementPositionId: number;
  positionTitle: string;
  advertisementDurationId: number;
  durationTitle: string;
  price: number;
  validFrom: string;
  validTo?: string | null;
  createdAt: string;
  createdByUserId: string;
  createdByUserName: string;
  isCurrent: boolean;
}

export interface AdvertisementPriceMatrixCellDto {
  advertisementPositionId: number;
  advertisementDurationId: number;
  currentPrice?: number | null;
  currentPriceId?: string | null;
  validFrom?: string | null;
}

export interface AdvertisementPriceMatrixDto {
  positions: AdvertisementPositionDto[];
  durations: AdvertisementDurationDto[];
  cells: AdvertisementPriceMatrixCellDto[];
}

export interface SetAdvertisementPriceCommand {
  advertisementPositionId: number;
  advertisementDurationId: number;
  price: number;
  validFrom?: string | null;
}

export const AdvertisementOrderStatus = {
  Draft: 0,
  PendingPayment: 1,
  Paid: 2,
  Active: 3,
  Expired: 4,
  Cancelled: 5,
} as const;

export type AdvertisementOrderStatus =
  (typeof AdvertisementOrderStatus)[keyof typeof AdvertisementOrderStatus];

export interface AdvertisementOrderDto {
  id: string;
  userId: string;
  userDisplayName: string;
  advertisementPositionId: number;
  positionTitle: string;
  advertisementDurationId: number;
  durationTitle: string;
  advertisementPriceId: string;
  price: number;
  startDate: string;
  endDate: string;
  status: AdvertisementOrderStatus;
  statusTitle: string;
  createdAt: string;
}

export interface SaveAdvertisementOrderCommand {
  userId: string;
  advertisementPositionId: number;
  advertisementDurationId: number;
  startDate: string;
  status: AdvertisementOrderStatus;
}

export interface UpdateAdvertisementOrderStatusCommand {
  id: string;
  status: AdvertisementOrderStatus;
  startDate?: string | null;
}

export const ADVERTISEMENT_ORDER_STATUS_OPTIONS: AdvertisementOrderStatus[] = [
  AdvertisementOrderStatus.Draft,
  AdvertisementOrderStatus.PendingPayment,
  AdvertisementOrderStatus.Paid,
  AdvertisementOrderStatus.Active,
  AdvertisementOrderStatus.Expired,
  AdvertisementOrderStatus.Cancelled,
];

export function advertisementOrderStatusLabelKey(status: AdvertisementOrderStatus): string {
  switch (status) {
    case AdvertisementOrderStatus.Draft:
      return 'modules.admin.adCommerce.orderStatus.draft';
    case AdvertisementOrderStatus.PendingPayment:
      return 'modules.admin.adCommerce.orderStatus.pendingPayment';
    case AdvertisementOrderStatus.Paid:
      return 'modules.admin.adCommerce.orderStatus.paid';
    case AdvertisementOrderStatus.Active:
      return 'modules.admin.adCommerce.orderStatus.active';
    case AdvertisementOrderStatus.Expired:
      return 'modules.admin.adCommerce.orderStatus.expired';
    case AdvertisementOrderStatus.Cancelled:
      return 'modules.admin.adCommerce.orderStatus.cancelled';
    default:
      return 'shared.unknown';
  }
}
