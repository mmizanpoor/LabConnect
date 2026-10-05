import { Injectable } from '@angular/core';
import { AuthService } from '@core/services/auth/auth.service';
import { ProfileDto, UpdateProfileCommand } from './profile.types';

@Injectable({ providedIn: 'root' })
export class ProfileService {
  constructor(private _authService: AuthService) {}

  getProfile(): Promise<ProfileDto> {
    return this._authService.getProfile();
  }

  updateProfile(command: UpdateProfileCommand): Promise<void> {
    return this._authService.updateProfile(command);
  }
}
