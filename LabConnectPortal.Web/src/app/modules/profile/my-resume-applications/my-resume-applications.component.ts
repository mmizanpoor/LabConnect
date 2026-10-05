import { Component, OnInit } from '@angular/core';
import { MatDialog, MatDialogModule } from '@angular/material/dialog';
import { TranslocoPipe } from '@jsverse/transloco';
import { LocalizationService } from '@core/services/localization/localization.service';
import { BaseGridComponent } from '@modules/base/components/base-grid/base-grid.component';
import { BehaviorSubject } from 'rxjs';
import { ColDef } from 'ag-grid-community';
import { BASE_DIALOG_PANEL_CLASS } from '@modules/base/components/base-dialog/base-dialog.component';
import { ProductResumeApplicationDto } from '../../products/products.types';
import { ProductResumeApplicationsService } from '../product-resume-applications/product-resume-applications.service';
import {
  MyResumeApplicationAction,
  MyResumeApplicationsColDef,
} from './my-resume-applications.coldef';
import { MyResumeApplicationStatusDialogComponent } from './my-resume-application-status-dialog.component';

@Component({
  selector: 'app-my-resume-applications',
  standalone: true,
  imports: [MatDialogModule, TranslocoPipe, BaseGridComponent],
  templateUrl: './my-resume-applications.component.html',
  styleUrl: './my-resume-applications.component.scss',
})
export class MyResumeApplicationsComponent implements OnInit {
  loading = false;
  error = '';
  rows: ProductResumeApplicationDto[] = [];
  colDef: ColDef[] = [];
  readonly list$ = new BehaviorSubject<ProductResumeApplicationDto[]>([]);

  constructor(
    private _applicationsService: ProductResumeApplicationsService,
    private _localization: LocalizationService,
    private _dialog: MatDialog,
    private _colDefService: MyResumeApplicationsColDef,
  ) {}

  ngOnInit(): void {
    this.colDef = this._colDefService.get();
    this._colDefService.actionClicked.subscribe((evt: MyResumeApplicationAction) => {
      if (evt?.type === 'status' && evt.row) this.openStatus(evt.row);
    });
    void this.load();
  }

  async load(): Promise<void> {
    this.loading = true;
    this.error = '';
    try {
      const result = await this._applicationsService.getMyApplications();
      if (!result.success || !result.data) {
        throw new Error(
          result.message ??
            this._localization.translate('modules.profile.myResumeApplications.errors.loadFailed'),
        );
      }
      this.rows = result.data;
      this.list$.next(this.rows);
    } catch (e: unknown) {
      this.error =
        e instanceof Error
          ? e.message
          : this._localization.translate('modules.profile.myResumeApplications.errors.loadFailed');
      this.rows = [];
      this.list$.next([]);
    } finally {
      this.loading = false;
    }
  }

  openStatus(row: ProductResumeApplicationDto): void {
    this._dialog.open(MyResumeApplicationStatusDialogComponent, {
      width: '480px',
      maxWidth: '94vw',
      panelClass: BASE_DIALOG_PANEL_CLASS,
      data: { application: row },
    });
  }
}
