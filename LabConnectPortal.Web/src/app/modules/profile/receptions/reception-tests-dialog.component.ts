import { Component, Inject } from '@angular/core';
import { MAT_DIALOG_DATA } from '@angular/material/dialog';
import { TranslocoPipe } from '@jsverse/transloco';
import { BehaviorSubject } from 'rxjs';
import { ColDef } from 'ag-grid-community';
import { BaseDialogComponent } from '@modules/base/components/base-dialog/base-dialog.component';
import { BaseGridComponent } from '@modules/base/components/base-grid/base-grid.component';
import { ReceptTestDto } from './receptions.types';
import { ReceptionTestsColDef } from './reception-tests.coldef';

export interface ReceptionTestsDialogData {
  patientName: string;
  sourceReceptId: string;
  receptionDate: string;
  tests: ReceptTestDto[];
}

@Component({
  selector: 'app-reception-tests-dialog',
  standalone: true,
  imports: [TranslocoPipe, BaseDialogComponent, BaseGridComponent],
  templateUrl: './reception-tests-dialog.component.html',
})
export class ReceptionTestsDialogComponent {
  readonly list$ = new BehaviorSubject<ReceptTestDto[]>([]);
  readonly colDef: ColDef[];

  constructor(
    private _colDef: ReceptionTestsColDef,
    @Inject(MAT_DIALOG_DATA) readonly data: ReceptionTestsDialogData,
  ) {
    this.list$.next(data.tests ?? []);
    this.colDef = this._colDef.get();
  }
}
