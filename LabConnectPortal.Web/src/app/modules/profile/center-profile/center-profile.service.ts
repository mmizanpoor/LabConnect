import { Injectable } from '@angular/core';
import { ApiHttpService } from '@core/services/http/api-http.service';
import {
  CenterProfileDto,
  CenterProfileFileKind,
  InitPayChargeCommand,
  SendCenterContactChangeOtpCommand,
  SmsChargeInfoDto,
  UpdateCenterProfileCommand,
} from './center-profile.types';
import { InitPayResult } from '../sms-charge/sms-charge.types';

@Injectable({ providedIn: 'root' })
export class CenterProfileService {
  constructor(private _http: ApiHttpService) {}

  getMyProfile() {
    return this._http.get<CenterProfileDto>('CenterProfile', 'GetMyProfile');
  }

  updateMyProfile(command: UpdateCenterProfileCommand) {
    return this._http.post<CenterProfileDto>(
      'CenterProfile',
      'UpdateMyProfile',
      command,
    );
  }

  getSmsChargeInfo() {
    return this._http.get<SmsChargeInfoDto>(
      'CenterProfile',
      'GetSmsChargeInfo',
    );
  }

  initPayCharge(command: InitPayChargeCommand) {
    return this._http.post<InitPayResult>(
      'CenterProfile',
      'InitPayCharge',
      command,
    );
  }

  sendContactChangeOtp(command: SendCenterContactChangeOtpCommand) {
    return this._http.post('CenterProfile', 'SendContactChangeOtp', command);
  }

  uploadLogo(file: File) {
    return this.uploadFile('UploadLogo', file);
  }

  uploadNationalCard(file: File) {
    return this.uploadFile('UploadNationalCard', file);
  }

  uploadLicense(file: File) {
    return this.uploadFile('UploadLicense', file);
  }

  uploadOfficialImage(file: File) {
    return this.uploadFile('UploadOfficialImage', file);
  }

  uploadTradeCard(file: File) {
    return this.uploadFile('UploadTradeCard', file);
  }

  deleteFile(kind: CenterProfileFileKind) {
    return this._http.post<CenterProfileDto>('CenterProfile', 'DeleteFile', {
      kind,
    });
  }

  getFileBlob(profileId: string, kind: CenterProfileFileKind) {
    return this._http.getBlob('CenterProfile', 'GetFile', { profileId, kind });
  }

  private uploadFile(action: string, file: File) {
    const formData = new FormData();
    formData.append('file', file);
    return this._http.postForm<CenterProfileDto>(
      'CenterProfile',
      action,
      formData,
    );
  }
}
