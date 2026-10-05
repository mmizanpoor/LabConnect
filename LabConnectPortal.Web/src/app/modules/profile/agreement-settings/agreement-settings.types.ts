export type LabAgreementSettingsFileKind = 'HeaderImage' | 'HeaderLogo';

export interface LabAgreementSettingsDto {
  id?: string | null;
  centerProfileId: string;
  useHeaderImage: boolean;
  headerImagePath?: string | null;
  labName: string;
  headerAddress: string;
  description1: string;
  headerLogoPath?: string | null;
  updatedAt?: string | null;
  hasHeaderImage: boolean;
  hasHeaderLogo: boolean;
}

export interface UpsertLabAgreementSettingsCommand {
  useHeaderImage: boolean;
  labName?: string;
  headerAddress?: string;
  description1?: string;
}
