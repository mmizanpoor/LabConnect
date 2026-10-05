import { Routes } from '@angular/router';
import { labPortalEntityGuard } from '@core/guards/lab-portal-entity.guard';
import { SystemEntity } from '@core/system-entity/system-entity';

export const kitGroupsRoutes: Routes = [
  {
    path: '',
    canActivate: [labPortalEntityGuard(SystemEntity.KitGroup)],
    loadComponent: () => import('./kit-groups-list.component').then((m) => m.KitGroupsListComponent),
  },
];
