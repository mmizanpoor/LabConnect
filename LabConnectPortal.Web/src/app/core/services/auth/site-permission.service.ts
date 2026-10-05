import { Injectable } from '@angular/core';
import { ApiHttpService } from '@core/services/http/api-http.service';
import { SystemEntity } from '@core/system-entity/system-entity';
import { AuthUtils } from './auth.utils';
import {
  SitePermissionAction,
  SiteUserPermissionDto,
} from './site-permission.types';

@Injectable({ providedIn: 'root' })
export class SitePermissionService {
  private readonly _permissions = new Map<string, SiteUserPermissionDto>();
  private _loaded = false;

  constructor(
    private _http: ApiHttpService,
    private _authUtils: AuthUtils
  ) {}

  get isLoaded(): boolean {
    return this._loaded;
  }

  async load(): Promise<void> {
    this._permissions.clear();
    this._loaded = false;

    if (!this._authUtils.isAdmin()) {
      this._loaded = true;
      return;
    }

    const result = await this._http.get<SiteUserPermissionDto[]>(
      'SiteUser',
      'GetMyPermissions'
    );
    if (result.success && result.data) {
      for (const permission of result.data) {
        this._permissions.set(
          permission.systemEntityId.toLowerCase(),
          permission
        );
      }
    }
    this._loaded = true;
  }

  clear(): void {
    this._permissions.clear();
    this._loaded = false;
  }

  can(entity: string, action: SitePermissionAction): boolean {
    if (this._authUtils.isAdministrator()) {
      return true;
    }

    if (!this._authUtils.isAdmin()) {
      return false;
    }

    const permission = this._permissions.get(entity.toLowerCase());
    if (!permission) {
      return false;
    }

    switch (action) {
      case 'view':
        return permission.canView;
      case 'create':
        return permission.canCreate;
      case 'update':
        return permission.canUpdate;
      case 'delete':
        return permission.canDelete;
      default:
        return false;
    }
  }

  canView(entity: string): boolean {
    return this.can(entity, 'view');
  }

  canExport(entity: string): boolean {
    return this.canView(entity);
  }

  hasAnySiteAdminAccess(): boolean {
    if (this._authUtils.isAdministrator()) {
      return true;
    }
    if (!this._authUtils.isAdmin()) {
      return false;
    }
    return SystemEntity.SiteAdminAll.some((entity) => this.can(entity, 'view'));
  }
}
