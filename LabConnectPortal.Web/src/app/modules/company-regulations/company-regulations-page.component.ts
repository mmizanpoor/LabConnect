import { Component, OnInit, inject } from '@angular/core';
import { DomSanitizer, SafeHtml } from '@angular/platform-browser';
import { TranslocoPipe } from '@jsverse/transloco';
import { LocalizationService } from '@core/services/localization/localization.service';
import {
  CompanyRegulationDto,
  CompanyRegulationType,
  companyRegulationTypeSortOrder,
  normalizeCompanyRegulationType,
} from '@modules/admin/company-regulations/company-regulations.types';
import { PublicCompanyRegulationsService } from './public-company-regulations.service';

@Component({
  selector: 'app-company-regulations-page',
  standalone: true,
  imports: [TranslocoPipe],
  templateUrl: './company-regulations-page.component.html',
})
export class CompanyRegulationsPageComponent implements OnInit {
  private _service = inject(PublicCompanyRegulationsService);
  private _localization = inject(LocalizationService);
  private _sanitizer = inject(DomSanitizer);

  loading = true;
  error = '';
  items: CompanyRegulationDto[] = [];

  async ngOnInit(): Promise<void> {
    try {
      const result = await this._service.getAll();
      if (!result.success || !result.data) {
        throw new Error(
          result.message ?? this._localization.translate('modules.companyRegulations.errors.loadFailed'),
        );
      }
      this.items = [...result.data].sort(
        (a, b) => companyRegulationTypeSortOrder(a.type) - companyRegulationTypeSortOrder(b.type),
      );
    } catch (e: unknown) {
      this.error =
        e instanceof Error
          ? e.message
          : this._localization.translate('modules.companyRegulations.errors.loadFailed');
    } finally {
      this.loading = false;
    }
  }

  typeLabel(type: CompanyRegulationDto['type']): string {
    switch (normalizeCompanyRegulationType(type)) {
      case CompanyRegulationType.RulesAndRegulations:
        return this._localization.translate(
          'modules.admin.companyRegulations.types.rulesAndRegulations',
        );
      case CompanyRegulationType.CompanyPolicy:
        return this._localization.translate('modules.admin.companyRegulations.types.companyPolicy');
      default:
        return '';
    }
  }

  safeBody(html: string): SafeHtml {
    return this._sanitizer.bypassSecurityTrustHtml(html || '');
  }
}
