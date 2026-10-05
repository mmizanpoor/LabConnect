export interface ActivityLogChangeDto {
  id: number;
  fieldName: string;
  oldValue: string | null;
  newValue: string | null;
}

export interface ActivityLogBatchDto {
  batchId: string;
  userId: string | null;
  userDisplayName: string | null;
  userType: string;
  action: string;
  entityName: string;
  recordKey: string;
  recordTitle: string | null;
  centerProfileId: string | null;
  centerName: string | null;
  ipAddress: string | null;
  createdAt: string;
  changes: ActivityLogChangeDto[];
}

export interface ActivityLogRecordGroupDto {
  entityName: string;
  recordKey: string;
  recordTitle: string | null;
  centerProfileId: string | null;
  centerName: string | null;
  latestAt: string;
  batchCount: number;
  batches: ActivityLogBatchDto[];
}

export interface ActivityLogUserOptionDto {
  id: string;
  displayName: string;
  mobileNumber?: string | null;
}

export interface GetActivityLogsQuery {
  entityName?: string;
  recordKey?: string;
  centerProfileId?: string;
  userId?: string;
  action?: string;
  from?: string;
  to?: string;
  page?: number;
  pageSize?: number;
}

export interface GetActivityLogsByRecordQuery {
  entityName: string;
  recordKey: string;
  page?: number;
  pageSize?: number;
}

export interface PagedResult<T> {
  items: T[];
  totalCount: number;
  page: number;
  pageSize: number;
}
