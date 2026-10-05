export enum CompanyRegulationType {
  RulesAndRegulations = 1,
  CompanyPolicy = 2,
}

export type CompanyRegulationTypeValue =
  | 'RulesAndRegulations'
  | 'CompanyPolicy'
  | CompanyRegulationType;

export interface CompanyRegulationDto {
  companyRegulationId: string;
  type: CompanyRegulationTypeValue;
  body: string;
  createdAt: string;
  modifiedAt: string;
}

export interface SaveCompanyRegulationCommand {
  type: CompanyRegulationType;
  body: string;
}

export interface UpdateCompanyRegulationCommand {
  companyRegulationId: string;
  body: string;
}

export const COMPANY_REGULATION_TYPES: readonly CompanyRegulationType[] = [
  CompanyRegulationType.RulesAndRegulations,
  CompanyRegulationType.CompanyPolicy,
];

export function normalizeCompanyRegulationType(
  type: CompanyRegulationTypeValue | string | number | null | undefined,
): CompanyRegulationType | null {
  if (type == null) {
    return null;
  }

  if (typeof type === 'number') {
    return COMPANY_REGULATION_TYPES.includes(type as CompanyRegulationType)
      ? (type as CompanyRegulationType)
      : null;
  }

  const normalized = String(type).trim();
  switch (normalized) {
    case 'RulesAndRegulations':
    case '1':
      return CompanyRegulationType.RulesAndRegulations;
    case 'CompanyPolicy':
    case '2':
      return CompanyRegulationType.CompanyPolicy;
    default:
      return null;
  }
}

export function companyRegulationTypeSortOrder(
  type: CompanyRegulationTypeValue | string | number | null | undefined,
): number {
  return normalizeCompanyRegulationType(type) ?? 999;
}
