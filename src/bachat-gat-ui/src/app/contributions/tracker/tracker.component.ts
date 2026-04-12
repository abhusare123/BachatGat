import { Component, OnInit } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { CommonModule, CurrencyPipe } from '@angular/common';
import { MatTableModule } from '@angular/material/table';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatDialogModule, MatDialog } from '@angular/material/dialog';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatCardModule } from '@angular/material/card';
import { MatChipsModule } from '@angular/material/chips';
import { ContributionService } from '../../core/contribution.service';
import { ContributionTracker } from '../../core/models';
import { RecordPaymentDialogComponent } from '../record-payment-dialog/record-payment-dialog.component';

@Component({
  selector: 'app-tracker',
  imports: [
    CommonModule, MatTableModule, MatButtonModule, MatIconModule,
    MatDialogModule, MatProgressSpinnerModule, MatCardModule, MatChipsModule, CurrencyPipe
  ],
  templateUrl: './tracker.component.html',
  styleUrl: './tracker.component.scss'
})
export class TrackerComponent implements OnInit {
  groupId!: number;
  tracker?: ContributionTracker;
  loading = true;

  constructor(
    private route: ActivatedRoute,
    private contributionSvc: ContributionService,
    private dialog: MatDialog
  ) {}

  ngOnInit() {
    this.groupId = +this.route.parent!.snapshot.paramMap.get('groupId')!;
    this.load();
  }

  load() {
    this.loading = true;
    this.contributionSvc.getTracker(this.groupId).subscribe({
      next: data => { this.tracker = data; this.loading = false; },
      error: () => this.loading = false
    });
  }

  openRecordPayment(memberId: number, memberName: string) {
    const ref = this.dialog.open(RecordPaymentDialogComponent, {
      data: { groupId: this.groupId, groupMemberId: memberId, memberName }
    });
    ref.afterClosed().subscribe(saved => { if (saved) this.load(); });
  }

  formatPeriod(period: string): string {
    const [y, m] = period.split('-');
    return new Date(+y, +m - 1).toLocaleString('default', { month: 'short', year: '2-digit' });
  }
}
