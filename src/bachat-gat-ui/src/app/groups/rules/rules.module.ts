import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RulesRoutingModule } from './rules-routing.module';
import { RuleListComponent } from './rule-list/rule-list.component';
import { EditRuleDialogComponent } from './edit-rule-dialog/edit-rule-dialog.component';

@NgModule({
  imports: [
    CommonModule,
    RulesRoutingModule,
    RuleListComponent,
    EditRuleDialogComponent
  ]
})
export class RulesModule { }
