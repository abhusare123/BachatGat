import { Component, Inject } from '@angular/core';
import { CommonModule, CurrencyPipe } from '@angular/common';
import { MatDialogModule, MatDialogRef, MAT_DIALOG_DATA } from '@angular/material/dialog';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { ForeclosureSummary } from '../../core/models';

export interface CloseLoanDialogData {
  borrowerName: string;
  summary: ForeclosureSummary;
}

@Component({
  selector: 'app-close-loan-dialog',
  standalone: true,
  imports: [CommonModule, MatDialogModule, MatButtonModule, MatIconModule, CurrencyPipe],
  template: `
    <h2 mat-dialog-title>Close Loan Early / कर्ज बंद करा</h2>
    <mat-dialog-content>
      <p>Closing loan for <strong>{{ data.borrowerName }}</strong>.</p>
      <p>Future EMI interest will be waived. The member must pay:</p>
      <table class="summary-table">
        <tr>
          <td>Outstanding Principal</td>
          <td class="amount">{{ data.summary.outstandingPrincipal | currency:'INR':'symbol':'1.2-2' }}</td>
        </tr>
        <tr>
          <td>Interest (up to current month)</td>
          <td class="amount">{{ data.summary.foreclosureInterest | currency:'INR':'symbol':'1.2-2' }}</td>
        </tr>
        <tr class="total-row">
          <td><strong>Total Payable</strong></td>
          <td class="amount"><strong>{{ data.summary.totalAmount | currency:'INR':'symbol':'1.2-2' }}</strong></td>
        </tr>
      </table>
    </mat-dialog-content>
    <mat-dialog-actions align="end">
      <button mat-button mat-dialog-close>Cancel</button>
      <button mat-raised-button color="warn" [mat-dialog-close]="true">
        <mat-icon>lock</mat-icon> Confirm Close
      </button>
    </mat-dialog-actions>
  `,
  styles: [`
    .summary-table { width: 100%; border-collapse: collapse; margin-top: 12px; }
    .summary-table td { padding: 6px 8px; }
    .amount { text-align: right; }
    .total-row td { border-top: 1px solid #ccc; padding-top: 10px; }
  `]
})
export class CloseLoanDialogComponent {
  constructor(
    public dialogRef: MatDialogRef<CloseLoanDialogComponent>,
    @Inject(MAT_DIALOG_DATA) public data: CloseLoanDialogData
  ) {}
}
