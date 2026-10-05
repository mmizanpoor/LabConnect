import { ContractType, DegreeLevel } from '../resume/resume.types';
import { EmployeeCountRange } from '../center-profile/center-profile.types';

export type GenderRequirement = 'Male' | 'Female' | 'NoPreference';
export type JobMilitaryRequirement = 'Exempted' | 'Completed' | 'NotImportant';
export type JobPostingStatus = 'Draft' | 'Active' | 'Closed';

export interface JobPostingDashboardStats {
  activeCount: number;
  expiredCount: number;
  sentApplicationsCount: number;
}

export interface JobPostingOrgSummaryDto {
  name: string;
  establishedYear?: number | null;
  description: string;
  website?: string | null;
  employeeCount?: EmployeeCountRange | null;
  hasLogo: boolean;
}

export interface JobPostingDto {
  jobPostingId: string;
  jobCategoryId: number;
  jobCategoryName: string;
  locationId: string;
  locationName: string;
  salaryRangeId: number;
  salaryRangeName: string;
  minimumWorkExperienceYears: number;
  jobDescription: string;
  contractTypes: ContractType[];
  requiredPersonalTraits: string[];
  essentialSkillIds: number[];
  essentialSkillNames: string[];
  benefits: string[];
  genderRequirement: GenderRequirement;
  militaryServiceRequirement: JobMilitaryRequirement;
  minimumDegreeLevel: DegreeLevel;
  additionalNotes: string;
  status: JobPostingStatus;
  createdAt: string;
  updatedAt: string;
  publishedAt?: string | null;
  closedAt?: string | null;
  organizationSummary?: JobPostingOrgSummaryDto | null;
}

export interface JobPostingListItemDto {
  jobPostingId: string;
  jobCategoryName: string;
  locationName: string;
  status: JobPostingStatus;
  createdAt: string;
  publishedAt?: string | null;
}

export interface SaveJobPostingCommand {
  jobCategoryId: number;
  locationId: string;
  salaryRangeId: number;
  minimumWorkExperienceYears: number;
  jobDescription: string;
  contractTypes: ContractType[];
  requiredPersonalTraits: string[];
  essentialSkillIds: number[];
  benefits: string[];
  genderRequirement: GenderRequirement;
  militaryServiceRequirement: JobMilitaryRequirement;
  minimumDegreeLevel: DegreeLevel;
  additionalNotes: string;
}

export interface UpdateJobPostingCommand extends SaveJobPostingCommand {
  jobPostingId: string;
}

export interface GetMyJobPostingsQuery {
  status?: JobPostingStatus;
  page?: number;
  pageSize?: number;
}

export interface PagedJobPostings {
  items: JobPostingListItemDto[];
  totalCount: number;
  page: number;
  pageSize: number;
}

export const GENDER_REQUIREMENTS: GenderRequirement[] = ['Male', 'Female', 'NoPreference'];
export const JOB_MILITARY_REQUIREMENTS: JobMilitaryRequirement[] = ['Exempted', 'Completed', 'NotImportant'];
export const JOB_POSTING_STATUSES: JobPostingStatus[] = ['Draft', 'Active', 'Closed'];

export const SUGGESTED_PERSONAL_TRAITS = [
  'پشتکار بالا',
  'مهارت ارتباطی قوی',
  'کار تیمی',
  'دقت بالا',
  'مسئولیت‌پذیری',
];

export const SUGGESTED_BENEFITS = [
  'بیمه تکمیلی',
  'ناهار رایگان',
  'پاداش عملکرد',
  'ساعت کاری منعطف',
  'آموزش حرفه‌ای',
];
