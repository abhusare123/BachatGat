import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { GroupListComponent } from './group-list/group-list.component';
import { GroupShellComponent } from './group-shell/group-shell.component';
import { GroupDetailComponent } from './group-detail/group-detail.component';

const routes: Routes = [
  // Group list — auto-redirects to single group
  { path: '', component: GroupListComponent },

  // Group shell — sidebar layout wrapping all group sections
  {
    path: ':id',
    component: GroupShellComponent,
    children: [
      { path: '', redirectTo: 'reports', pathMatch: 'full' },
      {
        path: 'contributions',
        loadChildren: () => import('../contributions/contributions.module').then(m => m.ContributionsModule)
      },
      {
        path: 'loans',
        loadChildren: () => import('../loans/loans.module').then(m => m.LoansModule)
      },
      {
        path: 'reports',
        loadChildren: () => import('../reports/reports.module').then(m => m.ReportsModule)
      },
      {
        path: 'expenses',
        loadChildren: () => import('../expenses/expenses.module').then(m => m.ExpensesModule)
      },
      { path: 'members', component: GroupDetailComponent },
      {
        path: 'income',
        loadChildren: () => import('../group-income/group-income.module').then(m => m.GroupIncomeModule)
      },
      {
        path: 'rules',
        loadChildren: () => import('./rules/rules.module').then(m => m.RulesModule)
      }
    ]
  }
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule]
})
export class GroupsRoutingModule { }
