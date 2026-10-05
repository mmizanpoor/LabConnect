import { SystemEntity } from '@core/system-entity/system-entity';
import { ProfileNavLink } from './profile-nav.types';

export type {
  ProfileNavGroup,
  ProfileNavLink,
  ProfileNavVisibility,
} from './profile-nav.types';
export { isProfileLinkVisible } from './profile-nav.helpers';
export { isStoreCenterType } from '@core/portal/portal.types';

export const PROFILE_MESSAGES_LINK: ProfileNavLink = {
  route: '/profile/messages',
  icon: 'mail_outline',
  labelKey: 'modules.profile.layout.messages',
  systemEntity: SystemEntity.Message,
  requiredAction: 'view',
  exact: true,
};

export const PROFILE_LOGIN_REPORT_LINK: ProfileNavLink = {
  route: '/profile/login-report',
  icon: 'login',
  labelKey: 'modules.profile.layout.loginReport',
  exact: true,
};
