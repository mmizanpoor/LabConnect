import { Component, Inject, OnInit } from '@angular/core';
import { MAT_DIALOG_DATA, MatDialog, MatDialogModule, MatDialogRef } from '@angular/material/dialog';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatTooltipModule } from '@angular/material/tooltip';
import { TranslocoPipe } from '@jsverse/transloco';
import jMoment from 'moment-jalaali';
import { LocalizationService } from '@core/services/localization/localization.service';
import { BaseGridComponent } from '@modules/base/components/base-grid/base-grid.component';
import { SpecialOffersService } from '../special-offers.service';
import { SpecialOfferRejectDialogComponent } from '../special-offer-reject-dialog/special-offer-reject-dialog.component';
import { SpecialOfferRequestDto, SpecialOfferRequestStatus } from '../special-offers.types';
import { BehaviorSubject } from 'rxjs';
import { ColDef } from 'ag-grid-community';
import { SpecialOfferRequestAction, SpecialOfferRequestsColDef } from './special-offer-requests.coldef';
import { LabPermissionService } from '@core/services/auth/lab-permission.service';
import { SystemEntity } from '@core/system-entity/system-entity';

export interface SpecialOfferRequestsDialogCloseResult {
  navigateToAgreementForm?: boolean;
  requestId?: number;
}

export interface SpecialOfferRequestsDialogData {
  offerId: number;
  offerTitle: string;
}

@Component({
  selector: 'app-special-offer-requests-dialog',
  standalone: true,
  imports: [MatDialogModule, MatButtonModule, MatIconModule, MatTooltipModule, TranslocoPipe, BaseGridComponent],
  templateUrl: './special-offer-requests-dialog.component.html',
})
export class SpecialOfferRequestsDialogComponent implements OnInit {
  loading = false;
  actionLoadingId: number | null = null;
  error = '';
  rows: SpecialOfferRequestDto[] = [];
  colDef: ColDef[] = [];
  readonly list$ = new BehaviorSubject<SpecialOfferRequestDto[]>([]);

  readonly statusEnum = SpecialOfferRequestStatus;

  constructor(
    @Inject(MAT_DIALOG_DATA) public data: SpecialOfferRequestsDialogData,
    private _dialogRef: MatDialogRef<SpecialOfferRequestsDialogComponent>,
    private _dialog: MatDialog,
    private _service: SpecialOffersService,
    private _localization: LocalizationService,
    private _colDef: SpecialOfferRequestsColDef,
    private _labPermission: LabPermissionService,
  ) {}

  ngOnInit(): void {
    this.colDef = this._colDef.get(
      (v) => this.formatDateOnly(v),
      (v) => this.formatTimeOnly(v),
      (s) => this.statusKey(s),
      (s) => this.statusBadgeClass(s),
      (r) => this.canApprove(r),
      (r) => this.canReject(r),
      (r) => this.canCreateContract(r),
    );

    this._colDef.actionClicked.subscribe((evt: SpecialOfferRequestAction) => {
      if (!evt?.row) return;
      if (evt.type === 'approve') void this.approve(evt.row);
      if (evt.type === 'reject') this.reject(evt.row);
      if (evt.type === 'createContract') this.createContract(evt.row);
    });

    void this.load();
  }

  async load(): Promise<void> {
    this.loading = true;
    this.error = '';
    try {
      const result = await this._service.getRequests(this.data.offerId);
      if (!result.success || !result.data) {
        throw new Error(
          result.message ?? this._localization.translate('modules.profile.specialOffers.errors.loadRequestsFailed'),
        );
      }
      this.rows = result.data;
      this.list$.next(this.rows);
    } catch (e: unknown) {
      this.error =
        e instanceof Error
          ? e.message
          : this._localization.translate('modules.profile.specialOffers.errors.loadRequestsFailed');
      this.rows = [];
      this.list$.next([]);
    } finally {
      this.loading = false;
    }
  }

  formatDateOnly(value: string): string {
    const parsed = jMoment(value).locale('fa');
    return parsed.isValid() ? parsed.format('jYYYY/jMM/jDD') : '—';
  }

  formatTimeOnly(value: string): string {
    const parsed = jMoment(value).locale('fa');
    return parsed.isValid() ? parsed.format('HH:mm') : '';
  }

