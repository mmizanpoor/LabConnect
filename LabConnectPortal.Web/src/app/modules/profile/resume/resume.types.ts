export enum EmploymentStatus {
  JobSeeker = 0,
  LookingForBetter = 1,
  Employed = 2,
}

export enum MaritalStatus {
  Single = 0,
  Married = 1,
}

export enum Gender {
  Female = 0,
  Male = 1,
}

export enum MilitaryServiceStatus {
  Exempt = 0,
  Completed = 1,
  InProgress = 2,
  NotApplicable = 3,
}

export enum DegreeLevel {
  Diploma = 0,
  Associate = 1,
  Bachelor = 2,
  Master = 3,
  Doctorate = 4,
  Other = 5,
}

export enum PersianMonth {
  Farvardin = 1,
  Ordibehesht = 2,
  Khordad = 3,
  Tir = 4,
  Mordad = 5,
  Shahrivar = 6,
  Mehr = 7,
  Aban = 8,
  Azar = 9,
  Dey = 10,
  Bahman = 11,
  Esfand = 12,
}

export enum SkillProficiencyLevel {
  Beginner = 0,
  Intermediate = 1,
  Advanced = 2,
  Expert = 3,
}

export enum LanguageProficiencyLevel {
  Beginner = 0,
  Intermediate = 1,
  Advanced = 2,
  Native = 3,
}

export enum SeniorityLevel {
  EntryLevel = 0,
  Specialist = 1,
  Manager = 2,
  Executive = 3,
}

export enum ContractType {
  FullTime = 0,
  PartTime = 1,
  Remote = 2,
  Internship = 3,
}

export interface ResumeDto {
  firstName: string;
  lastName: string;
  mobileNumber: string;
  basicInfo: BasicInfoDto;
  aboutMe: string;
  personalInfo: PersonalInfoDto;
  skills: UserSkillDto[];
  workExperiences: WorkExperienceDto[];
  educations: EducationDto[];
  languages: UserLanguageDto[];
  jobPreference: JobPreferenceDto | null;
  resumeFile: ResumeFileDto | null;
  isCompleteForApplication?: boolean;
}

export interface BasicInfoDto {
  jobTitle: string;
  employmentStatus: EmploymentStatus | null;
  hasProfilePhoto: boolean;
  latestWorkSummary: string | null;
  latestEducationSummary: string | null;
}

export interface PersonalInfoDto {
  email: string;
  mobilePhone: string;
  provinceId: number | null;
  provinceName: string | null;
  address: string;
  maritalStatus: MaritalStatus | null;
  birthYear: number | null;
  gender: Gender | null;
  militaryServiceStatus: MilitaryServiceStatus | null;
}

export interface WorkExperienceDto {
  workExperienceId?: string | null;
  jobTitle: string;
  companyName: string;
  startMonth: PersianMonth | null;
  startYear: number | null;
  endMonth: PersianMonth | null;
  endYear: number | null;
  isCurrentlyEmployed: boolean;
  jobDescription: string;
}

export interface EducationDto {
  educationId?: string | null;
  fieldOfStudy: string;
  institutionName: string;
  degreeLevel: DegreeLevel | null;
  startYear: number | null;
  endYear: number | null;
  isCurrentlyStudying: boolean;
  description: string;
}

export interface UserSkillDto {
  userSkillId?: string | null;
  skillId: number;
  skillName: string;
  proficiencyLevel: SkillProficiencyLevel | null;
}

export interface UserLanguageDto {
  userLanguageId?: string | null;
  languageNameId: number;
  languageName: string;
  proficiencyLevel: LanguageProficiencyLevel | null;
}

export interface JobPreferenceDto {
  preferredProvinceIds: number[];
  jobCategoryIds: number[];
  seniorityLevels: SeniorityLevel[];
  acceptableContractTypes: ContractType[];
  minimumSalaryId: number | null;
}

