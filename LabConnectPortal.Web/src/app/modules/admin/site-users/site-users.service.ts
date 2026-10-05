import { Injectable } from '@angular/core';
import { ApiHttpService } from '@core/services/http/api-http.service';
import { LocalizationService } from '@core/services/localization/localization.service';
import {
  SiteUserPermissionDto,
  SetSiteMemberPermissionsCommand,
} from '@core/services/auth/site-permission.types';
import {
  AddSiteMemberCommand,
  AddSiteMemberResultDto,
  ConfirmAddSiteMemberCommand,
  RemoveSiteMemberCommand,
  SiteMemberDto,
  ToggleSiteMemberActiveCommand,
} from './site-users.types';

@Injectable({ providedIn: 'root' })
export class SiteUsersService {
  constructor(
    private _http: ApiHttpService,
    private _localization: LocalizationService
  ) {}

  async getMembers(): Promise<SiteMemberDto[]> {
    const result = await this._http.get<SiteMemberDto[]>('SiteUser', 'GetMembers');
    if (!result.success || !result.data) {
      throw new Error(
        result.message ??
          this._localization.translate('modules.admin.siteUsers.errors.fetchListFailed')
      );
    }
    return result.data;
  }

  async addMember(command: AddSiteMemberCommand): Promise<AddSiteMemberResultDto> {
    const result = await this._http.post<AddSiteMemberResultDto>(
      'SiteUser',
      'AddMember',
      command
    );
    if (!result.success || !result.data) {
      throw new Error(
        result.message ??
          this._localization.translate('modules.admin.siteUsers.errors.addMemberFailed')
      );
    }
    return result.data;
  }

  async confirmAddMember(command: ConfirmAddSiteMemberCommand): Promise<void> {
    const result = await this._http.post('SiteUser', 'ConfirmAddMember', command);
    if (!result.success) {
      throw new Error(
        result.message ??
          this._localization.translate('modules.admin.siteUsers.errors.confirmAddFailed')
      );
    }
  }

  async toggleMemberActive(
    command: ToggleSiteMemberActiveCommand
  ): Promise<SiteMemberDto> {
    const result = await this._http.post<SiteMemberDto>(
      'SiteUser',
      'ToggleMemberActive',
      command
    );
    if (!result.success || !result.data) {
      throw new Error(
        result.message ??
          this._localization.translate('modules.admin.siteUsers.errors.toggleActiveFailed')
      );
    }
    return result.data;
  }

  async removeMember(command: RemoveSiteMemberCommand): Promise<void> {
    const result = await this._http.post('SiteUser', 'RemoveMember', command);
    if (!result.success) {
      throw new Error(
        result.message ??
          this._localization.translate('modules.admin.siteUsers.errors.removeFailed')
      );
    }
  }

  async getMemberPermissions(memberId: string): Promise<SiteUserPermissionDto[]> {
    const result = await this._http.get<SiteUserPermissionDto[]>(
      'SiteUser',
      'GetMemberPermissions',
      { memberId }
    );
    if (!result.success || !result.data) {
      throw new Error(
        result.message ??
          this._localization.translate(
            'modules.admin.siteUsers.permissions.errors.loadFailed'
          )
      );
    }
    return result.data;
  }

  async setMemberPermissions(
    command: SetSiteMemberPermissionsCommand
  ): Promise<void> {
    const result = await this._http.post('SiteUser', 'SetMemberPermissions', command);
    if (!result.success) {
      throw new Error(
        result.message ??
          this._localization.translate(
            'modules.admin.siteUsers.permissions.errors.saveFailed'
          )
      );
    }
  }
}
