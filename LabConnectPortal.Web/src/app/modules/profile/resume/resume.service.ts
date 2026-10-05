import { Injectable } from '@angular/core';
import { ApiHttpService } from '@core/services/http/api-http.service';
import { OperationResult } from '@core/types/operation-result.types';
import {
  ReferenceData,
  ReferenceItemDto,
  ResumeDto,
  SaveEducationsCommand,
  SaveLanguagesCommand,
  SaveSkillsCommand,
  SaveWorkExperiencesCommand,
  SkillSearchResultDto,
  UpdateAboutMeCommand,
  UpdateBasicInfoCommand,
  UpdateJobPreferenceCommand,
  UpdatePersonalInfoCommand,
  normalizeResume,
} from './resume.types';

@Injectable({ providedIn: 'root' })
export class ResumeService {
  constructor(private _http: ApiHttpService) {}

  getMyResume() {
    return this.unwrapResume(this._http.get<ResumeDto>('UserProfile', 'GetMyResume'));
  }

  updateBasicInfo(command: UpdateBasicInfoCommand) {
    return this.unwrapResume(this._http.post<ResumeDto>('UserProfile', 'UpdateBasicInfo', command));
  }

  updateAboutMe(command: UpdateAboutMeCommand) {
    return this.unwrapResume(this._http.post<ResumeDto>('UserProfile', 'UpdateAboutMe', command));
  }

  updatePersonalInfo(command: UpdatePersonalInfoCommand) {
    return this.unwrapResume(this._http.post<ResumeDto>('UserProfile', 'UpdatePersonalInfo', command));
  }

  updateJobPreference(command: UpdateJobPreferenceCommand) {
    return this.unwrapResume(this._http.post<ResumeDto>('UserProfile', 'UpdateJobPreference', command));
  }

  saveWorkExperiences(command: SaveWorkExperiencesCommand) {
    return this.unwrapResume(this._http.post<ResumeDto>('UserProfile', 'SaveWorkExperiences', command));
  }

  saveEducations(command: SaveEducationsCommand) {
    return this.unwrapResume(this._http.post<ResumeDto>('UserProfile', 'SaveEducations', command));
  }

  saveSkills(command: SaveSkillsCommand) {
    return this.unwrapResume(this._http.post<ResumeDto>('UserProfile', 'SaveSkills', command));
  }

  saveLanguages(command: SaveLanguagesCommand) {
    return this.unwrapResume(this._http.post<ResumeDto>('UserProfile', 'SaveLanguages', command));
  }

  uploadPhoto(file: File) {
    const formData = new FormData();
    formData.append('file', file);
    return this.unwrapResume(this._http.postForm<ResumeDto>('UserProfile', 'UploadPhoto', formData));
  }

  uploadResume(file: File) {
    const formData = new FormData();
    formData.append('file', file);
    return this.unwrapResume(this._http.postForm<ResumeDto>('UserProfile', 'UploadResume', formData));
  }

  deleteResume() {
    return this.unwrapResume(this._http.post<ResumeDto>('UserProfile', 'DeleteResume', {}));
  }

  getPhotoBlob() {
    return this._http.getBlob('UserProfile', 'GetPhoto');
  }

  getResumeBlob() {
    return this._http.getBlob('UserProfile', 'GetResume');
  }

  getProvinces() {
    return this.unwrap(this._http.get<ReferenceItemDto[]>('UserProfile', 'GetProvinces'));
  }

  getJobCategories() {
    return this.unwrap(this._http.get<ReferenceItemDto[]>('UserProfile', 'GetJobCategories'));
  }

  getSalaryRanges() {
    return this.unwrap(this._http.get<ReferenceItemDto[]>('UserProfile', 'GetSalaryRanges'));
  }

  getLanguageNames() {
    return this.unwrap(this._http.get<ReferenceItemDto[]>('UserProfile', 'GetLanguageNames'));
  }

  searchSkills(term: string) {
    return this.unwrap(this._http.get<SkillSearchResultDto[]>('UserProfile', 'SearchSkills', { term }));
  }

  createSkill(skillName: string) {
    return this.unwrap(
      this._http.post<SkillSearchResultDto>('UserProfile', 'CreateSkill', { skillName }),
    );
  }

  async loadReferenceData(): Promise<ReferenceData> {
    const [provinces, jobCategories, salaryRanges, languageNames] = await Promise.all([
      this.getProvinces(),
      this.getJobCategories(),
      this.getSalaryRanges(),
      this.getLanguageNames(),
    ]);
    return { provinces, jobCategories, salaryRanges, languageNames };
  }

  private async unwrapResume(promise: Promise<OperationResult<ResumeDto>>): Promise<ResumeDto> {
    return normalizeResume(await this.unwrap(promise));
  }

  private async unwrap<T>(promise: Promise<OperationResult<T>>): Promise<T> {
    const result = await promise;
    if (!result.success) {
      throw new Error(result.message ?? 'خطا در انجام عملیات');
    }
    return result.data as T;
  }
}