export interface ResumeFileDto {
  fileName: string;
  fileSize: number;
  uploadedAt: string | null;
}

export interface ReferenceItemDto {
  id: number;
  name: string;
}

export interface SkillSearchResultDto {
  skillId: number;
  skillName: string;
}

export interface UpdateBasicInfoCommand {
  firstName: string;
  lastName: string;
  jobTitle: string;
  employmentStatus: EmploymentStatus | null;
}

export interface UpdateAboutMeCommand {
  aboutMe: string;
}

export interface UpdatePersonalInfoCommand {
  email: string;
  mobilePhone: string;
  provinceId: number | null;
  address: string;
  maritalStatus: MaritalStatus | null;
  birthYear: number | null;
  gender: Gender | null;
  militaryServiceStatus: MilitaryServiceStatus | null;
}

export interface UpdateJobPreferenceCommand {
  preferredProvinceIds: number[];
  jobCategoryIds: number[];
  seniorityLevels: SeniorityLevel[];
  acceptableContractTypes: ContractType[];
  minimumSalaryId: number | null;
}

export interface ReferenceData {
  provinces: ReferenceItemDto[];
  jobCategories: ReferenceItemDto[];
  salaryRanges: ReferenceItemDto[];
  languageNames: ReferenceItemDto[];
}

export interface SaveWorkExperiencesCommand {
  items: WorkExperienceDto[];
}

export interface SaveEducationsCommand {
  items: EducationDto[];
}

export interface SaveSkillsCommand {
  items: UserSkillDto[];
}

export interface SaveLanguagesCommand {
  items: UserLanguageDto[];
}

export interface CreateSkillCommand {
  skillName: string;
}

export const PERSIAN_MONTHS = [
  PersianMonth.Farvardin,
  PersianMonth.Ordibehesht,
  PersianMonth.Khordad,
  PersianMonth.Tir,
  PersianMonth.Mordad,
  PersianMonth.Shahrivar,
  PersianMonth.Mehr,
  PersianMonth.Aban,
  PersianMonth.Azar,
  PersianMonth.Dey,
  PersianMonth.Bahman,
  PersianMonth.Esfand,
];

export const DEGREE_LEVELS = [
  DegreeLevel.Diploma,
  DegreeLevel.Associate,
  DegreeLevel.Bachelor,
  DegreeLevel.Master,
  DegreeLevel.Doctorate,
  DegreeLevel.Other,
];

export const EMPLOYMENT_STATUSES = [
  EmploymentStatus.Employed,
  EmploymentStatus.LookingForBetter,
  EmploymentStatus.JobSeeker,
];

export const SENIORITY_LEVELS = [
  SeniorityLevel.EntryLevel,
  SeniorityLevel.Specialist,
  SeniorityLevel.Manager,
  SeniorityLevel.Executive,
];

export const CONTRACT_TYPES = [
  ContractType.FullTime,
  ContractType.PartTime,
  ContractType.Remote,
  ContractType.Internship,
];

export const LANGUAGE_PROFICIENCY_LEVELS = [
  LanguageProficiencyLevel.Beginner,
  LanguageProficiencyLevel.Intermediate,
  LanguageProficiencyLevel.Advanced,
  LanguageProficiencyLevel.Native,
];

export const MILITARY_STATUSES = [
  MilitaryServiceStatus.Exempt,
  MilitaryServiceStatus.Completed,
  MilitaryServiceStatus.InProgress,
];

export const MARITAL_STATUSES = [MaritalStatus.Single, MaritalStatus.Married];
export const GENDERS = [Gender.Female, Gender.Male];

/** Resolves numeric or string enum values to the enum member name for i18n keys. */
export function getEnumName<T extends Record<string, string | number>>(
  enumObj: T,
  value: number | string | null | undefined,
): string {
  if (value === null || value === undefined || value === '') return '';

  if (typeof value === 'string' && Number.isNaN(Number(value))) {
    return value;
  }

  const numeric =
    typeof value === 'number'
      ? value
      : typeof enumObj[value as keyof T] === 'number'
        ? (enumObj[value as keyof T] as number)
        : Number(value);

  const name = enumObj[numeric as keyof T];
  return typeof name === 'string' ? name : String(value);
}

