import {
  Gender,
  RESUME_SECTION_IDS,
  ResumeDto,
  ResumeSectionId,
  coerceEnum,
} from './resume.types';

function hasText(value: string | null | undefined): boolean {
  return !!value?.trim();
}

export function isResumeSectionComplete(section: ResumeSectionId, resume: ResumeDto): boolean {
  switch (section) {
    case 'basicInfo':
      return (
        hasText(resume.firstName) &&
        hasText(resume.lastName) &&
        hasText(resume.basicInfo.jobTitle) &&
        resume.basicInfo.employmentStatus != null
      );
    case 'aboutMe':
      return hasText(resume.aboutMe);
    case 'personalInfo': {
      const info = resume.personalInfo;
      const core =
        hasText(info.email) &&
        hasText(info.mobilePhone) &&
        info.provinceId != null &&
        info.maritalStatus != null &&
        info.birthYear != null &&
        info.gender != null;
      if (!core) return false;
      if (coerceEnum(Gender, info.gender) === Gender.Male) {
        return info.militaryServiceStatus != null;
      }
      return true;
    }
    case 'skills':
      return resume.skills.length > 0;
    case 'workExperience':
      return resume.workExperiences.some((item) => hasText(item.jobTitle) && hasText(item.companyName));
    case 'education':
      return resume.educations.some((item) => hasText(item.fieldOfStudy) && hasText(item.institutionName));
    case 'languages':
      return resume.languages.some((item) => !!item.languageNameId && item.proficiencyLevel != null);
    case 'jobPreferences': {
      const pref = resume.jobPreference;
      return !!pref && (
        pref.preferredProvinceIds.length > 0 ||
        pref.jobCategoryIds.length > 0 ||
        pref.minimumSalaryId != null
      );
    }
    case 'resumeFile':
      return !!resume.resumeFile;
  }
}

export function getResumeProgress(resume: ResumeDto): {
  completed: number;
  total: number;
  percent: number;
} {
  const total = RESUME_SECTION_IDS.length;
  const completed = RESUME_SECTION_IDS.filter((section) =>
    isResumeSectionComplete(section, resume),
  ).length;
  return {
    completed,
    total,
    percent: total ? Math.round((completed / total) * 100) : 0,
  };
}
