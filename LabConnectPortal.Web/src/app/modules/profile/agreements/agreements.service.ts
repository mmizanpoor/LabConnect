import { Injectable } from '@angular/core';
import { ApiHttpService } from '@core/services/http/api-http.service';
import { LocalizationService } from '@core/services/localization/localization.service';
import {
  GetLabAgreementQuery,
  LabAgreementDto,
  LabAgreementLabStats,
  LabAgreementStats,
  SRLabNameDto,
} from './agreements.types';
import { CreateLabAgreementFromPortalCommand } from '../special-offers/special-offers.types';

@Injectable({ providedIn: 'root' })
export class AgreementsService {
  constructor(
    private _http: ApiHttpService,
    private _localization: LocalizationService,
  ) {}

  async getLabAgreements(query: GetLabAgreementQuery): Promise<LabAgreementDto[]> {
    const result = await this._http.post<LabAgreementDto[]>('LabAgreement', 'GetLabAgreements', query);
    if (!result.success || !result.data) {
      throw new Error(
        result.message ?? this._localization.translate('modules.profile.agreements.errors.loadFailed'),
      );
    }
    return result.data;
  }

  async getLabAgreementById(id: number): Promise<LabAgreementDto> {
    const result = await this._http.get<LabAgreementDto>('LabAgreement', 'GetLabAgreementById', { id });
    if (!result.success || !result.data) {
      throw new Error(
        result.message ?? this._localization.translate('modules.profile.agreements.errors.loadDetailFailed'),
      );
    }
    return result.data;
  }

  async getLabs(labcodes: number[]): Promise<SRLabNameDto[]> {
    if (!labcodes.length) return [];

    const result = await this._http.post<SRLabNameDto[]>('Reception', 'GetLabs', labcodes);
    if (!result.success || !result.data) {
      throw new Error(
        result.message ?? this._localization.translate('modules.profile.agreements.errors.loadLabsFailed'),
      );
    }
    return result.data;
  }

  collectAgreementLabCodes(agreements: LabAgreementDto[]): number[] {
    const codes = new Set<number>();
    for (const agreement of agreements) {
      if (agreement.primaryAgreementLabCodeNew) {
        codes.add(agreement.primaryAgreementLabCodeNew);
      }
      if (agreement.receiverAgreementLabCodeNew) {
        codes.add(agreement.receiverAgreementLabCodeNew);
      }
    }
    return [...codes];
  }

  buildLabNameMap(labs: SRLabNameDto[]): Record<number, string> {
    const map: Record<number, string> = {};
    for (const lab of labs) {
      const name = lab.vchLabName?.trim();
      if (!name) continue;
      if (lab.intLabIdNew != null) {
        map[lab.intLabIdNew] = name;
      }
      if (lab.intLabId != null) {
        map[lab.intLabId] = name;
      }
    }
    return map;
  }

  mergeLabNameMaps(...maps: Record<number, string>[]): Record<number, string> {
    return Object.assign({}, ...maps);
  }

  resolveLabName(map: Record<number, string>, labCodeNew: number): string {
    return map[labCodeNew]?.trim() || String(labCodeNew);
  }

  async loadLabNamesForAgreements(
    agreements: LabAgreementDto[],
    existingMap: Record<number, string> = {},
  ): Promise<Record<number, string>> {
    const codes = this.collectAgreementLabCodes(agreements).filter((code) => !existingMap[code]);
    if (!codes.length) return existingMap;

    const labs = await this.getLabs(codes);
    return this.mergeLabNameMaps(existingMap, this.buildLabNameMap(labs));
  }

  async createFromPortal(command: CreateLabAgreementFromPortalCommand): Promise<number> {
    const result = await this._http.post<number>('LabAgreement', 'CreateFromPortal', command);
    if (!result.success || result.data == null) {
      throw new Error(
        result.message ?? this._localization.translate('modules.profile.agreements.form.errors.saveFailed'),
      );
    }
    return result.data;
  }

  async getGlobalStats(): Promise<LabAgreementStats> {
    const result = await this._http.get<LabAgreementStats>('LabAgreement', 'GetStats');
    if (!result.success || !result.data) {
      throw new Error(
        result.message ?? this._localization.translate('modules.profile.agreements.errors.loadStatsFailed'),
      );
    }
    return result.data;
  }

  async getStatsForCurrentLab(): Promise<LabAgreementLabStats> {
    const result = await this._http.get<LabAgreementLabStats>('LabAgreement', 'GetStatsForCurrentLab');
    if (!result.success || !result.data) {
      throw new Error(
        result.message ?? this._localization.translate('modules.profile.agreements.errors.loadStatsFailed'),
      );
    }
    return result.data;
  }
}
