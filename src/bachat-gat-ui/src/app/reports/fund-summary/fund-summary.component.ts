import { Component, OnInit } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { CommonModule, CurrencyPipe } from '@angular/common';
import { MatCardModule } from '@angular/material/card';
import { MatTableModule } from '@angular/material/table';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatIconModule } from '@angular/material/icon';
import { ReportService } from '../../core/report.service';
import { FundSummary, LoanLedgerItem } from '../../core/models';

@Component({
  selector: 'app-fund-summary',
  imports: [CommonModule, MatCardModule, MatTableModule, MatProgressSpinnerModule, MatIconModule, CurrencyPipe],
  templateUrl: './fund-summary.component.html',
  styleUrl: './fund-summary.component.scss'
})
export class FundSummaryComponent implements OnInit {
  groupId!: number;
  summary?: FundSummary;
  ledger: LoanLedgerItem[] = [];
  loading = true;
  ledgerColumns = ['member', 'original', 'outstanding', 'interestPaid', 'status'];

  constructor(private route: ActivatedRoute, private reportSvc: ReportService) {}

  ngOnInit() {
    this.groupId = +this.route.snapshot.paramMap.get('id')!;
    this.reportSvc.getFundSummary(this.groupId).subscribe(s => { this.summary = s; this.loading = false; });
    this.reportSvc.getLoanLedger(this.groupId).subscribe(l => this.ledger = l);
  }
}
