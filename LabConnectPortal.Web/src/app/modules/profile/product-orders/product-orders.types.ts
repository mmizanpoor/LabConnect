export type ProductOrderStatus =
  | 'PendingPayment'
  | 'Paid'
  | 'Completed'
  | 'Cancelled'
  | 'Shipped'
  | 'Delivered';

export type ProductShipmentFilter = 'pending' | 'shipped' | 'all';

export interface ProductOrderListItemDto {
  id: string;
  centerName: string;
  buyerAddress?: string;
  buyerPhone?: string;
  /** API may send the enum name or numeric value. */
  status: ProductOrderStatus | number;
  totalAmount: number;
  shippingCost?: number;
  shippingMethod?: 'Post' | 'Courier' | 'Pickup' | number;
  createdAt: string;
  itemCount: number;
  canShip?: boolean;
  canDeliver?: boolean;
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
  status: ProductOrderStatus | number;
  totalAmount: number;
  shippingCost?: number;
  shippingMethod?: 'Post' | 'Courier' | 'Pickup' | number;
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
  buyerName?: string;
  buyerPhone?: string;
  canShip?: boolean;
  canDeliver?: boolean;
  canConfirmDelivery?: boolean;
  items: ProductOrderItemDto[];
}

export interface GetCenterOrdersQuery {
  status?: ProductOrderStatus;
  page?: number;
  pageSize?: number;
}

export interface PagedCenterOrders {
  items: ProductOrderListItemDto[];
  totalCount: number;
  page: number;
  pageSize: number;
}

export interface GetShipmentItemsQuery {
  /** pending | shipped | all */
  shipmentStatus?: ProductShipmentFilter;
  search?: string;
  page?: number;
  pageSize?: number;
}

export interface ProductOrderShipmentItemDto {
  id: string;
  productTitle: string;
  buyerCenterName: string;
  buyerCenterAddress: string;
  buyerPhone: string;
  quantity: number;
  paidAt?: string | null;
  shippedAt?: string | null;
  isShipped: boolean;
}

export interface PagedShipmentItems {
  items: ProductOrderShipmentItemDto[];
  totalCount: number;
  page: number;
  pageSize: number;
}

export interface ShipOrderCommand {
  orderId: string;
  trackingCode?: string | null;
  shippingCompany?: string | null;
}

export interface DeliverOrderCommand {
  orderId: string;
  notes?: string | null;
}

export const PRODUCT_ORDER_STATUSES: ProductOrderStatus[] = [
  'PendingPayment',
  'Paid',
  'Completed',
  'Shipped',
  'Delivered',
  'Cancelled',
];

export const PRODUCT_SHIPMENT_FILTERS: ProductShipmentFilter[] = ['pending', 'shipped', 'all'];
