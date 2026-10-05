export interface TokenResult {
  accessToken: string;
  refreshToken: string;
  expiresAt: string;
}

export enum LoginUserType {
  User = 0,
  UserLab = 1,
  Store = 2,
  AdminLab = 3,
}

export interface SendOtpCommand {
  mobileNumber: string;
  loginUserType: LoginUserType;
  labCode?: number | null;
  labCodeNew?: number | null;
}

export interface LoginApprovedLaboratoryCommand {
  labCodeNew: number;
}

export interface EnsurePortalLaboratoryCommand {
  labCode: number;
  labCodeNew: number;
}

export interface VerifyOtpCommand {
  mobileNumber: string;
  code: string;
  loginUserType: LoginUserType;
}

export interface LoginWithPasswordCommand {
  username: string;
  password: string;
}

export interface RegisterStoreCommand {
  username: string;
  mobileNumber: string;
  password: string;
  storeName: string;
}

export interface ProfileDto {
  username: string;
  firstName: string;
  lastName: string;
  mobileNumber: string;
  email: string;
  emailConfirmed?: boolean;
  address: string;
  phone: string;
  latitude?: number | null;
  longitude?: number | null;
  userType: LoginUserType | string;
  centerProfileId?: string | null;
  labCode?: number | null;
  labCodeNew?: number | null;
}

export interface UpdateProfileCommand {
  firstName: string;
  lastName: string;
  email: string;
  address: string;
  phone: string;
  latitude?: number | null;
  longitude?: number | null;
}

export interface UpdateUsernameCommand {
  username: string;
  code?: string | null;
  channel?: 'Email' | 'Mobile' | null;
}

export interface SendUsernameChangeOtpCommand {
  channel: 'Email' | 'Mobile';
  username: string;
}

export interface CheckUsernameAvailableCommand {
  username: string;
}

export interface SendEmailOtpCommand {
  email: string;
}

export interface ConfirmEmailOtpCommand {
  email: string;
  code: string;
}

export interface ChangePasswordCommand {
  currentPassword: string;
  newPassword: string;
  code?: string | null;
  channel?: 'Email' | 'Mobile' | null;
}

export interface SendPasswordChangeOtpCommand {
  channel: 'Email' | 'Mobile';
}

export interface RefreshTokenCommand {
  refreshToken: string;
}
