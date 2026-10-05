import { Component, DestroyRef, OnInit, inject } from '@angular/core';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { TranslocoPipe } from '@jsverse/transloco';
import { LocalizationService } from '@core/services/localization/localization.service';
import { BaseGridComponent } from '@modules/base/components/base-grid/base-grid.component';
import { BasePagingComponent } from '@modules/base/components/base-paging/base-paging.component';
import { ProductReviewService } from '@modules/products/products.service';
import { ProductReviewListItemDto } from '@modules/products/products.types';
import { BehaviorSubject } from 'rxjs';
import { ColDef } from 'ag-grid-community';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { ProductReviewsAdminColDef } from './product-reviews-admin.coldef';
import { SystemEntity } from '@core/system-entity/system-entity';
import { bindSystemEntity } from '@core/system-entity/bind-system-entity';

@Component({
  selector: 'app-product-reviews-admin',
  standalone: true,
  imports: [
    MatButtonModule,
    MatIconModule,
    TranslocoPipe,
    BasePagingComponent,
    BaseGridComponent,
  ],
  templateUrl: './product-reviews-admin.component.html',
  styles: `
    :host {
      display: block;
      height: 100%;
      min-height: 0;
    }
  `,
})
export class ProductReviewsAdminComponent implements OnInit {
  readonly entity = SystemEntity.ProductReview;
  private readonly _systemEntityBinding = bindSystemEntity(SystemEntity.ProductReview);

  private readonly _destroyRef = inject(DestroyRef);
  loading = false;
  error = '';
  rows: ProductReviewListItemDto[] = [];
  colDef: ColDef[] = [];
  readonly list$ = new BehaviorSubject<ProductReviewListItemDto[]>([]);
  totalCount = 0;
  page = 1;
  pageSize = 20;

  constructor(
    private _productReviewService: ProductReviewService,
    private _localization: LocalizationService,
    private _colDef: ProductReviewsAdminColDef,
  ) {}

  ngOnInit(): void {
    this.colDef = this._colDef.get();
    this._colDef.actionClicked
      .pipe(takeUntilDestroyed(this._destroyRef))
      .subscribe((evt) => {
        if (!evt?.row) return;
        if (evt.type === 'approve') void this.approve(evt.row);
        if (evt.type === 'reject') void this.reject(evt.row);
      });
    void this.load();
  }

  async load(): Promise<void> {
    this.loading = true;
    this.error = '';
    try {
      const result = await this._productReviewService.getPendingReviews({
        page: this.page,
        pageSize: this.pageSize,
      });
      if (!result.success || !result.data) {
        throw new Error(result.message ?? this._localization.translate('modules.admin.productReviews.errors.loadFailed'));
      }
      this.rows = result.data.items;
      this.totalCount = result.data.totalCount;
      this.list$.next(this.rows);
    } catch (e: unknown) {
      this.error =
        e instanceof Error ? e.message : this._localization.translate('modules.admin.productReviews.errors.loadFailed');
      this.rows = [];
      this.list$.next([]);
    } finally {
      this.loading = false;
    }
  }

  onPageChange(page: number): void {
    this.page = page;
    void this.load();
  }

  async approve(row: ProductReviewListItemDto): Promise<void> {
    const result = await this._productReviewService.approve(row.id);
    if (!result.success) {
      this.error = result.message ?? this._localization.translate('modules.admin.productReviews.errors.approveFailed');
      return;
    }
    await this.load();
  }

  async reject(row: ProductReviewListItemDto): Promise<void> {
    if (!confirm(this._localization.translate('modules.admin.productReviews.confirmReject'))) return;
    const result = await this._productReviewService.reject(row.id);
    if (!result.success) {
      this.error = result.message ?? this._localization.translate('modules.admin.productReviews.errors.rejectFailed');
      return;
    }
    await this.load();
  }
}
