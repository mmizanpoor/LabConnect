import { resolveApiUrl } from './resolve-api-url';

export const environment = {
  production: true,
  apiUrl: resolveApiUrl(),
  /** Syncfusion Essential Studio license key (JavaScript platform). Leave empty to show trial banner. */
  syncfusionLicenseKey: '',
};
