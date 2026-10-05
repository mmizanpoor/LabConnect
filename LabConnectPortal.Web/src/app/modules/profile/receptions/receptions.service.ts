import { Injectable } from '@angular/core';
import { ApiHttpService } from '@core/services/http/api-http.service';
import { LocalizationService } from '@core/services/localization/localization.service';
import {
  AdminReceptionDashboardSection,
  AdminReceptionDashboardStats,
  ClearReceiverReceptionCommand,
  ReceiveReceptionGroupFilterQuery,
  ReceptionDto,
  SRLabNameDto,
} from './receptions.types';

@Injectable({ providedIn: 'root' })
export class ReceptionsService {
  constructor(
    private _http: ApiHttpService,
    private _localization: LocalizationService,
  ) {}

  async getAllSRLabs(labCode: number, incoming: boolean): Promise<SRLabNameDto[]> {
    const result = await this._http.get<SRLabNameDto[]>('Reception', 'GetAllSRLabs', {
      labCode,
      incoming: String(incoming),
    });
    if (!result.success || !result.data) {
      throw new Error(
        result.message ?? this._localization.translate('modules.profile.receptions.errors.loadLabsFailed'),
      );
    }
    return result.data;
  }

  async getReceptions(filter: ReceiveReceptionGroupFilterQuery): Promise<ReceptionDto[]> {
    const result = await this._http.post<ReceptionDto[]>('Reception', 'GetReceptions', filter);
    if (!result.success || !result.data) {
      throw new Error(
        result.message ?? this._localization.translate('modules.profile.receptions.errors.loadFailed'),
      );
    }
    return result.data;
  }

  async clearReceiverReceptions(command: ClearReceiverReceptionCommand): Promise<void> {
    const result = await this._http.post('Reception', 'ClearReceiverReceptions', command);
    if (!result.success) {
      throw new Error(
        result.message ??
          this._localization.translate('modules.profile.receptions.errors.clearReceiverFailed'),
      );
    }
  }

  async getLabs(labcodes: number[]): Promise<SRLabNameDto[]> {
    if (!labcodes.length) return [];

    const result = await this._http.post<SRLabNameDto[]>('Reception', 'GetLabs', labcodes);
    if (!result.success || !result.data) {
      throw new Error(
        result.message ?? this._localization.translate('modules.profile.receptions.errors.loadLabsFailed'),
      );
    }
    return result.data;
  }

  buildLabNameMap(labs: SRLabNameDto[]): Record<number, string> {
    const map: Record<number, string> = {};
    for (const lab of labs) {
      const name = lab.vchLabName?.trim();
      if (!name) continue;
      map[lab.intLabId] = name;
      if (lab.intLabIdNew != null) {
        map[lab.intLabIdNew] = name;
      }
    }
    return map;
  }

  resolveLabName(map: Record<number, string>, labCode: number): string {
    return map[labCode]?.trim() || String(labCode);
  }

  async getAdminDashboardStats(
    labCode?: number,
    section?: AdminReceptionDashboardSection,
  ): Promise<AdminReceptionDashboardStats> {
    const result = await this._http.get<AdminReceptionDashboardStats>(
      'Reception',
      'GetAdminDashboardStats',
      {
        ...(labCode != null ? { labCode } : {}),
        ...(section != null ? { section } : {}),
      },
    );
    if (!result.success || !result.data) {
      throw new Error(
        result.message ?? this._localization.translate('modules.profile.receptions.errors.loadStatsFailed'),
      );
    }
    return result.data;
  }
}
