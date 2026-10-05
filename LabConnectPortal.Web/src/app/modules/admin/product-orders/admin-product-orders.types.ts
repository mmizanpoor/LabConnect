export type ProductOrderStatus =
  | 'PendingPayment'
  | 'Paid'
  | 'Completed'
  | 'Cancelled'
  | 'Shipped'
  | 'Delivered';

export type ShippingMethod = 'Post' | 'Courier' | 'Pickup';

export interface AdminProductOrderListItemDto {
  id: string;
  centerName: string;
  buyerName: string;
  buyerPhone: string;
  status: ProductOrderStatus | number;
  totalAmount: number;
  shippingCost?: number;
  shippingMethod?: ShippingMethod | number;
  trackingCode?: string | null;
  shippingCompany?: string | null;
  createdAt: string;
  itemCount: number;
}

export interface ProductOrderItemDto {
  id: string;
  itemType: 'Product';
  productTitle: string;
  quantity: number;
  unitPrice: number;
  lineTotal: number;
  shippedAt?: string | null;
  isShipped?: boolean;
  hasReview?: boolean;
  canReview?: boolean;
}

export interface ProductOrderDto {
  id: string;
  centerProfileId: string;
  centerName: string;
  status: ProductOrderStatus;
  totalAmount: number;
  shippingCost?: number;
  shippingMethod?: ShippingMethod | number;
  createdAt: string;
  paidAt?: string | null;
  completedAt?: string | null;
  shippedAt?: string | null;
  deliveredAt?: string | null;
  shippingRecipientName?: string;
  shippingAddress?: string;
  shippingPhone?: string;
  trackingCode?: string | null;
  shippingCompany?: string | null;
  buyerName?: string;
  buyerPhone?: string;
  items: ProductOrderItemDto[];
}

export interface GetAdminOrdersQuery {
  status?: ProductOrderStatus;
  search?: string;
  page?: number;
  pageSize?: number;
}

export interface PagedAdminOrders {
  items: AdminProductOrderListItemDto[];
  totalCount: number;
  page: number;
  pageSize: number;
}

export const ADMIN_ORDER_STATUSES: ProductOrderStatus[] = [
  'PendingPayment',
  'Paid',
  'Completed',
  'Shipped',
  'Delivered',
  'Cancelled',
];

const STATUS_BY_CODE: Record<number, ProductOrderStatus> = {
  0: 'PendingPayment',
  1: 'Paid',
  2: 'Completed',
  3: 'Cancelled',
  4: 'Shipped',
  5: 'Delivered',
};

export function normalizeProductOrderStatus(
  status: ProductOrderStatus | number | string | null | undefined,
): ProductOrderStatus | null {
  if (status == null || status === '') return null;
  if (typeof status === 'number') return STATUS_BY_CODE[status] ?? null;
  if (typeof status === 'string' && /^\d+$/.test(status)) {
    return STATUS_BY_CODE[Number(status)] ?? null;
  }
  const asString = String(status);
  if (
    asString === 'PendingPayment' ||
    asString === 'Paid' ||
    asString === 'Completed' ||
    asString === 'Cancelled' ||
    asString === 'Shipped' ||
    asString === 'Delivered'
  ) {
    return asString;
  }
  return null;
}
