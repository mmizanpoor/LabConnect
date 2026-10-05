import { Routes } from '@angular/router';

export const authRoutes: Routes = [
  { path: '', redirectTo: 'login', pathMatch: 'full' },
  {
    path: 'login',
    loadComponent: () => import('./login/login.component').then(m => m.LoginComponent),
  },
  {
    path: 'register-store',
    loadComponent: () => import('./register-store/register-store.component').then(m => m.RegisterStoreComponent),
  },
];