export function enumTranslateKey(
  group: string,
  enumObj: Record<string, string | number>,
  value: number | string | null | undefined,
): string {
  const name = getEnumName(enumObj, value);
  return name ? `modules.profile.resume.enums.${group}.${name}` : '';
}

export function coerceEnum<T extends Record<string, string | number>>(
  enumObj: T,
  value: number | string | null | undefined,
): Extract<T[keyof T], number> | null {
  const name = getEnumName(enumObj, value);
  if (!name) return null;
  const numeric = enumObj[name as keyof T];
  return typeof numeric === 'number' ? (numeric as Extract<T[keyof T], number>) : null;
}

export function coerceEnumArray<T extends Record<string, string | number>>(
  enumObj: T,
  values: Array<number | string> | null | undefined,
): Array<Extract<T[keyof T], number>> {
  return (values ?? [])
    .map((value) => coerceEnum(enumObj, value))
    .filter((value): value is Extract<T[keyof T], number> => value != null);
}

export function resumeEnumOptions(
  translate: (key: string) => string,
  group: string,
  enumObj: Record<string, string | number>,
  values: number[],
): { value: number; label: string }[] {
  return values.map((value) => ({
    value,
    label: translate(enumTranslateKey(group, enumObj, value)),
  }));
}

export function normalizeResume(resume: ResumeDto): ResumeDto {
  return {
    ...resume,
    basicInfo: {
      ...resume.basicInfo,
      employmentStatus: coerceEnum(EmploymentStatus, resume.basicInfo.employmentStatus),
    },
    personalInfo: {
      ...resume.personalInfo,
      maritalStatus: coerceEnum(MaritalStatus, resume.personalInfo.maritalStatus),
      gender: coerceEnum(Gender, resume.personalInfo.gender),
      militaryServiceStatus: coerceEnum(
        MilitaryServiceStatus,
        resume.personalInfo.militaryServiceStatus,
      ),
    },
    workExperiences: resume.workExperiences.map((item) => ({
      ...item,
      startMonth: coerceEnum(PersianMonth, item.startMonth),
      endMonth: coerceEnum(PersianMonth, item.endMonth),
    })),
    educations: resume.educations.map((item) => ({
      ...item,
      degreeLevel: coerceEnum(DegreeLevel, item.degreeLevel),
    })),
    skills: resume.skills.map((item) => ({
      ...item,
      proficiencyLevel: coerceEnum(SkillProficiencyLevel, item.proficiencyLevel),
    })),
    languages: resume.languages.map((item) => ({
      ...item,
      proficiencyLevel: coerceEnum(LanguageProficiencyLevel, item.proficiencyLevel),
    })),
    jobPreference: resume.jobPreference
      ? {
          ...resume.jobPreference,
          seniorityLevels: coerceEnumArray(SeniorityLevel, resume.jobPreference.seniorityLevels),
          acceptableContractTypes: coerceEnumArray(
            ContractType,
            resume.jobPreference.acceptableContractTypes,
          ),
        }
      : null,
  };
}

export type ResumeSectionAnchor = 'work-experience' | 'education';
export type ResumeSectionId =
  | 'basicInfo'
  | 'aboutMe'
  | 'personalInfo'
  | 'skills'
  | 'workExperience'
  | 'education'
  | 'languages'
  | 'jobPreferences'
  | 'resumeFile';

export const RESUME_SECTION_IDS: ResumeSectionId[] = [
  'basicInfo',
  'aboutMe',
  'personalInfo',
  'skills',
  'workExperience',
  'education',
  'languages',
  'jobPreferences',
  'resumeFile',
];
