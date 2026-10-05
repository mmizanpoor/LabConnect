import { Component, OnInit } from '@angular/core';
import { FormControl, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { TranslocoPipe } from '@jsverse/transloco';
import { LocalizationService } from '@core/services/localization/localization.service';
import { SystemEntity } from '@core/system-entity/system-entity';
import { bindSystemEntity } from '@core/system-entity/bind-system-entity';
import { BaseBackButtonComponent } from '@modules/base/components/base-back-button/base-back-button.component';
import { BaseButtonComponent } from '@modules/base/components/base-button/base-button.component';
import {
  BaseFormFieldComponent,
  BaseFormSelectOption,
} from '@modules/base/components/base-form-field/base-form-field.component';
import { RichTextEditorComponent } from '@modules/base/components/rich-text-editor/rich-text-editor.component';
import { CompanyRegulationsService } from './company-regulations.service';
import {
  COMPANY_REGULATION_TYPES,
  CompanyRegulationDto,
  CompanyRegulationType,
  normalizeCompanyRegulationType,
} from './company-regulations.types';

@Component({
  selector: 'app-company-regulation-form',
  standalone: true,
  imports: [
    ReactiveFormsModule,
    TranslocoPipe,
    BaseBackButtonComponent,
    BaseButtonComponent,
    BaseFormFieldComponent,
    RichTextEditorComponent,
  ],
  templateUrl: './company-regulation-form.component.html',
})
export class CompanyRegulationFormComponent implements OnInit {
  readonly entity = SystemEntity.CompanyRegulation;
  private readonly _systemEntityBinding = bindSystemEntity(SystemEntity.CompanyRegulation);

  loading = false;
  saving = false;
  error = '';
  isEdit = false;
  regulationId = '';
  availableTypes: CompanyRegulationType[] = [...COMPANY_REGULATION_TYPES];

  form = new FormGroup({
    type: new FormControl<CompanyRegulationType | null>(null, Validators.required),
    body: new FormControl('', Validators.required),
  });

  constructor(
    private _route: ActivatedRoute,
    private _router: Router,
    private _service: CompanyRegulationsService,
    private _localization: LocalizationService,
  ) {}

  get typeOptions(): BaseFormSelectOption[] {
    return this.availableTypes.map((type) => ({
      value: type,
      label: this.typeLabel(type),
    }));
  }

  ngOnInit(): void {
    this.regulationId = this._route.snapshot.paramMap.get('id') ?? '';
    this.isEdit = this.regulationId !== '' && this.regulationId !== 'new';
    void this.load();
  }

  async load(): Promise<void> {
    this.loading = true;
    this.error = '';
    try {
      const allResult = await this._service.getAll();
      if (!allResult.success || !allResult.data) {
        throw new Error(
          allResult.message ??
            this._localization.translate('modules.admin.companyRegulations.errors.loadFailed'),
        );
      }

      if (this.isEdit) {
        const existing = allResult.data.find((x) => x.companyRegulationId === this.regulationId);
        if (!existing) {
          throw new Error(
            this._localization.translate('modules.admin.companyRegulations.errors.loadFailed'),
          );
        }
        this.availableTypes = [normalizeCompanyRegulationType(existing.type)!];
        this.patchForm(existing);
        this.form.controls.type.disable();
      } else {
        const used = new Set(
          allResult.data
            .map((x) => normalizeCompanyRegulationType(x.type))
            .filter((type): type is CompanyRegulationType => type != null),
        );
        this.availableTypes = COMPANY_REGULATION_TYPES.filter((type) => !used.has(type));
        if (!this.availableTypes.length) {
          this.error = this._localization.translate(
            'modules.admin.companyRegulations.errors.allTypesExist',
          );
          this.form.disable();
          return;
        }
        this.form.controls.type.setValue(this.availableTypes[0]);
      }
    } catch (e: unknown) {
      this.error =
        e instanceof Error
          ? e.message
          : this._localization.translate('modules.admin.companyRegulations.errors.loadFailed');
    } finally {
      this.loading = false;
    }
  }

  patchForm(item: CompanyRegulationDto): void {
    this.form.patchValue({
      type: normalizeCompanyRegulationType(item.type),
      body: item.body,
    });
  }

  async save(): Promise<void> {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      this.error = this._localization.translate(
        'modules.admin.companyRegulations.errors.validationFailed',
      );
      return;
    }

    this.saving = true;
    this.error = '';
    const value = this.form.getRawValue();

    try {
      const result = this.isEdit
        ? await this._service.update({
            companyRegulationId: this.regulationId,
            body: value.body ?? '',
          })
        : await this._service.create({
            type: value.type!,
            body: value.body ?? '',
          });

      if (!result.success || !result.data) {
        throw new Error(
          result.message ??
            this._localization.translate('modules.admin.companyRegulations.errors.saveFailed'),
        );
      }

      void this._router.navigate(['/admin/company-regulations']);
    } catch (e: unknown) {
      this.error =
        e instanceof Error
          ? e.message
          : this._localization.translate('modules.admin.companyRegulations.errors.saveFailed');
    } finally {
      this.saving = false;
    }
  }

  backToList(): void {
    void this._router.navigate(['/admin/company-regulations']);
  }

  private typeLabel(type: CompanyRegulationType): string {
    if (type === CompanyRegulationType.RulesAndRegulations) {
      return this._localization.translate(
        'modules.admin.companyRegulations.types.rulesAndRegulations',
      );
    }
    return this._localization.translate('modules.admin.companyRegulations.types.companyPolicy');
  }
}
