import { Injectable } from '@angular/core';
import { JwtHelperService } from '@auth0/angular-jwt';
import { TokenService } from '../token/token.service';

@Injectable({ providedIn: 'root' })
export class AuthUtils {
  private readonly _jwt = new JwtHelperService();

  constructor(private _tokenService: TokenService) {}

  get isAuthenticated(): boolean {
    const token = this._tokenService.accessToken;
    return !!token && !this._jwt.isTokenExpired(token);
  }

  get username(): string {
    const token = this._tokenService.accessToken;
    if (!token) return '';
    const decoded = this._jwt.decodeToken(token);
    return (
      (decoded['unique_name'] as string) ??
      (decoded[
        'http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name'
      ] as string) ??
      (decoded['name'] as string) ??
      ''
    );
  }

  get mobileNumber(): string {
    const token = this._tokenService.accessToken;
    if (!token) return '';
    const decoded = this._jwt.decodeToken(token);
    return (
      (decoded[
        'http://schemas.xmlsoap.org/ws/2005/05/identity/claims/mobilephone'
      ] as string) ??
      (decoded['mobilephone'] as string) ??
      (decoded['MobilePhone'] as string) ??
      ''
    );
  }

  get displayIdentity(): string {
    const username = this.username.trim();
    if (username) return username;
    return this.mobileNumber.trim();
  }

  get userType(): string {
    const token = this._tokenService.accessToken;
    if (!token) return '';
    const decoded = this._jwt.decodeToken(token) as Record<string, unknown> | null;
    if (!decoded) return '';
    const raw =
      decoded['userType'] ??
      decoded['UserType'] ??
      decoded['http://schemas.microsoft.com/ws/2008/06/identity/claims/role'];
    if (raw === undefined || raw === null) return '';
    return Array.isArray(raw) ? String(raw[0] ?? '').trim() : String(raw).trim();
  }

  isAdministrator(): boolean {
    return this.userType === 'Administrator' || this.userType === '4';
  }

  isAdmin(): boolean {
    return this.userType === 'Admin' || this.userType === '5';
  }

  /** Full site owner or limited site staff with SiteUserPermissions. */
  canAccessAdminPortal(): boolean {
    return this.isAdministrator() || this.isAdmin();
  }

  isAdminLab(): boolean {
    return this.userType === 'AdminLab' || this.userType === '3';
  }

  isStore(): boolean {
    const value = this.userType.toLowerCase();
    return value === 'store' || value === '2';
  }

  isUser(): boolean {
    return this.userType === 'User' || this.userType === '0';
  }

  isUserLab(): boolean {
    return this.userType === 'UserLab' || this.userType === '1';
  }

  canAccessResume(): boolean {
    return this.isUser() || this.isUserLab();
  }

  canManageCenterProfile(): boolean {
    return this.isAdminLab() || this.isStore();
  }

  canViewReceptions(): boolean {
    return this.isLabPortalUser();
  }

  canViewAgreements(): boolean {
    return this.isAdminLab() || this.isUserLab();
  }

  /** AdminLab or UserLab portal users only. */
  isLabPortalUser(): boolean {
    if (
      this.isAdministrator() ||
      this.isAdmin() ||
      this.isStore() ||
      this.isUser()
    ) {
      return false;
    }
    return this.isAdminLab() || this.isUserLab();
  }
}
