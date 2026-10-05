import { bootstrapApplication } from '@angular/platform-browser';
import { registerLicense } from '@syncfusion/ej2-base';
import { appConfig } from './app/app.config';
import { AppComponent } from './app/app.component';
import { environment } from '@env/environment';
import { syncfusionLicenseKey as fileSyncfusionLicenseKey } from '@env/syncfusion-license';

const syncfusionLicenseKey = fileSyncfusionLicenseKey.trim() || environment.syncfusionLicenseKey.trim();
if (syncfusionLicenseKey) {
  registerLicense(syncfusionLicenseKey);
}

bootstrapApplication(AppComponent, appConfig)
  .catch((err) => console.error(err));
