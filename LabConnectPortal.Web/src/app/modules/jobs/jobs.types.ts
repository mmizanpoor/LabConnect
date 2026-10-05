import { ContractType, DegreeLevel } from '../profile/resume/resume.types';
import { EmployeeCountRange } from '../profile/center-profile/center-profile.types';

export type GenderRequirement = 'Male' | 'Female' | 'NoPreference';
export type JobMilitaryRequirement = 'Exempted' | 'Completed' | 'NotImportant';
export type { ContractType, DegreeLevel };

export interface PublicJobPostingCardDto {
  jobPostingId: string;
  title: string;
  organizationName: string;
  provinceName: string;
  locationName: string;
  contractTypes: ContractType[];
  salaryRangeName: string;
  publishedAt?: string | null;
  hasOrganizationLogo: boolean;
}

export interface JobPostingOrgSummaryDto {
  name: string;
  establishedYear?: number | null;
  description: string;
  website?: string | null;
  employeeCount?: EmployeeCountRange | null;
  hasLogo: boolean;
}

export interface PublicJobPostingDetailDto {
  jobPostingId: string;
  jobCategoryName: string;
  locationName: string;
  provinceName: string;
  salaryRangeName: string;
  minimumWorkExperienceYears: number;
  jobDescription: string;
  contractTypes: ContractType[];
  requiredPersonalTraits: string[];
  essentialSkillNames: string[];
  benefits: string[];
  genderRequirement: GenderRequirement;
  militaryServiceRequirement: JobMilitaryRequirement;
  minimumDegreeLevel: DegreeLevel;
  additionalNotes: string;
  publishedAt?: string | null;
  organizationSummary?: JobPostingOrgSummaryDto | null;
}

export interface GetActivePostingsQuery {
  page?: number;
  pageSize?: number;
  search?: string;
  provinceIds?: number[];
  salaryRangeIds?: number[];
  contractTypes?: ContractType[];
  genderRequirements?: GenderRequirement[];
  minimumDegreeLevels?: DegreeLevel[];
}

export interface PublicJobProvinceFilterDto {
  provinceId: number;
  name: string;
}

export interface PublicJobSalaryFilterDto {
  salaryRangeId: number;
  name: string;
}

export interface PublicJobPostingFiltersDto {
  provinces: PublicJobProvinceFilterDto[];
  salaryRanges: PublicJobSalaryFilterDto[];
  contractTypes: ContractType[];
  genderRequirements: GenderRequirement[];
  minimumDegreeLevels: DegreeLevel[];
}

export interface PagedPublicJobs {
  items: PublicJobPostingCardDto[];
  totalCount: number;
  page: number;
  pageSize: number;
}

export type JobApplicationStatus = 'Pending' | 'Approved' | 'Rejected';

export interface MyJobApplicationListItemDto {
  jobApplicationId: string;
  jobPostingId: string;
  jobTitle: string;
  organizationName: string;
  status: JobApplicationStatus;
  submittedAt: string;
  reviewedAt?: string | null;
}

export interface JobApplicationDto {
  jobApplicationId: string;
  jobPostingId: string;
  jobTitle: string;
  organizationName: string;
  applicantFirstName: string;
  applicantLastName: string;
  applicantMobileNumber: string;
  status: JobApplicationStatus;
  submittedAt: string;
  reviewedAt?: string | null;
  reviewNotes: string;
  visitScheduledAt?: string | null;
  visitLocation?: string | null;
}

export interface PagedJobApplications {
  items: JobApplicationDto[];
  totalCount: number;
  page: number;
  pageSize: number;
}

export interface ApproveJobApplicationCommand {
  jobApplicationId: string;
  reviewNotes: string;
  visitScheduledAt?: string | null;
  visitLocation?: string | null;
}

export interface RejectJobApplicationCommand {
  jobApplicationId: string;
  reviewNotes: string;
}

export interface GetApplicationsForPostingQuery {
  jobPostingId: string;
  page?: number;
  pageSize?: number;
}
