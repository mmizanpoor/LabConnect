export type SitePermissionAction = 'view' | 'create' | 'update' | 'delete';

export interface SiteUserPermissionDto {
  systemEntityId: string;
  canView: boolean;
  canCreate: boolean;
  canUpdate: boolean;
  canDelete: boolean;
}

export interface SetSiteMemberPermissionsCommand {
  memberId: string;
  permissions: SiteUserPermissionDto[];
}
