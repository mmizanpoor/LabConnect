import { Injectable } from '@angular/core';
import { ApiHttpService } from '@core/services/http/api-http.service';
import { ResumeDto } from '../resume/resume.types';
import {
  GetProductResumeApplicationsQuery,
  ProductResumeApplicationDto,
  ProductResumeApplicationForOwnerDto,
  ProductResumeApplicationsPageDto,
  ReviewProductResumeApplicationCommand,
} from '../../products/products.types';

@Injectable({ providedIn: 'root' })
export class ProductResumeApplicationsService {
  constructor(private _http: ApiHttpService) {}

  getForProduct(query: GetProductResumeApplicationsQuery) {
    return this._http.post<ProductResumeApplicationsPageDto>(
      'ProductResumeApplication',
      'GetForProduct',
      query,
    );
  }

  getApplicantResume(productResumeApplicationId: string) {
    return this._http.get<ResumeDto>('ProductResumeApplication', 'GetApplicantResume', {
      productResumeApplicationId,
    });
  }

  getApplicantResumePhoto(productResumeApplicationId: string) {
    return this._http.getBlob('ProductResumeApplication', 'GetApplicantResumePhoto', {
      productResumeApplicationId,
    });
  }

  getApplicantResumeFile(productResumeApplicationId: string) {
    return this._http.getBlob('ProductResumeApplication', 'GetApplicantResumeFile', {
      productResumeApplicationId,
    });
  }

  getMyApplications() {
    return this._http.get<ProductResumeApplicationDto[]>(
      'ProductResumeApplication',
      'GetMyApplications',
    );
  }

  review(command: ReviewProductResumeApplicationCommand) {
    return this._http.post<ProductResumeApplicationForOwnerDto>(
      'ProductResumeApplication',
      'Review',
      command,
    );
  }
}
