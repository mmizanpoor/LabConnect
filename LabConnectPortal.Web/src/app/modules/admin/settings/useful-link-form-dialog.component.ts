import { Component } from '@angular/core';
import {
  AbstractControl,
  FormControl,
  FormGroup,
  ReactiveFormsModule,
  ValidationErrors,
  Validators,
} from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatDialogModule, MatDialogRef } from '@angular/material/dialog';
import { TranslocoPipe } from '@jsverse/transloco';
import { BaseButtonComponent } from '@modules/base/components/base-button/base-button.component';
import { BaseDialogComponent } from '@modules/base/components/base-dialog/base-dialog.component';
import { BaseFormFieldComponent } from '@modules/base/components/base-form-field/base-form-field.component';

export interface UsefulLinkFormDialogResult {
  title: string;
  url: string;
}

function urlValidator(control: AbstractControl): ValidationErrors | null {
  const value = (control.value as string | null)?.trim() ?? '';
  if (!value) return null;
  if (
    value.startsWith('/') ||
    value.startsWith('http://') ||
    value.startsWith('https://')
  ) {
    return null;
  }
  return { invalidUrl: true };
}

@Component({
  selector: 'app-useful-link-form-dialog',
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
    <base-dialog [title]="'modules.admin.settings.actions.addUsefulLinkTitle' | transloco">
      <div class="min-w-[320px]" dir="rtl">
        <form [formGroup]="form" class="flex flex-col gap-5">
          <base-form-field
            class="w-full"
            layout="floating"
            [required]="true"
            [label]="'modules.admin.settings.columns.linkTitle' | transloco"
            [control]="form.controls.title"
          />
          <base-form-field
            class="w-full"
            layout="floating"
            type="url"
            [required]="true"
            [label]="'modules.admin.settings.columns.linkUrl' | transloco"
            [control]="form.controls.url"
          />
        </form>
      </div>

      <div baseDialogActions class="flex items-center justify-end gap-2">
        <base-button type="button" color="warn" size="sm" (click)="cancel()">
          {{ 'shared.cancel' | transloco }}
        </base-button>
        <base-button color="green" size="sm" [disabled]="form.invalid" (click)="save()">
          {{ 'shared.save' | transloco }}
        </base-button>
      </div>
    </base-dialog>
  `,
})
export class UsefulLinkFormDialogComponent {
  form = new FormGroup({
    title: new FormControl('', Validators.required),
    url: new FormControl('', [Validators.required, urlValidator]),
  });

  constructor(
    private _dialogRef: MatDialogRef<
      UsefulLinkFormDialogComponent,
      UsefulLinkFormDialogResult | undefined
    >
  ) {}

  cancel(): void {
    this._dialogRef.close();
  }

  save(): void {
    if (this.form.invalid) return;
    const value = this.form.getRawValue();
    this._dialogRef.close({
      title: (value.title ?? '').trim(),
      url: (value.url ?? '').trim(),
    });
  }
}
