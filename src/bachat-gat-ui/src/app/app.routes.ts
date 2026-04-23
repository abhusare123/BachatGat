import { Routes } from '@angular/router';
import { authGuard } from './core/auth.guard';

export const routes: Routes = [
  { path: '', redirectTo: 'groups', pathMatch: 'full' },

  {
    path: 'login',
    loadChildren: () => import('./auth/auth.module').then(m => m.AuthModule)
  },

  // Group list (auto-redirects to single group if only one)
  {
    path: 'groups',
    canActivate: [authGuard],
    loadChildren: () => import('./groups/groups.module').then(m => m.GroupsModule)
  },

  {
    path: 'profile',
    canActivate: [authGuard],
    loadChildren: () => import('./profile/profile.module').then(m => m.ProfileModule)
  },

  {
    path: 'dashboard',
    canActivate: [authGuard],
    loadComponent: () =>
      import('./features/dashboard/dashboard.component').then(m => m.DashboardComponent),
  },

  { path: '**', redirectTo: 'groups' }
];
