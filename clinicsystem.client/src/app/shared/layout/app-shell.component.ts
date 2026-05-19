import { Component, OnInit } from '@angular/core';
import { Router, NavigationEnd } from '@angular/router';
import { AuthService } from '../../core/services/auth.service';
import { filter } from 'rxjs/operators';

@Component({
  selector: 'app-shell',
  templateUrl: './app-shell.component.html'
})
export class AppShellComponent implements OnInit {
  currentUser = this.auth.getCurrentUser();
  today = new Date();
  pageTitle = 'Dashboard';

  private titleMap: Record<string, string> = {
    'dashboard': 'Dashboard',
    'patients': 'Patients',
    'queue': 'Visit Queue',
    'lab-queue': 'Laboratory Queue',
    'billing': 'Billing',
    'admin/users': 'User Management',
  };

  constructor(public auth: AuthService, private router: Router) {}

  ngOnInit(): void {
    this.router.events.pipe(filter(e => e instanceof NavigationEnd)).subscribe((e: any) => {
      const segments = e.urlAfterRedirects.split('/').filter(Boolean);
      const key = segments.slice(1).join('/');
      this.pageTitle = this.titleMap[key] ?? this.titleMap[segments[segments.length - 1]] ?? 'Clinic System';
    });
  }

  get userInitial(): string {
    return (this.currentUser?.fullName?.[0] ?? 'U').toUpperCase();
  }

  hasRole(...roles: string[]): boolean {
    return this.auth.hasRole(...roles);
  }

  logout(): void {
    this.auth.logout();
  }
}