  statusBadgeClass(status: SpecialOfferRequestStatus): string {
    switch (status) {
      case SpecialOfferRequestStatus.Pending:
        return 'bg-amber-100 text-amber-700';
      case SpecialOfferRequestStatus.Rejected:
        return 'bg-red-100 text-red-700';
      case SpecialOfferRequestStatus.AwaitingContractCreation:
        return 'bg-violet-100 text-violet-700';
      case SpecialOfferRequestStatus.ContractCreated:
        return 'bg-blue-100 text-blue-700';
      default:
        return 'bg-amber-100 text-amber-700';
    }
  }

  statusKey(status: SpecialOfferRequestStatus): string {
    switch (status) {
      case SpecialOfferRequestStatus.Pending:
        return 'modules.profile.specialOffers.requestStatus.pending';
      case SpecialOfferRequestStatus.Rejected:
        return 'modules.profile.specialOffers.requestStatus.rejected';
      case SpecialOfferRequestStatus.AwaitingContractCreation:
        return 'modules.profile.specialOffers.requestStatus.awaitingContract';
      case SpecialOfferRequestStatus.ContractCreated:
        return 'modules.profile.specialOffers.requestStatus.contractCreated';
      default:
        return 'modules.profile.specialOffers.requestStatus.pending';
    }
  }

  truncate(text: string, max = 80): string {
    const value = text?.trim() ?? '';
    if (value.length <= max) return value || '—';
    return `${value.slice(0, max)}…`;
  }

  canApprove(row: SpecialOfferRequestDto): boolean {
    return (
      this._labPermission.can(SystemEntity.SpecialOffer, 'update') &&
      row.status === SpecialOfferRequestStatus.Pending
    );
  }

  canReject(row: SpecialOfferRequestDto): boolean {
    return (
      this._labPermission.can(SystemEntity.SpecialOffer, 'update') &&
      row.status === SpecialOfferRequestStatus.Pending
    );
  }

  canCreateContract(row: SpecialOfferRequestDto): boolean {
    return (
      this._labPermission.can(SystemEntity.LabAgreement, 'create') &&
      row.status === SpecialOfferRequestStatus.AwaitingContractCreation
    );
  }

  async approve(row: SpecialOfferRequestDto): Promise<void> {
    if (!this.canApprove(row)) return;

    this.actionLoadingId = row.id;
    this.error = '';
    try {
      const result = await this._service.approveRequest(row.id);
      if (!result.success) {
        throw new Error(
          result.message ?? this._localization.translate('modules.profile.specialOffers.errors.approveFailed'),
        );
      }
      this._dialogRef.close({ navigateToAgreementForm: true, requestId: row.id } satisfies SpecialOfferRequestsDialogCloseResult);
    } catch (e: unknown) {
      this.error =
        e instanceof Error
          ? e.message
          : this._localization.translate('modules.profile.specialOffers.errors.approveFailed');
    } finally {
      this.actionLoadingId = null;
    }
  }

  reject(row: SpecialOfferRequestDto): void {
    if (!this.canReject(row)) return;

    const ref = this._dialog.open(SpecialOfferRejectDialogComponent, {
      width: '480px',
      maxWidth: '95vw',
      autoFocus: false,
      data: { requestId: row.id, userDisplayName: row.userDisplayName },
    });

    ref.afterClosed().subscribe((result: { requestId: number; reason: string } | null) => {
      if (!result) return;
      void this.submitReject(result.requestId, result.reason);
    });
  }

  private async submitReject(requestId: number, reason: string): Promise<void> {
    this.actionLoadingId = requestId;
    this.error = '';
    try {
      const result = await this._service.rejectRequest({ requestId, reason });
      if (!result.success) {
        throw new Error(
          result.message ?? this._localization.translate('modules.profile.specialOffers.errors.rejectFailed'),
        );
      }
      await this.load();
    } catch (e: unknown) {
      this.error =
        e instanceof Error
          ? e.message
          : this._localization.translate('modules.profile.specialOffers.errors.rejectFailed');
    } finally {
      this.actionLoadingId = null;
    }
  }

  createContract(row: SpecialOfferRequestDto): void {
    if (!this.canCreateContract(row)) return;
    this._dialogRef.close({ navigateToAgreementForm: true, requestId: row.id } satisfies SpecialOfferRequestsDialogCloseResult);
  }

  close(): void {
    this._dialogRef.close();
  }
}
