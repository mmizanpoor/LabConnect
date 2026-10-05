import { Injectable } from '@angular/core';
import { ApiHttpService } from '@core/services/http/api-http.service';
import {
  LabAgreementSettingsDto,
  LabAgreementSettingsFileKind,
  UpsertLabAgreementSettingsCommand,
} from './agreement-settings.types';

@Injectable({ providedIn: 'root' })
export class AgreementSettingsService {
  constructor(private _http: ApiHttpService) {}

  getMine() {
    return this._http.get<LabAgreementSettingsDto>('LabAgreementSettings', 'GetMine');
  }

  upsert(command: UpsertLabAgreementSettingsCommand) {
    return this._http.post<LabAgreementSettingsDto>('LabAgreementSettings', 'Update', command);
  }

  uploadHeaderImage(file: File) {
    return this.uploadFile('UploadHeaderImage', file);
  }

  uploadHeaderLogo(file: File) {
    return this.uploadFile('UploadHeaderLogo', file);
  }

  getFileBlob(kind: LabAgreementSettingsFileKind) {
    return this._http.getBlob('LabAgreementSettings', 'GetFile', { kind });
  }

  private uploadFile(action: string, file: File) {
    const formData = new FormData();
    formData.append('file', file);
    return this._http.postForm<LabAgreementSettingsDto>('LabAgreementSettings', action, formData);
  }
}
