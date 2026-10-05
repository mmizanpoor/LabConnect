import { Injectable } from '@angular/core';
import { ApiHttpService } from '@core/services/http/api-http.service';
import { LocalizationService } from '@core/services/localization/localization.service';
import {
  AddLabMemberCommand,
  AddLabMemberResultDto,
  ConfirmAddLabMemberCommand,
  LabMemberDto,
  RemoveLabMemberCommand,
  ToggleLabMemberActiveCommand,
} from './lab-users.types';
import {
  LabUserPermissionDto,
  SetLabMemberPermissionsCommand,
} from '@core/services/auth/lab-permission.types';

@Injectable({ providedIn: 'root' })
export class LabUsersService {
  constructor(
    private _http: ApiHttpService,
    private _localization: LocalizationService,
  ) {}

  async getMembers(): Promise<LabMemberDto[]> {
    const result = await this._http.get<LabMemberDto[]>('LabUser', 'GetMembers');
    if (!result.success || !result.data) {
      throw new Error(result.message ?? this._localization.translate('modules.profile.labUsers.errors.fetchListFailed'));
    }
    return result.data;
  }

  async addMember(command: AddLabMemberCommand): Promise<AddLabMemberResultDto> {
    const result = await this._http.post<AddLabMemberResultDto>('LabUser', 'AddMember', command);
    if (!result.success || !result.data) {
      throw new Error(result.message ?? this._localization.translate('modules.profile.labUsers.errors.addMemberFailed'));
    }
    return result.data;
  }

  async confirmAddMember(command: ConfirmAddLabMemberCommand): Promise<void> {
    const result = await this._http.post('LabUser', 'ConfirmAddMember', command);
    if (!result.success) {
      throw new Error(result.message ?? this._localization.translate('modules.profile.labUsers.errors.confirmAddFailed'));
    }
  }

  async toggleMemberActive(command: ToggleLabMemberActiveCommand): Promise<LabMemberDto> {
    const result = await this._http.post<LabMemberDto>('LabUser', 'ToggleMemberActive', command);
    if (!result.success || !result.data) {
      throw new Error(result.message ?? this._localization.translate('modules.profile.labUsers.errors.toggleActiveFailed'));
    }
    return result.data;
  }

  async removeMember(command: RemoveLabMemberCommand): Promise<void> {
    const result = await this._http.post('LabUser', 'RemoveMember', command);
    if (!result.success) {
      throw new Error(result.message ?? this._localization.translate('modules.profile.labUsers.errors.removeFailed'));
    }
  }

  async getMemberPermissions(memberId: string): Promise<LabUserPermissionDto[]> {
    const result = await this._http.get<LabUserPermissionDto[]>(
      'LabUser',
      'GetMemberPermissions',
      { memberId }
    );
    if (!result.success || !result.data) {
      throw new Error(
        result.message ??
          this._localization.translate(
            'modules.profile.labUsers.permissions.errors.loadFailed'
          )
      );
    }
    return result.data;
  }

  async setMemberPermissions(command: SetLabMemberPermissionsCommand): Promise<void> {
    const result = await this._http.post('LabUser', 'SetMemberPermissions', command);
    if (!result.success) {
      throw new Error(
        result.message ??
          this._localization.translate(
            'modules.profile.labUsers.permissions.errors.saveFailed'
          )
      );
    }
  }
}
