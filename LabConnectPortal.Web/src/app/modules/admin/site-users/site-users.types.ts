export interface SiteMemberDto {
  id: string;
  userType: string;
  mobileNumber: string;
  firstName: string;
  lastName: string;
  isActive: boolean;
  mobileConfirmed: boolean;
  createdAt: string;
}

export interface AddSiteMemberCommand {
  mobileNumber: string;
}

export type AddSiteMemberOtpScenario = 'user' | 'create';

export interface AddSiteMemberResultDto {
  requiresOtp: boolean;
  otpScenario?: AddSiteMemberOtpScenario;
  member?: SiteMemberDto;
}

export interface ConfirmAddSiteMemberCommand {
  mobileNumber: string;
  code: string;
}

export interface ToggleSiteMemberActiveCommand {
  memberId: string;
  isActive: boolean;
}

export interface RemoveSiteMemberCommand {
  memberId: string;
}
