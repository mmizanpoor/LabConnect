import { Component, OnInit } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { MatDialog, MatDialogModule } from '@angular/material/dialog';
import { TranslocoPipe } from '@jsverse/transloco';
import { LocalizationService } from '@core/services/localization/localization.service';
import { BaseBackButtonComponent } from '@modules/base/components/base-back-button/base-back-button.component';
import { BaseGridComponent } from '@modules/base/components/base-grid/base-grid.component';
import { BasePagingComponent } from '@modules/base/components/base-paging/base-paging.component';
import { BehaviorSubject } from 'rxjs';
import { ColDef } from 'ag-grid-community';
import { SystemEntity } from '@core/system-entity/system-entity';
import { bindSystemEntity } from '@core/system-entity/bind-system-entity';
import {
  UserResumeDialogComponent,
  UserResumeDialogData,
} from '../../admin/users/user-resume-dialog/user-resume-dialog.component';
import { ProductResumeApplicationForOwnerDto } from '../../products/products.types';
import { ProductResumeApplicationsService } from './product-resume-applications.service';
import {
  ProductResumeApplicationAction,
  ProductResumeApplicationsColDef,
} from './product-resume-applications.coldef';

@Component({
  selector: 'app-product-resume-applications-review',
  standalone: true,
  imports: [
    MatDialogModule,
    TranslocoPipe,
    BaseBackButtonComponent,
    BasePagingComponent,
    BaseGridComponent,
  ],
  templateUrl: './product-resume-applications-review.component.html',
  styleUrl: './product-resume-applications-review.component.scss',
})
export class ProductResumeApplicationsReviewComponent implements OnInit {
  readonly entity = SystemEntity.Product;
  private readonly _systemEntityBinding = bindSystemEntity(SystemEntity.Product);

  loading = false;
  error = '';
  rows: ProductResumeApplicationForOwnerDto[] = [];
  colDef: ColDef[] = [];
  readonly list$ = new BehaviorSubject<ProductResumeApplicationForOwnerDto[]>([]);
  totalCount = 0;
  page = 1;
  pageSize = 20;
  productId = '';
  productTitle = '';

  constructor(
    private _route: ActivatedRoute,
    private _applicationsService: ProductResumeApplicationsService,
    private _localization: LocalizationService,
    private _dialog: MatDialog,
    private _colDefService: ProductResumeApplicationsColDef,
  ) {}

  ngOnInit(): void {
    this.productId = this._route.snapshot.paramMap.get('id') ?? '';
    this.colDef = this._colDefService.get();
    this._colDefService.actionClicked.subscribe((evt: ProductResumeApplicationAction) => {
      if (evt?.type === 'view' && evt.row) this.openResume(evt.row);
    });
    void this.load();
  }

  async load(): Promise<void> {
    if (!this.productId) return;
    this.loading = true;
    this.error = '';
    try {
      const result = await this._applicationsService.getForProduct({
        productId: this.productId,
        page: this.page,
        pageSize: this.pageSize,
      });
      if (!result.success || !result.data) {
        throw new Error(
          result.message ??
            this._localization.translate('modules.profile.productResumeApplications.errors.loadFailed'),
        );
      }
      this.rows = result.data.items;
      this.totalCount = result.data.totalCount;
      this.productTitle = result.data.productTitle ?? '';
      this.list$.next(this.rows);
    } catch (e: unknown) {
      this.error =
        e instanceof Error
          ? e.message
          : this._localization.translate('modules.profile.productResumeApplications.errors.loadFailed');
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

  openResume(row: ProductResumeApplicationForOwnerDto): void {
    const fullName = `${row.applicantFirstName ?? ''} ${row.applicantLastName ?? ''}`.trim()
      || row.applicantMobileNumber;
    const dialogRef = this._dialog.open(UserResumeDialogComponent, {
      width: '720px',
      maxWidth: '94vw',
      data: {
        userId: row.applicantUserId,
        fullName,
        productResumeApplicationId: row.productResumeApplicationId,
        status: row.status,
        reviewNotes: row.reviewNotes,
      } satisfies UserResumeDialogData,
    });
    dialogRef.afterClosed().subscribe((result) => {
      if (result?.reviewed) void this.load();
    });
  }
}
