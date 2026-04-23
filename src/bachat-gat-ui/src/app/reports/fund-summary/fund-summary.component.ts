import { Component, OnInit } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { CommonModule, CurrencyPipe } from '@angular/common';
import { MatCardModule } from '@angular/material/card';
import { MatTableModule } from '@angular/material/table';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { ReportService } from '../../core/report.service';
import { GroupService } from '../../core/group.service';
import { FundSummary, LoanLedgerItem } from '../../core/models';
import { jsPDF } from 'jspdf';
import autoTable from 'jspdf-autotable';

@Component({
  selector: 'app-fund-summary',
  imports: [CommonModule, MatCardModule, MatTableModule, MatProgressSpinnerModule, MatIconModule, CurrencyPipe, MatButtonModule],
  templateUrl: './fund-summary.component.html',
  styleUrl: './fund-summary.component.scss'
})
export class FundSummaryComponent implements OnInit {
  groupId!: number;
  groupName = '';
  summary?: FundSummary;
  ledger: LoanLedgerItem[] = [];
  loading = true;
  ledgerColumns = ['member', 'original', 'outstanding', 'interestPaid', 'status'];

  constructor(
    private route: ActivatedRoute,
    private reportSvc: ReportService,
    private groupSvc: GroupService
  ) {}

  ngOnInit() {
    let r = this.route.snapshot;
    while (r && !r.paramMap.has('id')) r = r.parent!;
    this.groupId = +(r?.paramMap.get('id') ?? 0);
    this.groupSvc.getGroup(this.groupId).subscribe(g => this.groupName = g.name);
    this.reportSvc.getFundSummary(this.groupId).subscribe(s => { this.summary = s; this.loading = false; });
    this.reportSvc.getLoanLedger(this.groupId).subscribe(l => this.ledger = l);
  }

  exportPdf() {
    if (!this.summary) return;

    const doc = new jsPDF();
    const inr = (n: number) =>
      'Rs. ' + new Intl.NumberFormat('en-IN', { maximumFractionDigits: 0 }).format(n);
    const today = new Date().toLocaleDateString('en-IN', { day: '2-digit', month: 'long', year: 'numeric' });

    // ── Header ──────────────────────────────────────────────
    doc.setFillColor(27, 94, 32);
    doc.rect(0, 0, 210, 28, 'F');

    doc.setTextColor(255, 255, 255);
    doc.setFontSize(16);
    doc.setFont('helvetica', 'bold');
    doc.text(this.groupName || 'Bachat Gat', 14, 12);

    doc.setFontSize(9);
    doc.setFont('helvetica', 'normal');
    doc.text('Group Financial Report', 14, 20);
    doc.text(`Exported: ${today}`, 196, 20, { align: 'right' });

    doc.setTextColor(0, 0, 0);

    // ── Fund Summary ─────────────────────────────────────────
    doc.setFontSize(11);
    doc.setFont('helvetica', 'bold');
    doc.text('Fund Summary', 14, 38);

    const s = this.summary;
    autoTable(doc, {
      startY: 42,
      head: [['Description', 'Amount']],
      body: [
        ['Total Contributions Collected', inr(s.totalContributionsCollected)],
        ['Total Loans Disbursed',         inr(s.totalLoansDisbursed)],
        ['Loan Outstanding',              inr(s.totalLoanOutstanding)],
        ['Interest Collected',            inr(s.totalInterestCollected)],
        ['Total Expenses',                inr(s.totalExpenses)],
        ['Other Income',                  inr(s.totalOtherIncome)],
        ['Remaining Balance',             inr(s.availableBalance)],
      ],
      headStyles: { fillColor: [27, 94, 32], textColor: 255, fontStyle: 'bold' },
      columnStyles: {
        0: { cellWidth: 120 },
        1: { cellWidth: 62, halign: 'right', fontStyle: 'bold' },
      },
      alternateRowStyles: { fillColor: [240, 248, 240] },
      didDrawCell: (data) => {
        if (data.section === 'body' && data.row.index === 6) {
          doc.setFillColor(232, 245, 233);
        }
      },
      margin: { left: 14, right: 14 },
    });

    // ── Loan Ledger ───────────────────────────────────────────
    if (this.ledger.length > 0) {
      const afterContrib = (doc as any).lastAutoTable.finalY + 12;
      doc.setFontSize(11);
      doc.setFont('helvetica', 'bold');
      doc.text('Loan Ledger', 14, afterContrib);

      autoTable(doc, {
        startY: afterContrib + 4,
        head: [['Member', 'Loan Amount', 'Outstanding', 'Interest Paid', 'Status']],
        body: this.ledger.map(l => [
          l.memberName,
          inr(l.originalAmount),
          inr(l.outstandingBalance),
          inr(l.totalInterestPaid),
          l.status,
        ]),
        headStyles: { fillColor: [27, 94, 32], textColor: 255, fontStyle: 'bold' },
        columnStyles: {
          0: { cellWidth: 48 },
          1: { cellWidth: 36, halign: 'right' },
          2: { cellWidth: 36, halign: 'right' },
          3: { cellWidth: 36, halign: 'right' },
          4: { cellWidth: 26, halign: 'center' },
        },
        alternateRowStyles: { fillColor: [240, 248, 240] },
        margin: { left: 14, right: 14 },
      });
    }

    // ── Footer (all pages) ────────────────────────────────────
    const pageCount = (doc as any).internal.getNumberOfPages();
    doc.setFontSize(8);
    doc.setFont('helvetica', 'normal');
    doc.setTextColor(120, 120, 120);
    for (let i = 1; i <= pageCount; i++) {
      doc.setPage(i);
      doc.text(`Page ${i} of ${pageCount}`, 196, 290, { align: 'right' });
    }

    const filename = `${(this.groupName || 'report').replace(/\s+/g, '-')}-report.pdf`;
    doc.save(filename);
  }
}
