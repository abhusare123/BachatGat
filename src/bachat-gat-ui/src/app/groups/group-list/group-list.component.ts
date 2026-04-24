import { Component, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { CommonModule } from '@angular/common';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatDialogModule, MatDialog } from '@angular/material/dialog';
import { TranslateModule } from '@ngx-translate/core';
import { GroupService } from '../../core/group.service';
import { Group } from '../../core/models';
import { CreateGroupDialogComponent } from '../create-group-dialog/create-group-dialog.component';
import { GroupCardComponent, Group as DsGroup } from '../../shared/ui/group-card/group-card.component';
import { BrandHeroComponent } from '../../shared/ui/brand-hero/brand-hero.component';

@Component({
  selector: 'app-group-list',
  imports: [CommonModule, MatButtonModule, MatIconModule, MatProgressSpinnerModule, MatDialogModule, GroupCardComponent, BrandHeroComponent, TranslateModule],
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
      next: groups => {
        this.loading = false;
        if (groups.length === 1 && !history.state?.showAll) {
          this.router.navigate(['/groups', groups[0].id, 'reports'], { replaceUrl: true });
          return;
        }
        this.groups = groups;
      },
      error: () => this.loading = false
    });
  }

  toCardGroup(g: Group): DsGroup {
    return {
      id:           g.id,
      name:         g.name,
      memberCount:  g.memberCount,
      monthlyAmount: g.monthlyAmount,
      interestRate:  g.interestRatePercent,
      description:   g.description,
    };
  }

  openGroup(id: number) { this.router.navigate(['/groups', id, 'reports']); }

  openCreate() {
    this.dialog.open(CreateGroupDialogComponent).afterClosed()
      .subscribe(created => { if (created) this.load(); });
  }
}
