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

  { path: '**', redirectTo: 'groups' }
];
