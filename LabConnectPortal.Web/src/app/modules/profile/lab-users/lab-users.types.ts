export interface LabMemberDto {
  id: string;
  userType: string;
  centerProfileId?: string | null;
  labCode?: number | null;
  labCodeNew?: number | null;
  mobileNumber: string;
  firstName: string;
  lastName: string;
  isActive: boolean;
  mobileConfirmed: boolean;
  createdAt: string;
}

export interface AddLabMemberCommand {
  mobileNumber: string;
}

export type AddLabMemberOtpScenario = 'user' | 'transfer' | 'create';

export interface AddLabMemberResultDto {
  requiresOtp: boolean;
  otpScenario?: AddLabMemberOtpScenario;
  member?: LabMemberDto;
}

export interface ConfirmAddLabMemberCommand {
  mobileNumber: string;
  code: string;
}

export interface ToggleLabMemberActiveCommand {
  memberId: string;
  isActive: boolean;
}

export interface RemoveLabMemberCommand {
  memberId: string;
}
