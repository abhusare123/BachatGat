import { Component, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { CommonModule } from '@angular/common';
import { MatTabsModule } from '@angular/material/tabs';
import { MatTableModule } from '@angular/material/table';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatChipsModule } from '@angular/material/chips';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatDialogModule, MatDialog } from '@angular/material/dialog';
import { GroupService } from '../../core/group.service';
import { AuthService } from '../../core/auth.service';
import { GroupDetail, GroupMemberRole } from '../../core/models';
import { AddMemberDialogComponent } from '../add-member-dialog/add-member-dialog.component';

@Component({
  selector: 'app-group-detail',
  imports: [
    CommonModule, MatTabsModule, MatTableModule, MatButtonModule,
    MatIconModule, MatChipsModule, MatProgressSpinnerModule, MatDialogModule
  ],
  templateUrl: './group-detail.component.html',
  styleUrl: './group-detail.component.scss'
})
export class GroupDetailComponent implements OnInit {
  groupId!: number;
  group?: GroupDetail;
  loading = true;
  GroupMemberRole = GroupMemberRole;

  get isAdmin(): boolean {
    const userId = this.authSvc.currentUser()?.userId;
    if (!userId || !this.group) return false;
    return this.group.members.some(m => m.userId === userId && m.role === GroupMemberRole.Admin);
  }

  get displayedColumns(): string[] {
    return this.isAdmin ? ['name', 'phone', 'role', 'joined', 'actions'] : ['name', 'phone', 'role', 'joined'];
  }

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private groupSvc: GroupService,
    private authSvc: AuthService,
    private dialog: MatDialog
  ) {}

  ngOnInit() {
    this.groupId = +this.route.snapshot.paramMap.get('id')!;
    this.load();
  }

  load() {
    this.loading = true;
    this.groupSvc.getGroup(this.groupId).subscribe({
      next: g => { this.group = g; this.loading = false; },
      error: () => this.loading = false
    });
  }

  roleLabel(role: GroupMemberRole): string {
    return GroupMemberRole[role];
  }

  goTo(section: string) {
    this.router.navigate([`/groups/${this.groupId}/${section}`]);
  }

  openAddMember() {
    this.dialog.open(AddMemberDialogComponent, { data: { groupId: this.groupId } })
      .afterClosed().subscribe(added => { if (added) this.load(); });
  }

  viewProfile(userId: number) {
    this.router.navigate(['/profile', userId]);
  }

  removeMember(memberId: number) {
    if (!confirm('Remove this member from the group?')) return;
    this.groupSvc.removeMember(this.groupId, memberId).subscribe(() => this.load());
  }
}
