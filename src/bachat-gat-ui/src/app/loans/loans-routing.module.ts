import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { LoanListComponent } from './loan-list/loan-list.component';
import { LoanRequestComponent } from './loan-request/loan-request.component';

const routes: Routes = [
  { path: '', component: LoanListComponent },
  { path: 'request', component: LoanRequestComponent }
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule]
})
export class LoansRoutingModule { }
