import { Component, OnInit, signal } from '@angular/core';
import { ActivatedRoute, Router, RouterOutlet, RouterLink, RouterLinkActive } from '@angular/router';
import { CommonModule } from '@angular/common';
import { MatIconModule } from '@angular/material/icon';
import { MatListModule } from '@angular/material/list';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { GroupService } from '../../core/group.service';

interface NavItem {
  label: string;
  labelMr: string;
  icon: string;
  path: string;
}

@Component({
  selector: 'app-group-shell',
  imports: [
    CommonModule, RouterOutlet, RouterLink, RouterLinkActive,
    MatIconModule, MatListModule, MatProgressSpinnerModule
  ],
  templateUrl: './group-shell.component.html',
  styleUrl: './group-shell.component.scss'
})
export class GroupShellComponent implements OnInit {
  groupId!: number;
  groupName = signal('');

  navItems: NavItem[] = [
    { label: 'Reports',       labelMr: 'अहवाल',  icon: 'bar_chart',       path: 'reports'       },
    { label: 'Contributions', labelMr: 'बचत',    icon: 'savings',         path: 'contributions' },
    { label: 'Loans',         labelMr: 'कर्ज',   icon: 'account_balance', path: 'loans'         },
    { label: 'Expenses',      labelMr: 'खर्च',   icon: 'receipt_long',    path: 'expenses'      },
    { label: 'Penalties',     labelMr: 'दंड',    icon: 'gavel',           path: 'penalties'     },
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
