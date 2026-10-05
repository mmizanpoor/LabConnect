import { Injectable } from '@angular/core';
import { ApiHttpService } from '@core/services/http/api-http.service';
import {
  ActivityLogBatchDto,
  ActivityLogRecordGroupDto,
  ActivityLogUserOptionDto,
  GetActivityLogsByRecordQuery,
  GetActivityLogsQuery,
  PagedResult,
} from './activity-log.types';

@Injectable({ providedIn: 'root' })
export class ActivityLogService {
  constructor(private _http: ApiHttpService) {}

  getAll(query: GetActivityLogsQuery) {
    return this._http.post<PagedResult<ActivityLogRecordGroupDto>>('ActivityLog', 'GetAll', query);
  }

  getByRecord(query: GetActivityLogsByRecordQuery) {
    return this._http.post<PagedResult<ActivityLogBatchDto>>('ActivityLog', 'GetByRecord', query);
  }

  getBatch(batchId: string) {
    return this._http.get<ActivityLogBatchDto>('ActivityLog', 'GetBatch', { batchId });
  }

  getCenterUsers() {
    return this._http.get<ActivityLogUserOptionDto[]>('ActivityLog', 'GetCenterUsers');
  }
}
