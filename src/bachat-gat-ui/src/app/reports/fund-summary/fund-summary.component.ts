import { Component, OnInit } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { CommonModule, CurrencyPipe } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { MatTableModule } from '@angular/material/table';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatInputModule } from '@angular/material/input';
import { ReportService } from '../../core/report.service';
import { GroupService } from '../../core/group.service';
import { FundSummary, LoanLedgerItem, MonthlyReport } from '../../core/models';
import { StatTileComponent } from '../../shared/ui/stat-tile/stat-tile.component';
import { jsPDF } from 'jspdf';
import autoTable from 'jspdf-autotable';

@Component({
  selector: 'app-fund-summary',
  imports: [CommonModule, FormsModule, MatTableModule, MatProgressSpinnerModule, CurrencyPipe, MatButtonModule, MatIconModule, MatInputModule, StatTileComponent],
  templateUrl: './fund-summary.component.html',
  styleUrl: './fund-summary.component.scss'
})
export class FundSummaryComponent implements OnInit {
  groupId!: number;
  groupName = '';
  summary?: FundSummary;
  ledger: LoanLedgerItem[] = [];
  monthlyReport?: MonthlyReport;
  selectedPeriod = new Date().toISOString().slice(0, 7);
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
    this.loadMonthlyReport();
  }

  loadMonthlyReport() {
    this.monthlyReport = undefined;
    this.reportSvc.getMonthlyReport(this.groupId, this.selectedPeriod)
      .subscribe({ next: r => this.monthlyReport = r, error: err => console.error('Monthly report error', err) });
  }

  exportPdf() {
    if (!this.summary) return;

    const doc = new jsPDF();
    const W = 210;
    const s = this.summary;

    const inr = (n: number) =>
      'Rs. ' + new Intl.NumberFormat('en-IN', { maximumFractionDigits: 0 }).format(n);
    const today = new Date().toLocaleDateString('en-IN',
      { day: '2-digit', month: 'long', year: 'numeric' });

    const C = {
      greenDark:  [27, 94, 32]   as [number,number,number],
      greenMid:   [46, 125, 50]  as [number,number,number],
      greenLight: [232, 245, 233] as [number,number,number],
      white:      [255, 255, 255] as [number,number,number],
      gray:       [120, 120, 120] as [number,number,number],
      dark:       [30, 30, 30]   as [number,number,number],
    };

    // ── Header ───────────────────────────────────────────────
    doc.setFillColor(...C.greenDark);
    doc.rect(0, 0, W, 30, 'F');
    doc.setFillColor(...C.greenMid);
    doc.rect(0, 30, W, 2.5, 'F');

    doc.setTextColor(...C.white);
    doc.setFont('helvetica', 'bold');
    doc.setFontSize(18);
    doc.text(this.groupName || 'Bachat Gat', 14, 14);
    doc.setFont('helvetica', 'normal');
    doc.setFontSize(8.5);
    doc.text('Group Financial Summary Report', 14, 23);
    doc.text(`As of ${today}`, W - 14, 23, { align: 'right' });

    // ── Fund Overview section header ─────────────────────────
    doc.setTextColor(...C.dark);
    doc.setFont('helvetica', 'bold');
    doc.setFontSize(9);
    doc.text('FUND OVERVIEW', 14, 42);
    doc.setDrawColor(...C.greenMid);
    doc.setLineWidth(0.4);
    doc.line(14, 44, W - 14, 44);

    // ── Stat cards (2 columns) ────────────────────────────────
    const cardDefs = [
      { label: 'Total Contributions Collected', value: inr(s.totalContributionsCollected), accent: [76, 175, 80]  as [number,number,number] },
      { label: 'Total Loans Disbursed',          value: inr(s.totalLoansDisbursed),         accent: [33, 150, 243] as [number,number,number] },
      { label: 'Loan Outstanding',               value: inr(s.totalLoanOutstanding),        accent: [255, 152, 0]  as [number,number,number] },
      { label: 'Interest Collected',             value: inr(s.totalInterestCollected),      accent: [0, 188, 212]  as [number,number,number] },
      { label: 'Total Expenses',                 value: inr(s.totalExpenses),               accent: [244, 67, 54]  as [number,number,number] },
      { label: 'Other Income',                   value: inr(s.totalOtherIncome),            accent: [255, 193, 7]  as [number,number,number] },
    ];

    const cW = 87, cH = 19, colX = [14, 109], startY = 48, gap = 4;
    cardDefs.forEach((card, i) => {
      const cx = colX[i % 2];
      const cy = startY + Math.floor(i / 2) * (cH + gap);
      doc.setFillColor(248, 251, 248);
      doc.roundedRect(cx, cy, cW, cH, 2, 2, 'F');
      doc.setFillColor(...card.accent);
      doc.rect(cx, cy, 3.5, cH, 'F');
      doc.setFont('helvetica', 'normal');
      doc.setFontSize(7.5);
      doc.setTextColor(...C.gray);
      doc.text(card.label.toUpperCase(), cx + 7, cy + 7);
      doc.setFont('helvetica', 'bold');
      doc.setFontSize(11.5);
      doc.setTextColor(...C.dark);
      doc.text(card.value, cx + 7, cy + 15);
    });

    // ── Available Balance highlight bar ───────────────────────
    const balY = startY + Math.ceil(cardDefs.length / 2) * (cH + gap) + 5;
    doc.setFillColor(...C.greenDark);
    doc.roundedRect(14, balY, W - 28, 22, 3, 3, 'F');
    doc.setFont('helvetica', 'normal');
    doc.setFontSize(8.5);
    doc.setTextColor(...C.white);
    doc.text('AVAILABLE BALANCE  /  निव्वळ शिल्लक', 22, balY + 9);
    doc.setFont('helvetica', 'bold');
    doc.setFontSize(15);
    doc.text(inr(s.availableBalance), W - 22, balY + 15, { align: 'right' });
    doc.setTextColor(...C.dark);

    // ── Loan Ledger ───────────────────────────────────────────
    if (this.ledger.length > 0) {
      const ledgerY = balY + 30;
      doc.setFont('helvetica', 'bold');
      doc.setFontSize(9);
      doc.text('LOAN LEDGER', 14, ledgerY);
      doc.setDrawColor(...C.greenMid);
      doc.setLineWidth(0.4);
      doc.line(14, ledgerY + 2, W - 14, ledgerY + 2);

      autoTable(doc, {
        startY: ledgerY + 5,
        head: [['Member', 'Loan Amount', 'Outstanding', 'Interest Paid', 'Status']],
        body: this.ledger.map(l => [
          l.memberName,
          inr(l.originalAmount),
          inr(l.outstandingBalance),
          inr(l.totalInterestPaid),
          l.status,
        ]),
        headStyles: { fillColor: C.greenDark, textColor: 255, fontStyle: 'bold', fontSize: 8.5 },
        bodyStyles: { fontSize: 8.5, textColor: C.dark },
        columnStyles: {
          0: { cellWidth: 52 },
          1: { cellWidth: 34, halign: 'right' },
          2: { cellWidth: 34, halign: 'right' },
          3: { cellWidth: 34, halign: 'right' },
          4: { cellWidth: 28, halign: 'center' },
        },
        alternateRowStyles: { fillColor: C.greenLight },
        didParseCell: (data) => {
          if (data.column.index === 4 && data.section === 'body') {
            const st = String(data.cell.raw);
            data.cell.styles.textColor = st === 'Active' ? C.greenDark : C.gray;
            data.cell.styles.fontStyle = 'bold';
          }
        },
        margin: { left: 14, right: 14 },
      });
    }

    // ── Footer ────────────────────────────────────────────────
    const pageCount = (doc as any).internal.getNumberOfPages();
    for (let i = 1; i <= pageCount; i++) {
      doc.setPage(i);
      doc.setFillColor(245, 245, 245);
      doc.rect(0, 285, W, 12, 'F');
      doc.setFont('helvetica', 'normal');
      doc.setFontSize(7.5);
      doc.setTextColor(...C.gray);
      doc.text('BachatGat — Confidential', 14, 291);
      doc.text(`Page ${i} of ${pageCount}`, W - 14, 291, { align: 'right' });
    }

    const filename = `${(this.groupName || 'report').replace(/\s+/g, '-')}-fund-summary.pdf`;
    doc.save(filename);
  }

  exportMonthlyPdf() {
    if (!this.monthlyReport) return;

    const r = this.monthlyReport;
    const doc = new jsPDF({ orientation: 'landscape' });
    const W = 297;

    const inr = (n: number) =>
      'Rs. ' + new Intl.NumberFormat('en-IN', { maximumFractionDigits: 0 }).format(n);
    const today = new Date().toLocaleDateString('en-IN',
      { day: '2-digit', month: 'long', year: 'numeric' });
    const periodLabel = (() => {
      const [y, m] = r.period.split('-');
      return new Date(+y, +m - 1).toLocaleDateString('en-IN', { month: 'long', year: 'numeric' });
    })();

    const C = {
      greenDark:  [27, 94, 32]    as [number,number,number],
      greenMid:   [46, 125, 50]   as [number,number,number],
      greenLight: [232, 245, 233]  as [number,number,number],
      greenTotal: [200, 230, 201]  as [number,number,number],
      white:      [255, 255, 255]  as [number,number,number],
      gray:       [110, 110, 110]  as [number,number,number],
      dark:       [30, 30, 30]    as [number,number,number],
    };

    // ── Header ───────────────────────────────────────────────
    doc.setFillColor(...C.greenDark);
    doc.rect(0, 0, W, 30, 'F');
    doc.setFillColor(...C.greenMid);
    doc.rect(0, 30, W, 2.5, 'F');

    doc.setTextColor(...C.white);
    doc.setFont('helvetica', 'bold');
    doc.setFontSize(16);
    doc.text(this.groupName || r.groupName, 12, 13);
    doc.setFont('helvetica', 'normal');
    doc.setFontSize(8.5);
    doc.text(`Monthly Report  —  ${periodLabel}`, 12, 23);
    doc.text(`Exported: ${today}`, W - 12, 23, { align: 'right' });
    doc.setTextColor(...C.dark);

    // ── Summary highlight boxes (top) ────────────────────────
    const bxW = (W - 28 - 8) / 3;
    const bxY = 36;
    const bxH = 14;
    [
      { label: 'Monthly Contributions',  value: inr(r.totalMonthlyContributions),                     color: [76, 175, 80]  as [number,number,number] },
      { label: 'Total EMI Collected',    value: inr(r.totalMonthlyPrincipal + r.totalMonthlyInterest), color: [33, 150, 243] as [number,number,number] },
      { label: 'Grand Total Due',        value: inr(r.grandTotalDue),                                  color: C.greenDark },
    ].forEach((box, i) => {
      const bx = 12 + i * (bxW + 4);
      doc.setFillColor(...box.color);
      doc.roundedRect(bx, bxY, bxW, bxH, 2, 2, 'F');
      doc.setTextColor(...C.white);
      doc.setFont('helvetica', 'normal');
      doc.setFontSize(6.5);
      doc.text(box.label.toUpperCase(), bx + bxW / 2, bxY + 5, { align: 'center' });
      doc.setFont('helvetica', 'bold');
      doc.setFontSize(10);
      doc.text(box.value, bx + bxW / 2, bxY + 11.5, { align: 'center' });
    });
    doc.setTextColor(...C.dark);

    // ── Section header ────────────────────────────────────────
    doc.setFont('helvetica', 'bold');
    doc.setFontSize(9);
    doc.text('MEMBER-WISE MONTHLY DETAILS', 12, 58);
    doc.setDrawColor(...C.greenMid);
    doc.setLineWidth(0.4);
    doc.line(12, 60, W - 12, 60);

    // ── Main table (grouped headers) ──────────────────────────
    autoTable(doc, {
      startY: 63,
      head: [
        [
          { content: '#',           rowSpan: 2, styles: { halign: 'center', valign: 'middle' } },
          { content: 'Member Name', rowSpan: 2, styles: { valign: 'middle' } },
          { content: 'Contributions', colSpan: 2, styles: { halign: 'center' } },
          { content: 'Loan',          colSpan: 4, styles: { halign: 'center' } },
          { content: 'Monthly',       colSpan: 2, styles: { halign: 'center' } },
        ],
        ['Total', 'This Month', 'Disbursed', 'EMI (P)', 'EMI (I)', 'Outstanding', 'Penalty', 'Total Due'],
      ],
      body: [
        ...r.members.map(m => [
          m.serial,
          m.memberName,
          inr(m.totalContributions),
          inr(m.monthlyContribution),
          inr(m.loanDisbursed),
          inr(m.monthlyPrincipal),
          inr(m.monthlyInterest),
          inr(m.outstandingLoan),
          inr(m.monthlyPenalty),
          inr(m.totalDue),
        ]),
        [
          { content: 'TOTAL', colSpan: 2, styles: { fontStyle: 'bold' as const, halign: 'right' as const } },
          inr(r.totalContributions),
          inr(r.totalMonthlyContributions),
          inr(r.totalLoanDisbursed),
          inr(r.totalMonthlyPrincipal),
          inr(r.totalMonthlyInterest),
          inr(r.totalOutstandingLoan),
          inr(r.totalMonthlyPenalties),
          inr(r.grandTotalDue),
        ],
      ],
      headStyles: { fillColor: C.greenDark, textColor: 255, fontStyle: 'bold', fontSize: 7.5, halign: 'right' },
      bodyStyles: { fontSize: 8, textColor: C.dark },
      columnStyles: {
        0: { cellWidth: 9,  halign: 'center' },
        1: { cellWidth: 42, halign: 'left'   },
        2: { cellWidth: 24, halign: 'right'  },
        3: { cellWidth: 24, halign: 'right'  },
        4: { cellWidth: 24, halign: 'right'  },
        5: { cellWidth: 24, halign: 'right'  },
        6: { cellWidth: 24, halign: 'right'  },
        7: { cellWidth: 26, halign: 'right'  },
        8: { cellWidth: 20, halign: 'right'  },
        9: { cellWidth: 26, halign: 'right'  },
      },
      alternateRowStyles: { fillColor: C.greenLight },
      didParseCell: (data) => {
        if (data.row.index === r.members.length) {
          data.cell.styles.fillColor = C.greenTotal;
          data.cell.styles.fontStyle = 'bold';
          data.cell.styles.fontSize = 8.5;
        }
      },
      margin: { left: 12, right: 12 },
    });

    // ── Footer ────────────────────────────────────────────────
    const pageCount = (doc as any).internal.getNumberOfPages();
    for (let i = 1; i <= pageCount; i++) {
      doc.setPage(i);
      doc.setFillColor(245, 245, 245);
      doc.rect(0, 203, W, 9, 'F');
      doc.setFont('helvetica', 'normal');
      doc.setFontSize(7.5);
      doc.setTextColor(...C.gray);
      doc.text('BachatGat — Confidential', 12, 209);
      doc.text(`Page ${i} of ${pageCount}`, W - 12, 209, { align: 'right' });
    }

    const filename = `${(this.groupName || 'report').replace(/\s+/g, '-')}-monthly-${r.period}.pdf`;
    doc.save(filename);
  }
}
