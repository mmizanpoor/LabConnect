import { Component, Inject, Optional } from '@angular/core';
import { FormControl, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { MAT_DIALOG_DATA, MatDialogModule, MatDialogRef } from '@angular/material/dialog';
import { MatIconModule } from '@angular/material/icon';
import { TranslocoPipe } from '@jsverse/transloco';
import { BaseButtonComponent } from '@modules/base/components/base-button/base-button.component';
import { BaseCheckboxComponent } from '@modules/base/components/base-checkbox/base-checkbox.component';
import { BaseDialogComponent } from '@modules/base/components/base-dialog/base-dialog.component';
import { BaseFormFieldComponent } from '@modules/base/components/base-form-field/base-form-field.component';
import { ApiKeysService } from './api-keys.service';
import { ApiKeyDto } from './api-keys.types';

export interface ApiKeyDialogData {
  item: ApiKeyDto;
  adminMode: boolean;
}

@Component({
  selector: 'app-api-key-create-dialog',
  standalone: true,
  imports: [
    ReactiveFormsModule,
    MatDialogModule,
    MatIconModule,
    TranslocoPipe,
    BaseButtonComponent,
    BaseCheckboxComponent,
    BaseDialogComponent,
    BaseFormFieldComponent,
  ],
  template: `
    <base-dialog [title]="(editing ? 'shared.apiKeys.editTitle' : 'shared.apiKeys.createTitle') | transloco">
      <div class="min-w-[min(78vw,420px)]" dir="rtl">
        @if (error) {
          <div class="mb-4 rounded-md bg-red-50 p-3 text-sm text-red-700">{{ error }}</div>
        }

        @if (!createdKey) {
          <form [formGroup]="form" class="flex flex-col gap-4">
            <base-form-field
              layout="floating"
              [required]="true"
              [maxLength]="200"
              [label]="'shared.apiKeys.name' | transloco"
              [control]="form.controls.name"
            />
            <base-checkbox formControlName="allowAdd">
              {{ 'shared.apiKeys.allowAdd' | transloco }}
            </base-checkbox>
            <base-checkbox formControlName="allowEdit">
              {{ 'shared.apiKeys.allowEdit' | transloco }}
            </base-checkbox>
            <base-checkbox formControlName="allowView">
              {{ 'shared.apiKeys.allowView' | transloco }}
            </base-checkbox>
          </form>
        } @else {
          <div class="rounded-md border border-amber-200 bg-amber-50 p-4">
            <p class="mb-3 mt-0 text-sm leading-7 text-amber-900">
              {{ 'shared.apiKeys.copyWarning' | transloco }}
            </p>
            <div class="flex items-center gap-2 rounded-md border border-slate-200 bg-white p-2" dir="ltr">
              <code class="min-w-0 flex-1 break-all text-xs text-slate-700">{{ createdKey }}</code>
              <button
                type="button"
                class="inline-flex h-9 w-9 shrink-0 cursor-pointer items-center justify-center rounded-md border-0 bg-slate-100 text-slate-700"
                (click)="copyKey()"
                [attr.aria-label]="'shared.apiKeys.copy' | transloco"
              >
                <mat-icon class="!h-5 !w-5 !text-[1.25rem]">content_copy</mat-icon>
              </button>
            </div>
            @if (copied) {
              <p class="mb-0 mt-2 text-xs font-medium text-emerald-700">
                {{ 'shared.apiKeys.copied' | transloco }}
              </p>
            }
          </div>
        }
      </div>

      <div baseDialogActions class="flex items-center justify-end gap-2">
        @if (!createdKey) {
          <base-button type="button" color="warn" size="sm" (click)="close()">
            {{ 'shared.cancel' | transloco }}
          </base-button>
          <base-button
            type="button"
            color="green"
            size="sm"
            [disabled]="form.invalid || saving"
            [loading]="saving"
            (click)="save()"
          >
            {{ (editing ? 'shared.save' : 'shared.apiKeys.create') | transloco }}
          </base-button>
        } @else {
          <base-button type="button" size="sm" (click)="close(true)">
            {{ 'shared.close' | transloco }}
          </base-button>
        }
      </div>
    </base-dialog>
  `,
})
export class ApiKeyCreateDialogComponent {
  saving = false;
  error = '';
  createdKey = '';
  copied = false;

  get editing(): boolean {
    return !!this.data?.item;
  }

  readonly form = new FormGroup({
    name: new FormControl('', {
      nonNullable: true,
      validators: [Validators.required, Validators.maxLength(200)],
    }),
    allowAdd: new FormControl(false, { nonNullable: true }),
    allowEdit: new FormControl(false, { nonNullable: true }),
    allowView: new FormControl(false, { nonNullable: true }),
  });

  constructor(
    private _dialogRef: MatDialogRef<ApiKeyCreateDialogComponent, boolean>,
    private _apiKeys: ApiKeysService,
    @Optional() @Inject(MAT_DIALOG_DATA) readonly data: ApiKeyDialogData | null,
  ) {
    if (data?.item) {
      this.form.setValue({
        name: data.item.name,
        allowAdd: data.item.allowAdd,
        allowEdit: data.item.allowEdit,
        allowView: data.item.allowView,
      });
    }
  }

  async save(): Promise<void> {
    if (this.form.invalid || this.saving) return;
    this.saving = true;
    this.error = '';
    try {
      const values = this.form.getRawValue();
      const result = this.editing
        ? await (this.data!.adminMode
            ? this._apiKeys.update({ id: this.data!.item.id, ...values })
            : this._apiKeys.updateMine({ id: this.data!.item.id, ...values }))
        : await this._apiKeys.create(values);
      if (result.success && result.data) {
        if (this.editing) {
          this.close(true);
          return;
        }
        this.createdKey = result.data.keyName;
        return;
      }
      this.error = result.message || '';
    } catch {
      this.error = this.editing ? 'خطا در ویرایش کلید API' : 'خطا در ایجاد کلید API';
    } finally {
      this.saving = false;
    }
  }

  async copyKey(): Promise<void> {
    await navigator.clipboard.writeText(this.createdKey);
    this.copied = true;
  }

  close(created = false): void {
    this._dialogRef.close(created || !!this.createdKey);
  }
}
