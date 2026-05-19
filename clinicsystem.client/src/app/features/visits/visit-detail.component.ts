import { Component, OnInit } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { ApiService } from '../../core/services/api.service';
import { AuthService } from '../../core/services/auth.service';
import { Visit } from '../../core/models/models';

@Component({
  selector: 'app-visit-detail',
  templateUrl: './visit-detail.component.html'
})
export class VisitDetailComponent implements OnInit {
  visit?: Visit;
  loading = true;
  activeTab = 'vitals';
  newStatus = '';

  tabs = [
    { id: 'vitals', label: '💓 Vitals' },
    { id: 'history', label: '📋 Medical History' },
    { id: 'prescriptions', label: '💊 Prescriptions' },
    { id: 'lab', label: '🔬 Laboratory' },
    { id: 'ai', label: '🤖 AI Suggestions' },
    { id: 'billing', label: '💰 Billing' },
  ];

  constructor(private route: ActivatedRoute, private api: ApiService, public auth: AuthService) {}

  ngOnInit(): void {
    const id = this.route.snapshot.paramMap.get('visitId')!;
    this.api.getVisit(id).subscribe({
      next: v => { this.visit = v; this.newStatus = v.status; this.loading = false; },
      error: () => this.loading = false
    });
  }

  updateStatus(): void {
    if (!this.visit) return;
    this.api.updateVisitStatus(this.visit.visitId, this.newStatus).subscribe(() => {
      if (this.visit) this.visit.status = this.newStatus;
    });
  }

  hasRole(...roles: string[]): boolean { return this.auth.hasRole(...roles); }

  getStatusClass(status: string): string {
    const map: Record<string, string> = {
      Waiting: 'badge-waiting', WithNurse: 'badge-withnurse', WithDoctor: 'badge-withdoctor',
      ForLaboratory: 'badge-forlab', ForBilling: 'badge-forbilling', Completed: 'badge-completed'
    };
    return map[status] ?? 'badge-waiting';
  }
}
