import { Component, OnInit, signal } from '@angular/core';
import { ActivatedRoute, Router, RouterOutlet, RouterLink, RouterLinkActive } from '@angular/router';
import { CommonModule } from '@angular/common';
import { MatIconModule } from '@angular/material/icon';
import { MatListModule } from '@angular/material/list';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { TranslateModule } from '@ngx-translate/core';
import { GroupService } from '../../core/group.service';

interface NavItem {
  labelKey: string;
  icon: string;
  path: string;
}

@Component({
  selector: 'app-group-shell',
  imports: [
    CommonModule, RouterOutlet, RouterLink, RouterLinkActive,
    MatIconModule, MatListModule, MatProgressSpinnerModule, TranslateModule
  ],
  templateUrl: './group-shell.component.html',
  styleUrl: './group-shell.component.scss'
})
export class GroupShellComponent implements OnInit {
  groupId!: number;
  groupName = signal('');

  navItems: NavItem[] = [
    { labelKey: 'nav.reports',       icon: 'bar_chart',       path: 'reports'       },
    { labelKey: 'nav.contributions', icon: 'savings',         path: 'contributions' },
    { labelKey: 'nav.loans',         icon: 'account_balance', path: 'loans'         },
    { labelKey: 'nav.expenses',      icon: 'receipt_long',    path: 'expenses'      },
    { labelKey: 'nav.otherIncome',   icon: 'account_balance', path: 'income'        },
  ];

  constructor(private route: ActivatedRoute, private router: Router, private groupSvc: GroupService) {}

  ngOnInit() {
    this.groupId = +this.route.snapshot.paramMap.get('id')!;
    this.groupSvc.getGroup(this.groupId).subscribe(g => this.groupName.set(g.name));
  }

  goToAllGroups() {
    this.router.navigate(['/groups'], { state: { showAll: true } });
  }
}
