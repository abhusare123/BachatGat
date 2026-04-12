import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { LoanListComponent } from './loan-list/loan-list.component';
import { LoanRequestComponent } from './loan-request/loan-request.component';
import { RepaymentListComponent } from './repayment-list/repayment-list.component';

const routes: Routes = [
  { path: '', component: LoanListComponent },
  { path: 'request', component: LoanRequestComponent },
  { path: ':loanId/repayments', component: RepaymentListComponent }
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule]
})
export class LoansRoutingModule { }
