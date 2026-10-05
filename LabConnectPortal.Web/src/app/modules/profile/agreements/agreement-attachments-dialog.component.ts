import { Component, Inject, OnInit, inject } from '@angular/core';
import { DomSanitizer, SafeResourceUrl } from '@angular/platform-browser';
import { MAT_DIALOG_DATA, MatDialogModule, MatDialogRef } from '@angular/material/dialog';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { TranslocoPipe } from '@jsverse/transloco';
import { LocalizationService } from '@core/services/localization/localization.service';
import { AgreementsService } from './agreements.service';
import { LabAgreementAttachmentDto } from './agreements.types';

export interface AgreementAttachmentsDialogData {
  agreementId: number;
  agreementTitle?: string | null;
  attachments?: LabAgreementAttachmentDto[] | null;
}

@Component({
  selector: 'app-agreement-attachments-dialog',
  standalone: true,
  imports: [MatDialogModule, MatButtonModule, MatIconModule, TranslocoPipe],
  templateUrl: './agreement-attachments-dialog.component.html',
})
export class AgreementAttachmentsDialogComponent implements OnInit {
  private _agreementsService = inject(AgreementsService);
  private _localization = inject(LocalizationService);
  private _sanitizer = inject(DomSanitizer);

  loading = false;
  error = '';
  attachments: LabAgreementAttachmentDto[] = [];
  selectedAttachment: LabAgreementAttachmentDto | null = null;

  constructor(
    private _dialogRef: MatDialogRef<AgreementAttachmentsDialogComponent>,
    @Inject(MAT_DIALOG_DATA) readonly data: AgreementAttachmentsDialogData,
  ) {
    if (data.attachments) {
      this.attachments = [...data.attachments];
      this.selectedAttachment = this.attachments[0] ?? null;
    }
  }

  ngOnInit(): void {
    if (!this.attachments.length) {
      void this.loadAttachments();
    }
  }

  async loadAttachments(): Promise<void> {
    this.loading = true;
    this.error = '';
    try {
      const agreement = await this._agreementsService.getLabAgreementById(this.data.agreementId);
      this.attachments = agreement.attachments ?? [];
      this.selectedAttachment = this.attachments[0] ?? null;
    } catch (e: unknown) {
      this.error =
        e instanceof Error
          ? e.message
          : this._localization.translate('modules.profile.agreements.errors.loadDetailFailed');
    } finally {
      this.loading = false;
    }
  }

  selectAttachment(attachment: LabAgreementAttachmentDto): void {
    this.selectedAttachment = attachment;
  }

  attachmentName(attachment: LabAgreementAttachmentDto | null): string {
    if (!attachment) return '—';
    return attachment.remark?.trim() || 'ضمیمه';
  }

  attachmentContentType(attachment: LabAgreementAttachmentDto | null): string {
    if (!attachment) return 'application/octet-stream';

    const contentType = attachment.contentType?.trim();
    if (contentType) return contentType;

    const name = this.attachmentName(attachment).toLowerCase();
    if (name.endsWith('.pdf')) return 'application/pdf';
    if (name.endsWith('.png')) return 'image/png';
    if (name.endsWith('.jpg') || name.endsWith('.jpeg')) return 'image/jpeg';
    if (name.endsWith('.gif')) return 'image/gif';
    if (name.endsWith('.webp')) return 'image/webp';
    return 'application/octet-stream';
  }

  attachmentDataUrl(attachment: LabAgreementAttachmentDto | null): string {
    if (!attachment?.fileName?.trim()) return '';
    return `data:${this.attachmentContentType(attachment)};base64,${attachment.fileName}`;
  }

  safePreviewUrl(attachment: LabAgreementAttachmentDto | null): SafeResourceUrl {
    return this._sanitizer.bypassSecurityTrustResourceUrl(this.attachmentDataUrl(attachment));
  }

  isImage(attachment: LabAgreementAttachmentDto | null): boolean {
    return this.attachmentContentType(attachment).startsWith('image/');
  }

  isPdf(attachment: LabAgreementAttachmentDto | null): boolean {
    return this.attachmentContentType(attachment) === 'application/pdf';
  }

  downloadAttachment(attachment: LabAgreementAttachmentDto, event?: Event): void {
    event?.stopPropagation();
    const link = document.createElement('a');
    link.href = this.attachmentDataUrl(attachment);
    link.download = this.attachmentName(attachment);
    link.click();
  }

  close(): void {
    this._dialogRef.close();
  }
}
