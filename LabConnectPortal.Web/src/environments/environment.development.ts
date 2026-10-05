import { resolveApiUrl } from './resolve-api-url';

export const environment = {
  production: false,
  apiUrl: resolveApiUrl(),
  /** Syncfusion Essential Studio license key (JavaScript platform). Leave empty to show trial banner. */
  syncfusionLicenseKey: '',
};
