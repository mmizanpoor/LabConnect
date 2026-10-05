import { Injectable } from '@angular/core';
import { ApiHttpService } from '@core/services/http/api-http.service';
import {
  ProductCategoryPriceItemDto,
  SaveProductCategoryPricesCommand,
} from './category-prices.types';

@Injectable({ providedIn: 'root' })
export class CategoryPricesService {
  constructor(private _http: ApiHttpService) {}

  getAll() {
    return this._http.get<ProductCategoryPriceItemDto[]>('ProductCategoryPrice', 'GetAll');
  }

  saveAll(command: SaveProductCategoryPricesCommand) {
    return this._http.post<ProductCategoryPriceItemDto[]>('ProductCategoryPrice', 'SaveAll', command);
  }
}
