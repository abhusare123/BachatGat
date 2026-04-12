import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { FundSummaryComponent } from './fund-summary/fund-summary.component';

const routes: Routes = [
  { path: '', component: FundSummaryComponent }
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule]
})
export class ReportsRoutingModule { }
