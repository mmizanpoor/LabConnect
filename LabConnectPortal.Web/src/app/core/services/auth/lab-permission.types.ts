export type LabPermissionAction = 'view' | 'create' | 'update' | 'delete';

export interface LabUserPermissionDto {
  systemEntityId: string;
  canView: boolean;
  canCreate: boolean;
  canUpdate: boolean;
  canDelete: boolean;
}

export interface SetLabMemberPermissionsCommand {
  memberId: string;
  permissions: LabUserPermissionDto[];
}
