import { Routes } from '@angular/router';
import { labPortalEntityGuard } from '@core/guards/lab-portal-entity.guard';
import { SystemEntity } from '@core/system-entity/system-entity';

export const labUsersRoutes: Routes = [
  {
    path: '',
    canActivate: [labPortalEntityGuard(SystemEntity.LabUser, 'view')],
    loadComponent: () => import('./lab-users.component').then((m) => m.LabUsersComponent),
  },
];
