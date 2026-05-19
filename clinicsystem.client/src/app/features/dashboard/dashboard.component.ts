import { Component, OnInit } from '@angular/core';
import { AuthService } from '../../core/services/auth.service';
import { ApiService } from '../../core/services/api.service';
import { Visit } from '../../core/models/models';

@Component({
  selector: 'app-dashboard',
  templateUrl: './dashboard.component.html'
})
export class DashboardComponent implements OnInit {
  user = this.auth.getCurrentUser();
  todayVisits: Visit[] = [];
  loading = true;
  today = new Date();

  constructor(public auth: AuthService, private api: ApiService) {}

  ngOnInit(): void {
    this.api.getVisits().subscribe({
      next: v => { this.todayVisits = v; this.loading = false; },
      error: () => this.loading = false
    });
  }

  get waiting(): number { return this.todayVisits.filter(v => v.status === 'Waiting').length; }
  get withDoctor(): number { return this.todayVisits.filter(v => v.status === 'WithDoctor').length; }
  get completed(): number { return this.todayVisits.filter(v => v.status === 'Completed').length; }

  hasRole(...roles: string[]): boolean { return this.auth.hasRole(...roles); }

  getStatusClass(status: string): string {
    const map: Record<string, string> = {
      Waiting: 'badge-waiting', WithNurse: 'badge-withnurse', WithDoctor: 'badge-withdoctor',
      ForLaboratory: 'badge-forlab', ForBilling: 'badge-forbilling', Completed: 'badge-completed'
    };
    return map[status] ?? 'badge-waiting';
  }
}
