import { Routes } from '@angular/router';
import { authGuard } from './core/auth.guard';

export const routes: Routes = [
  { path: '', redirectTo: 'groups', pathMatch: 'full' },
  {
    path: 'login',
    loadChildren: () => import('./auth/auth.module').then(m => m.AuthModule)
  },
  {
    path: 'groups',
    canActivate: [authGuard],
    loadChildren: () => import('./groups/groups.module').then(m => m.GroupsModule)
  },
  {
    path: 'groups/:groupId/contributions',
    canActivate: [authGuard],
    loadChildren: () => import('./contributions/contributions.module').then(m => m.ContributionsModule)
  },
  {
    path: 'groups/:groupId/loans',
    canActivate: [authGuard],
    loadChildren: () => import('./loans/loans.module').then(m => m.LoansModule)
  },
  {
    path: 'groups/:groupId/reports',
    canActivate: [authGuard],
    loadChildren: () => import('./reports/reports.module').then(m => m.ReportsModule)
  },
  { path: '**', redirectTo: 'groups' }
];
