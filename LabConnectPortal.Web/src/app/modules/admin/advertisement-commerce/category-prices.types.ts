export interface ProductCategoryPriceItemDto {
  productCategoryId: number;
  categoryTitle: string;
  productCategoryGroupId: number | null;
  groupTitle: string;
  price: number | null;
  updatedAt: string | null;
}

export interface SaveProductCategoryPriceItemCommand {
  productCategoryId: number;
  price: number;
}

export interface SaveProductCategoryPricesCommand {
  items: SaveProductCategoryPriceItemCommand[];
}
