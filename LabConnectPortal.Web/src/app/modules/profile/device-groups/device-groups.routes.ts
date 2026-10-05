import { Routes } from '@angular/router';
import { labPortalEntityGuard } from '@core/guards/lab-portal-entity.guard';
import { SystemEntity } from '@core/system-entity/system-entity';

export const deviceGroupsRoutes: Routes = [
  {
    path: '',
    canActivate: [labPortalEntityGuard(SystemEntity.DeviceGroup)],
    loadComponent: () =>
      import('./device-groups-list.component').then((m) => m.DeviceGroupsListComponent),
  },
];
