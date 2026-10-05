export type ProductOrderStatus =
  | 'PendingPayment'
  | 'Paid'
  | 'Completed'
  | 'Cancelled'
  | 'Shipped'
  | 'Delivered';

export type ShippingMethod = 'Post' | 'Courier' | 'Pickup';

export interface ProductOrderListItemDto {
  id: string;
  centerName: string;
  status: ProductOrderStatus;
  totalAmount: number;
  shippingCost?: number;
  shippingMethod?: ShippingMethod | number;
  createdAt: string;
  itemCount: number;
  canConfirmDelivery?: boolean;
}

export type CartItemType = 'Product';

export interface ProductOrderItemDto {
  id: string;
  itemType: CartItemType;
  productId?: string | null;
  productTitle: string;
  quantity: number;
  unitPrice: number;
  discountPercent?: number | null;
  lineTotal: number;
  hasReview: boolean;
  canReview: boolean;
  shippedAt?: string | null;
  isShipped?: boolean;
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
  deliveryNotes?: string | null;
  canConfirmDelivery?: boolean;
  items: ProductOrderItemDto[];
}

export interface GetMyOrdersQuery {
  status?: ProductOrderStatus;
  page?: number;
  pageSize?: number;
}

export interface PagedMyOrders {
  items: ProductOrderListItemDto[];
  totalCount: number;
  page: number;
  pageSize: number;
}

export const MY_ORDER_STATUSES: ProductOrderStatus[] = [
  'PendingPayment',
  'Paid',
  'Completed',
  'Shipped',
  'Delivered',
  'Cancelled',
];
