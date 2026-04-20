import { Component, OnInit } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { CommonModule, CurrencyPipe, DatePipe } from '@angular/common';
import { MatTableModule } from '@angular/material/table';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatDialogModule, MatDialog } from '@angular/material/dialog';
import { PenaltyService } from '../../core/penalty.service';
import { GroupService } from '../../core/group.service';
import { AuthService } from '../../core/auth.service';
import { PenaltyDto, GroupMemberRole } from '../../core/models';
import { AddPenaltyDialogComponent } from '../add-penalty-dialog/add-penalty-dialog.component';

@Component({
  selector: 'app-penalty-list',
  imports: [
    CommonModule, CurrencyPipe, DatePipe,
    MatTableModule, MatButtonModule, MatIconModule,
    MatProgressSpinnerModule, MatDialogModule
  ],
  templateUrl: './penalty-list.component.html',
  styleUrl: './penalty-list.component.scss'
})
export class PenaltyListComponent implements OnInit {
  groupId!: number;
  penalties: PenaltyDto[] = [];
  loading = true;
  currentUserRole?: GroupMemberRole;
  displayedColumns = ['date', 'member', 'purpose', 'amount', 'addedBy'];

  get canAdd(): boolean {
    return this.currentUserRole !== undefined
      && this.currentUserRole <= GroupMemberRole.Treasurer;
  }

  get totalAmount(): number {
    return this.penalties.reduce((s, p) => s + p.amount, 0);
  }

  constructor(
    private route: ActivatedRoute,
    private penaltySvc: PenaltyService,
    private groupSvc: GroupService,
    private authSvc: AuthService,
    private dialog: MatDialog
  ) {}

  ngOnInit() {
    let r = this.route.snapshot;
    while (r && !r.paramMap.has('id')) r = r.parent!;
    this.groupId = +(r?.paramMap.get('id') ?? 0);

    this.groupSvc.getGroup(this.groupId).subscribe(g => {
      const userId = this.authSvc.currentUser()?.userId;
      const me = g.members.find(m => m.userId === userId && m.isActive);
      if (me) this.currentUserRole = me.role;
    });

    this.loadPenalties();
  }

  loadPenalties() {
    this.loading = true;
    this.penaltySvc.getPenalties(this.groupId).subscribe({
      next: data => { this.penalties = data; this.loading = false; },
      error: () => this.loading = false
    });
  }

  openAddDialog() {
    const ref = this.dialog.open(AddPenaltyDialogComponent, { width: '420px' });
    ref.componentInstance.setGroupId(this.groupId);
    ref.afterClosed().subscribe(added => { if (added) this.loadPenalties(); });
  }
}
