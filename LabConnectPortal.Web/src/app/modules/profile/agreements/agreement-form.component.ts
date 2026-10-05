import { Component, OnInit, ViewChild } from '@angular/core';
import { FormControl, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { MatButtonModule } from '@angular/material/button';
import { BaseCheckboxComponent } from '@modules/base/components/base-checkbox/base-checkbox.component';
import { MatIconModule } from '@angular/material/icon';
import { MatTableModule } from '@angular/material/table';
import { TranslocoPipe } from '@jsverse/transloco';
import jMoment, { Moment } from 'moment-jalaali';
import { LocalizationService } from '@core/services/localization/localization.service';
import { BaseBackButtonComponent } from '@modules/base/components/base-back-button/base-back-button.component';
import { BaseButtonComponent } from '@modules/base/components/base-button/base-button.component';
import { BaseFormFieldComponent } from '@modules/base/components/base-form-field/base-form-field.component';
import { DocumentEditorComponent } from '@modules/base/components/document-editor/document-editor.component';
import { AgreementsService } from './agreements.service';
import { SpecialOffersService } from '../special-offers/special-offers.service';
import {
  CreateLabAgreementFromPortalCommand,
  PortalLabAgreementAttachmentCommand,
  SpecialOfferRequestForAgreementDto,
  SpecialOfferTestItemDto,
} from '../special-offers/special-offers.types';
import { SystemEntity } from '@core/system-entity/system-entity';
import { bindSystemEntity } from '@core/system-entity/bind-system-entity';
import { LabPermissionService } from '@core/services/auth/lab-permission.service';

interface AgreementTestRow extends SpecialOfferTestItemDto {
  selected: boolean;
}

interface PendingAttachment {
  fileName: string;
  contentType: string;
  fileBase64: string;
  remark: string;
}

@Component({
  selector: 'app-agreement-form',
  standalone: true,
  imports: [
    ReactiveFormsModule,
    MatButtonModule,
    BaseCheckboxComponent,
    MatIconModule,
    MatTableModule,
    TranslocoPipe,
    BaseBackButtonComponent,
    BaseButtonComponent,
    BaseFormFieldComponent,
    DocumentEditorComponent,
  ],
  templateUrl: './agreement-form.component.html',
})
export class AgreementFormComponent implements OnInit {
  readonly entity = SystemEntity.LabAgreement;
  private readonly _systemEntityBinding = bindSystemEntity(SystemEntity.LabAgreement);

  @ViewChild(DocumentEditorComponent) documentEditor?: DocumentEditorComponent;

  loading = false;
  saving = false;
  error = '';
  requestId: number | null = null;
  prefetch: SpecialOfferRequestForAgreementDto | null = null;
  isAddendum = false;
  parentId: number | null = null;
  testRows: AgreementTestRow[] = [];
  attachments: PendingAttachment[] = [];
  attachmentError = '';

  displayedColumns = ['select', 'cpnCode', 'fullName', 'discount', 'approvePrice', 'approvedPrice'];

  form = new FormGroup({
    title: new FormControl('', [Validators.required, Validators.maxLength(300)]),
    contractNumber: new FormControl('', [Validators.required, Validators.maxLength(100)]),
    startDate: new FormControl<Moment | null>(null, Validators.required),
    expDate: new FormControl<Moment | null>(null, Validators.required),
    primaryLab: new FormControl('—'),
    receiverLab: new FormControl('—'),
  });

  constructor(
    private _route: ActivatedRoute,
    private _router: Router,
    private _agreementsService: AgreementsService,
    private _specialOffersService: SpecialOffersService,
    private _localization: LocalizationService,
    private _labPermission: LabPermissionService,
  ) {}

  get canCreate(): boolean {
    return this._labPermission.can(this.entity, 'create');
  }

  get allSelected(): boolean {
    return this.testRows.length > 0 && this.testRows.every((row) => row.selected);
  }

  get someSelected(): boolean {
    return this.testRows.some((row) => row.selected) && !this.allSelected;
  }

  ngOnInit(): void {
    const requestIdParam = this._route.snapshot.queryParamMap.get('requestId');
    this.requestId = requestIdParam ? Number(requestIdParam) : null;
    void this.load();
  }

  async load(): Promise<void> {
    this.loading = true;
    this.error = '';
    try {
      if (this.requestId) {
        const result = await this._specialOffersService.getRequestForAgreement(this.requestId);
        if (!result.success || !result.data) {
          throw new Error(
            result.message ?? this._localization.translate('modules.profile.agreements.form.errors.loadRequestFailed'),
          );
        }
        this.prefetch = result.data;
        this.isAddendum = this.prefetch.isAddendum && !!this.prefetch.parentId;
        this.parentId = this.prefetch.parentId ?? null;
        this.form.controls.primaryLab.setValue(this.prefetch.primaryLabName?.trim() || '—');
        this.form.controls.receiverLab.setValue(this.prefetch.receiverLabName?.trim() || '—');

        if (this.isAddendum) {
          this.form.patchValue({
            title: this.prefetch.offerTitle,
            contractNumber: this.prefetch.activeAgreementContractNumber ?? '',
            startDate: jMoment(),
            expDate: this.parseDate(
              this.prefetch.activeAgreementExpDate ?? this.prefetch.offerEndDate,
            ),
          });
          this.form.controls.contractNumber.disable();
          this.form.controls.expDate.disable();
        } else {
          this.form.patchValue({
            title: this.prefetch.offerTitle,
            startDate: this.parseDate(this.prefetch.offerStartDate),
            expDate: this.parseDate(this.prefetch.offerEndDate),
          });
        }
        this.testRows = this.prefetch.tests.map((test) => ({ ...test, selected: true }));
      }
    } catch (e: unknown) {
      this.error =
        e instanceof Error
          ? e.message
          : this._localization.translate('modules.profile.agreements.form.errors.loadRequestFailed');
    } finally {
      this.loading = false;
    }
  }

  toggleSelectAll(checked: boolean): void {
    this.testRows = this.testRows.map((row) => ({ ...row, selected: checked }));
  }

  toggleRow(row: AgreementTestRow, checked: boolean): void {
    row.selected = checked;
  }

  calcApprovedPrice(row: AgreementTestRow): number | null {
    if (row.approvePrice == null) return null;
    const discount = row.discount ?? 0;
    return Math.round(row.approvePrice * (1 - discount / 100));
  }

  formatPrice(value: number | null): string {
    if (value == null) return '—';
    return value.toLocaleString('fa-IR');
  }

  async onAttachmentSelected(event: Event): Promise<void> {
    const input = event.target as HTMLInputElement;
    const files = input.files;
    if (!files?.length) return;

    this.attachmentError = '';
    for (const file of Array.from(files)) {
      if (!this.isAllowedAttachment(file)) {
        this.attachmentError = this._localization.translate('modules.profile.agreements.form.errors.invalidAttachment');
        continue;
      }
      if (file.size > 5 * 1024 * 1024) {
        this.attachmentError = this._localization.translate('modules.profile.agreements.form.errors.attachmentTooLarge');
        continue;
      }

      const base64 = await this.readFileAsBase64(file);
      this.attachments.push({
        fileName: file.name,
        contentType: file.type || 'application/octet-stream',
        fileBase64: base64,
        remark: file.name,
      });
    }

    input.value = '';
  }

  removeAttachment(index: number): void {
    this.attachments.splice(index, 1);
  }

  async save(): Promise<void> {
    if (!this.canCreate) return;
    this.error = '';
    this.attachmentError = '';

    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    const contractText = this.documentEditor?.getContent() ?? '';
    if (this.documentEditor?.isEmptyContent()) {
      this.error = this._localization.translate('modules.profile.agreements.form.errors.textRequired');
      return;
    }

    const selectedTests = this.testRows.filter((row) => row.selected);
    if (this.testRows.length && !selectedTests.length) {
      this.error = this._localization.translate('modules.profile.agreements.form.errors.noTestsSelected');
      return;
    }

    const raw = this.form.getRawValue();
    const startDate = this.toIsoDate(raw.startDate ?? null, false);
    const expDate = this.toIsoDate(raw.expDate ?? null, true);
    if (!startDate || !expDate) {
      this.error = this._localization.translate('modules.profile.agreements.form.errors.invalidDates');
      return;
    }

    if (!this.requestId || !this.prefetch) {
      this.error = this._localization.translate('modules.profile.agreements.form.errors.loadRequestFailed');
      return;
    }

    if (!this.prefetch.primaryLabCodeNew || !this.prefetch.receiverLabCodeNew) {
      this.error = this._localization.translate('modules.profile.agreements.form.errors.invalidLabCodes');
      return;
    }

    const command: CreateLabAgreementFromPortalCommand = {
      specialOfferRequestId: this.requestId,
      contractNumber: raw.contractNumber?.trim() ?? '',
      title: raw.title?.trim() ?? '',
      text: contractText,
      startDate,
      expDate,
      primaryAgreementLabCodeNew: this.prefetch.primaryLabCodeNew,
      receiverAgreementLabCodeNew: this.prefetch.receiverLabCodeNew,
      isAddendum: this.isAddendum,
      parentId: this.parentId,
      attachments: this.attachments.map(
        (a): PortalLabAgreementAttachmentCommand => ({
          fileName: a.fileBase64,
          contentType: a.contentType,
          remark: a.fileName,
        }),
      ),
      testPrices: selectedTests.map((row) => ({ testInfoId: row.testInfoId })),
    };

    this.saving = true;
    try {
      await this._agreementsService.createFromPortal(command);
      void this._router.navigate(['/profile/agreements']);
    } catch (e: unknown) {
      this.error =
        e instanceof Error
          ? e.message
          : this._localization.translate('modules.profile.agreements.form.errors.saveFailed');
    } finally {
      this.saving = false;
    }
  }

  private parseDate(value: string): Moment | null {
    const parsed = jMoment(value);
    return parsed.isValid() ? parsed : null;
  }

  private toIsoDate(value: Moment | null, endOfDay: boolean): string | null {
    if (!value) return null;
    const date = value.clone().locale('fa');
    if (endOfDay) {
      date.hours(23).minutes(59).seconds(59).milliseconds(999);
    } else {
      date.hours(0).minutes(0).seconds(0).milliseconds(0);
    }
    return date.toISOString();
  }

  private isAllowedAttachment(file: File): boolean {
    if (file.type.startsWith('image/')) return true;
    return file.type === 'application/pdf';
  }

  private readFileAsBase64(file: File): Promise<string> {
    return new Promise((resolve, reject) => {
      const reader = new FileReader();
      reader.onload = () => {
        const result = reader.result as string;
        const commaIndex = result.indexOf(',');
        resolve(commaIndex >= 0 ? result.slice(commaIndex + 1) : result);
      };
      reader.onerror = () => reject(new Error('read failed'));
      reader.readAsDataURL(file);
    });
  }
}
