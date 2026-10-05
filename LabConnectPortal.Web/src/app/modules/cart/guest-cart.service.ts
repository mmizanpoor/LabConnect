import { Injectable } from '@angular/core';
import { PublicProductListingDetailDto } from '../products/products.types';
import { CartDto, CartItemDto } from './cart.types';
import { CartService } from './cart.service';

const STORAGE_KEY = 'labconnect_guest_cart';

export interface GuestCartItem {
  productId: string;
  productTitle: string;
  centerName: string;
  centerProfileId: string;
  quantity: number;
  unitPrice: number;
  originalUnitPrice: number;
  discountPercent?: number | null;
  lineTotal: number;
  imagePath?: string | null;
  stockQuantity: number;
}

interface GuestCartStorage {
  items: GuestCartItem[];
}

@Injectable({ providedIn: 'root' })
export class GuestCartService {
  addFromProduct(product: PublicProductListingDetailDto, quantity: number): void {
    const qty = Math.max(1, quantity);
    const cart = this.readStorage();
    const existing = cart.items.find((item) => item.productId === product.productId);

    if (cart.items.length > 0 && cart.items[0]!.centerProfileId !== product.centerProfileId) {
      throw new Error('CART_CENTER_MISMATCH');
    }

    const nextQuantity = (existing?.quantity ?? 0) + qty;
    if (nextQuantity > product.stockQuantity) {
      throw new Error('STOCK_EXCEEDED');
    }

    if (existing) {
      existing.quantity = nextQuantity;
      existing.stockQuantity = product.stockQuantity;
      existing.lineTotal = existing.unitPrice * existing.quantity;
    } else {
      cart.items.push({
        productId: product.productId,
        productTitle: product.title,
        centerName: product.centerName,
        centerProfileId: product.centerProfileId,
        quantity: qty,
        unitPrice: product.finalPrice,
        originalUnitPrice: product.price,
        discountPercent: product.discountPercent,
        lineTotal: product.finalPrice * qty,
        imagePath: product.images[0]?.imagePath ?? null,
        stockQuantity: product.stockQuantity,
      });
    }

    this.writeStorage(cart);
  }

  getItemQuantity(productId: string): number {
    return this.readStorage().items.find((item) => item.productId === productId)?.quantity ?? 0;
  }

  getCartDto(): CartDto {
    const items = this.readStorage().items;
    const mappedItems: CartItemDto[] = items.map((item) => ({
      id: `guest-${item.productId}`,
      itemType: 'Product',
      productId: item.productId,
      productTitle: item.productTitle,
      centerName: item.centerName,
      centerProfileId: item.centerProfileId,
      quantity: item.quantity,
      unitPrice: item.originalUnitPrice ?? item.unitPrice,
      discountPercent: item.discountPercent,
      lineTotal: item.lineTotal,
      imagePath: item.imagePath,
      stockQuantity: item.stockQuantity ?? item.quantity,
    }));

    return {
      centerProfileId: items[0]?.centerProfileId ?? null,
      centerName: items[0]?.centerName ?? null,
      items: mappedItems,
      totalAmount: items.reduce((sum, item) => sum + item.lineTotal, 0),
    };
  }

  itemCount(): number {
    return this.readStorage().items.reduce((sum, item) => sum + item.quantity, 0);
  }

  hasItems(): boolean {
    return this.readStorage().items.length > 0;
  }

  updateQuantity(cartItemId: string, quantity: number): boolean {
    if (quantity < 1) return false;
    const productId = this.extractProductId(cartItemId);
    const cart = this.readStorage();
    const item = cart.items.find((entry) => entry.productId === productId);
    if (!item) return false;

    if (quantity > item.stockQuantity) {
      return false;
    }

    item.quantity = quantity;
    item.lineTotal = item.unitPrice * quantity;
    this.writeStorage(cart);
    return true;
  }

  removeItem(cartItemId: string): void {
    const productId = this.extractProductId(cartItemId);
    const cart = this.readStorage();
    cart.items = cart.items.filter((entry) => entry.productId !== productId);
    this.writeStorage(cart);
  }

  clear(): void {
    localStorage.removeItem(STORAGE_KEY);
  }

  async syncToServer(cartService: CartService): Promise<void> {
    const guestItems = this.readStorage().items;
    if (!guestItems.length) return;

    const cartResult = await cartService.getMyCart();
    if (!cartResult.success) {
      throw new Error(cartResult.message ?? 'SYNC_FAILED');
    }

    let serverItems = cartResult.data?.items ?? [];

    for (const guestItem of guestItems) {
      const existing = serverItems.find((item) => item.productId === guestItem.productId);

      if (existing) {
        const targetQty = Math.max(existing.quantity, guestItem.quantity);
        if (targetQty > existing.quantity) {
          const updateResult = await cartService.updateItem({
            cartItemId: existing.id,
            quantity: targetQty,
          });
          if (!updateResult.success) {
            throw new Error(updateResult.message ?? 'SYNC_FAILED');
          }
          if (updateResult.data) {
            serverItems = updateResult.data.items;
          }
        }
        continue;
      }

      const addResult = await cartService.addItem({
        productId: guestItem.productId,
        quantity: guestItem.quantity,
      });
      if (!addResult.success) {
        throw new Error(addResult.message ?? 'SYNC_FAILED');
      }
      if (addResult.data) {
        serverItems = addResult.data.items;
      }
    }

    this.clear();
  }

  isGuestItemId(id: string): boolean {
    return id.startsWith('guest-');
  }

  private extractProductId(cartItemId: string): string {
    return cartItemId.startsWith('guest-') ? cartItemId.slice(6) : cartItemId;
  }

  private readStorage(): GuestCartStorage {
    try {
      const raw = localStorage.getItem(STORAGE_KEY);
      if (!raw) return { items: [] };
      const parsed = JSON.parse(raw) as { items?: unknown };
      if (!Array.isArray(parsed.items)) return { items: [] };

      // Clear carts keyed by legacy listingId commerce identity.
      const hasLegacyListingId = parsed.items.some(
        (item) =>
          !!item &&
          typeof item === 'object' &&
          'listingId' in item,
      );
      if (hasLegacyListingId) {
        localStorage.removeItem(STORAGE_KEY);
        return { items: [] };
      }

      return { items: parsed.items as GuestCartItem[] };
    } catch {
      return { items: [] };
    }
  }

  private writeStorage(cart: GuestCartStorage): void {
    localStorage.setItem(STORAGE_KEY, JSON.stringify(cart));
  }
}
