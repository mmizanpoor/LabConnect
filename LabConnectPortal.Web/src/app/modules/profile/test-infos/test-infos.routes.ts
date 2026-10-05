import { Routes } from '@angular/router';
import { labPortalEntityGuard } from '@core/guards/lab-portal-entity.guard';
import { SystemEntity } from '@core/system-entity/system-entity';

export const testInfosRoutes: Routes = [
  {
    path: '',
    canActivate: [labPortalEntityGuard(SystemEntity.TestInfo)],
    loadComponent: () => import('./test-infos-list.component').then((m) => m.TestInfosListComponent),
  },
];
