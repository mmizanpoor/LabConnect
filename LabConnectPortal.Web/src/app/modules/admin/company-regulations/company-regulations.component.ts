import { Component, DestroyRef, OnInit, inject } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { Router } from '@angular/router';
import { TranslocoPipe } from '@jsverse/transloco';
import { LocalizationService } from '@core/services/localization/localization.service';
import { SitePermissionService } from '@core/services/auth/site-permission.service';
import { SystemEntity } from '@core/system-entity/system-entity';
import { bindSystemEntity } from '@core/system-entity/bind-system-entity';
import { BaseButtonComponent } from '@modules/base/components/base-button/base-button.component';
import { BaseGridComponent } from '@modules/base/components/base-grid/base-grid.component';
import { ColDef } from 'ag-grid-community';
import { BehaviorSubject } from 'rxjs';
import { CompanyRegulationsColDef } from './company-regulations.coldef';
import { CompanyRegulationsService } from './company-regulations.service';
import {
  COMPANY_REGULATION_TYPES,
  CompanyRegulationDto,
  CompanyRegulationType,
  normalizeCompanyRegulationType,
} from './company-regulations.types';

@Component({
  selector: 'app-company-regulations',
  standalone: true,
  imports: [TranslocoPipe, BaseButtonComponent, BaseGridComponent],
  templateUrl: './company-regulations.component.html',
  styles: `
    :host {
      display: block;
      height: 100%;
      min-height: 0;
    }
  `,
})
export class CompanyRegulationsComponent implements OnInit {
  readonly entity = SystemEntity.CompanyRegulation;
  private readonly _systemEntityBinding = bindSystemEntity(SystemEntity.CompanyRegulation);
  private readonly _destroyRef = inject(DestroyRef);

  loading = false;
  error = '';
  rows: CompanyRegulationDto[] = [];
  colDef: ColDef[] = [];
  readonly list$ = new BehaviorSubject<CompanyRegulationDto[]>([]);

  constructor(
    private _service: CompanyRegulationsService,
    private _router: Router,
    private _localization: LocalizationService,
    private _colDef: CompanyRegulationsColDef,
    private _sitePermission: SitePermissionService,
  ) {}

  get canCreate(): boolean {
    return this._sitePermission.can(this.entity, 'create') && this.availableTypeCount > 0;
  }

  get canUpdate(): boolean {
    return this._sitePermission.can(this.entity, 'update');
  }

  get canDelete(): boolean {
    return this._sitePermission.can(this.entity, 'delete');
  }

  get availableTypeCount(): number {
    const used = new Set(
      this.rows
        .map((row) => normalizeCompanyRegulationType(row.type))
        .filter((type): type is CompanyRegulationType => type != null),
    );
    return COMPANY_REGULATION_TYPES.filter((type) => !used.has(type)).length;
  }

  ngOnInit(): void {
    this.colDef = this._colDef.get();
    this._colDef.actionClicked
      .pipe(takeUntilDestroyed(this._destroyRef))
      .subscribe((evt) => {
        if (!evt?.row) return;
        if (evt.type === 'edit') {
          if (!this.canUpdate) return;
          this.openEdit(evt.row);
        }
        if (evt.type === 'delete') {
          if (!this.canDelete) return;
          void this.deleteRow(evt.row);
        }
      });
    void this.load();
  }

  async load(): Promise<void> {
    this.loading = true;
    this.error = '';
    try {
      const result = await this._service.getAll();
      if (!result.success || !result.data) {
        throw new Error(
          result.message ??
            this._localization.translate('modules.admin.companyRegulations.errors.loadFailed'),
        );
      }
      this.rows = result.data;
      this.list$.next(this.rows);
    } catch (e: unknown) {
      this.error =
        e instanceof Error
          ? e.message
          : this._localization.translate('modules.admin.companyRegulations.errors.loadFailed');
      this.rows = [];
      this.list$.next([]);
    } finally {
      this.loading = false;
    }
  }

  openCreate(): void {
    if (!this.canCreate) return;
    void this._router.navigate(['/admin/company-regulations/new']);
  }

  openEdit(row: CompanyRegulationDto): void {
    if (!this.canUpdate) return;
    void this._router.navigate(['/admin/company-regulations', row.companyRegulationId]);
  }

  async deleteRow(row: CompanyRegulationDto): Promise<void> {
    if (!this.canDelete) return;
    const normalizedType = normalizeCompanyRegulationType(row.type);
    const typeLabel =
      normalizedType === CompanyRegulationType.RulesAndRegulations
        ? this._localization.translate(
            'modules.admin.companyRegulations.types.rulesAndRegulations',
          )
        : this._localization.translate('modules.admin.companyRegulations.types.companyPolicy');
    const message = this._localization.translate(
      'modules.admin.companyRegulations.confirmDelete',
      { title: typeLabel },
    );
    if (!confirm(message)) return;

    const result = await this._service.delete(row.companyRegulationId);
    if (!result.success) {
      this.error =
        result.message ??
        this._localization.translate('modules.admin.companyRegulations.errors.deleteFailed');
      return;
    }
    await this.load();
  }
}
