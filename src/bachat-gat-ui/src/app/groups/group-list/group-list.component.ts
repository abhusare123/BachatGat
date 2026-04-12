import { Component, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { CommonModule, CurrencyPipe } from '@angular/common';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatDialogModule, MatDialog } from '@angular/material/dialog';
import { GroupService } from '../../core/group.service';
import { Group } from '../../core/models';
import { CreateGroupDialogComponent } from '../create-group-dialog/create-group-dialog.component';

@Component({
  selector: 'app-group-list',
  imports: [CommonModule, MatCardModule, MatButtonModule, MatIconModule, MatProgressSpinnerModule, MatDialogModule, CurrencyPipe],
  templateUrl: './group-list.component.html',
  styleUrl: './group-list.component.scss'
})
export class GroupListComponent implements OnInit {
  groups: Group[] = [];
  loading = true;

  constructor(private groupSvc: GroupService, private router: Router, private dialog: MatDialog) {}

  ngOnInit() { this.load(); }

  load() {
    this.loading = true;
    this.groupSvc.getGroups().subscribe({
      next: g => { this.groups = g; this.loading = false; },
      error: () => this.loading = false
    });
  }

  openGroup(id: number) { this.router.navigate(['/groups', id]); }

  openCreate() {
    this.dialog.open(CreateGroupDialogComponent).afterClosed()
      .subscribe(created => { if (created) this.load(); });
  }
}
