import { Injectable } from '@angular/core';
import { ApiHttpService } from '@core/services/http/api-http.service';
import {
  TestInfoDetailDto,
  TestInfoListItemDto,
  UpdateTestInfoApprovePriceCommand,
  UpdateTestInfoCommand,
} from './test-infos.types';

@Injectable({ providedIn: 'root' })
export class TestInfosService {
  constructor(private _http: ApiHttpService) {}

  getAll() {
    return this._http.get<TestInfoListItemDto[]>('TestInfo', 'GetAll');
  }

  getById(id: number) {
    return this._http.get<TestInfoDetailDto>('TestInfo', 'GetById', { id });
  }

  update(command: UpdateTestInfoCommand) {
    return this._http.post<TestInfoDetailDto>('TestInfo', 'Update', command);
  }

  updateApprovePrice(command: UpdateTestInfoApprovePriceCommand) {
    return this._http.post<TestInfoDetailDto>('TestInfo', 'UpdateApprovePrice', command);
  }
}
