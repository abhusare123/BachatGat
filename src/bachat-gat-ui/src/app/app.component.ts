import { Component } from '@angular/core';
import { RouterOutlet, RouterLink, Router } from '@angular/router';
import { CommonModule } from '@angular/common';
import { MatToolbarModule } from '@angular/material/toolbar';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatMenuModule } from '@angular/material/menu';
import { MatDividerModule } from '@angular/material/divider';
import { AuthService } from './core/auth.service';

@Component({
  selector: 'app-root',
  imports: [
    RouterOutlet, RouterLink, CommonModule,
    MatToolbarModule, MatButtonModule, MatIconModule, MatMenuModule, MatDividerModule
  ],
  templateUrl: './app.component.html',
  styleUrl: './app.component.scss'
})
export class AppComponent {
  constructor(public auth: AuthService, private router: Router) {}

  get currentGroupId(): number | null {
    const match = this.router.url.match(/\/groups\/(\d+)/);
    return match ? +match[1] : null;
  }
}
