import { Component, Inject } from '@angular/core';
import { MAT_DIALOG_DATA, MatDialogModule } from '@angular/material/dialog';
import { TranslocoPipe } from '@jsverse/transloco';
import { BaseDialogComponent } from '@modules/base/components/base-dialog/base-dialog.component';
import { ProductResumeApplicationDto } from '../../products/products.types';
import { resumeApplicationStatusKey } from '../product-resume-applications/product-resume-application-status';

export interface MyResumeApplicationStatusDialogData {
  application: ProductResumeApplicationDto;
}

@Component({
  selector: 'app-my-resume-application-status-dialog',
  standalone: true,
  imports: [MatDialogModule, TranslocoPipe, BaseDialogComponent],
  template: `
    <base-dialog [title]="'modules.profile.myResumeApplications.statusTitle' | transloco">
      <div class="space-y-3 text-sm" dir="rtl">
        <div>
          <p class="mb-1 text-xs text-slate-500">
            {{ 'modules.profile.myResumeApplications.columns.listing' | transloco }}
          </p>
          <p class="font-semibold text-slate-800">{{ data.application.productTitle }}</p>
        </div>
        <div>
          <p class="mb-1 text-xs text-slate-500">
            {{ 'modules.profile.myResumeApplications.columns.status' | transloco }}
          </p>
          <p class="font-semibold" [class]="statusClass">{{ statusLabelKey | transloco }}</p>
        </div>
        @if (data.application.reviewNotes.trim()) {
          <div>
            <p class="mb-1 text-xs text-slate-500">
              {{ 'modules.profile.productResumeApplications.review.reason' | transloco }}
            </p>
            <p class="whitespace-pre-line rounded-xl bg-slate-50 p-3 text-slate-700">
              {{ data.application.reviewNotes }}
            </p>
          </div>
        }
      </div>
    </base-dialog>
  `,
})
export class MyResumeApplicationStatusDialogComponent {
  constructor(
    @Inject(MAT_DIALOG_DATA) readonly data: MyResumeApplicationStatusDialogData,
  ) {}

  get statusLabelKey(): string {
    const key = resumeApplicationStatusKey(this.data.application.status);
    return `modules.profile.productResumeApplications.status.${key}`;
  }

  get statusClass(): string {
    const key = resumeApplicationStatusKey(this.data.application.status);
    if (key === 'approved') return 'text-emerald-700';
    if (key === 'rejected') return 'text-red-700';
    return 'text-amber-700';
  }
}
