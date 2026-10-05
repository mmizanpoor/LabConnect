import { Component, Inject } from '@angular/core';
import {
  FormControl,
  FormGroup,
  ReactiveFormsModule,
  Validators,
} from '@angular/forms';
import {
  MAT_DIALOG_DATA,
  MatDialogModule,
  MatDialogRef,
} from '@angular/material/dialog';
import { MatButtonModule } from '@angular/material/button';
import { TranslocoPipe } from '@jsverse/transloco';
import { BaseButtonComponent } from '@modules/base/components/base-button/base-button.component';
import { BaseDialogComponent } from '@modules/base/components/base-dialog/base-dialog.component';
import { BaseFormFieldComponent } from '@modules/base/components/base-form-field/base-form-field.component';
import { LaboratoriesService } from './laboratories.service';
import { CenterProfileListItemDto } from './laboratories.types';

export interface LaboratoryFormDialogData {
  mode: 'create' | 'editMobile';
  laboratory?: CenterProfileListItemDto;
}

@Component({
  selector: 'app-laboratory-form-dialog',
  standalone: true,
  imports: [
    ReactiveFormsModule,
    MatDialogModule,
    MatButtonModule,
    TranslocoPipe,
    BaseButtonComponent,
    BaseDialogComponent,
    BaseFormFieldComponent,
  ],
  template: `
    <base-dialog
      [title]="
        (data.mode === 'create'
          ? 'modules.admin.laboratories.addTitle'
          : 'modules.admin.laboratories.editMobileTitle') | transloco
      "
    >
      <div class="min-w-[320px]" dir="rtl">
        @if (error) {
          <div class="mb-4 rounded-lg bg-red-50 p-3 text-sm text-red-700">{{ error }}</div>
        }
        <form [formGroup]="form" class="flex flex-col gap-5">
          @if (data.mode === 'create') {
            <base-form-field
              layout="floating"
              [required]="true"
              [label]="'modules.admin.laboratories.fields.centerCode' | transloco"
              [control]="form.controls.labCodeNew"
            />
          }
          <base-form-field
            layout="floating"
            [required]="true"
            [label]="'shared.mobile' | transloco"
            [control]="form.controls.mobileNumber"
          />
        </form>
      </div>

      <div baseDialogActions class="flex items-center justify-end gap-2">
        <base-button type="button" color="warn" size="sm" (click)="cancel()">
          {{ 'shared.cancel' | transloco }}
        </base-button>
        <base-button
          color="green"
          size="sm"
          [disabled]="saving || form.invalid"
          [loading]="saving"
          (click)="save()"
        >
          {{ 'shared.save' | transloco }}
        </base-button>
      </div>
    </base-dialog>
  `,
})
export class LaboratoryFormDialogComponent {
  saving = false;
  error = '';

  form = new FormGroup({
    labCodeNew: new FormControl('', {
      validators: [Validators.required, Validators.pattern(/^\d{5}$/)],
      nonNullable: true,
    }),
    mobileNumber: new FormControl('', {
      validators: [Validators.required, Validators.pattern(/^09\d{9}$/)],
      nonNullable: true,
    }),
  });

  constructor(
    @Inject(MAT_DIALOG_DATA) readonly data: LaboratoryFormDialogData,
    private _dialogRef: MatDialogRef<LaboratoryFormDialogComponent, boolean>,
    private _laboratoriesService: LaboratoriesService
  ) {
    if (data.mode === 'editMobile') {
      this.form.controls.labCodeNew.clearValidators();
      this.form.controls.labCodeNew.updateValueAndValidity();
      if (data.laboratory?.mobileNumber) {
        this.form.controls.mobileNumber.setValue(data.laboratory.mobileNumber);
      }
    }
  }

  cancel(): void {
    this._dialogRef.close(false);
  }

  async save(): Promise<void> {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    this.saving = true;
    this.error = '';

    const mobileNumber = this.form.controls.mobileNumber.value.trim();

    if (this.data.mode === 'create') {
      const labCodeNew = Number(this.form.controls.labCodeNew.value);
      const result = await this._laboratoriesService.createLaboratory({
        labCodeNew,
        mobileNumber,
      });
      if (!result.success) {
        this.error = result.message ?? '';
        this.saving = false;
        return;
      }
    } else if (this.data.laboratory) {
      const result = await this._laboratoriesService.updateLaboratoryMobile({
        profileId: this.data.laboratory.id,
        mobileNumber,
      });
      if (!result.success) {
        this.error = result.message ?? '';
        this.saving = false;
        return;
      }
    }

    this.saving = false;
    this._dialogRef.close(true);
  }
}
