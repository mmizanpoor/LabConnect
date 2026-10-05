export type CartItemType = 'Product';

export type ShippingMethod = 'Post' | 'Courier' | 'Pickup';

export const SHIPPING_METHODS: ShippingMethod[] = ['Post', 'Courier', 'Pickup'];

export interface CartItemDto {
  id: string;
  itemType: CartItemType;
  productId?: string | null;
  productTitle: string;
  centerName: string;
  centerProfileId: string;
  quantity: number;
  unitPrice: number;
  discountPercent?: number | null;
  lineTotal: number;
  imagePath?: string | null;
  stockQuantity: number;
}

export interface CartDto {
  centerProfileId?: string | null;
  centerName?: string | null;
  defaultShippingCost?: number;
  items: CartItemDto[];
  totalAmount: number;
}

export interface AddToCartCommand {
  productId?: string;
  quantity?: number;
}

export interface UpdateCartItemCommand {
  cartItemId: string;
  quantity: number;
}

export interface CheckoutCommand {
  replaceExistingCenterItems?: boolean;
  shippingRecipientName?: string;
  shippingAddress?: string;
  shippingPhone?: string;
  shippingMethod?: ShippingMethod;
}

export interface ProductOrderDto {
  id: string;
  centerProfileId: string;
  centerName: string;
  status: string;
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
  canConfirmDelivery?: boolean;
  items: unknown[];
}

export interface CheckoutResultDto {
  order?: ProductOrderDto | null;
}
