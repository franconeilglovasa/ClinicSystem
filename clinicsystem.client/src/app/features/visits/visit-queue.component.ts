import { Component, OnInit } from '@angular/core';
import { FormBuilder } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { ApiService } from '../../core/services/api.service';
import { AuthService } from '../../core/services/auth.service';
import { Visit, Patient } from '../../core/models/models';
import { Subject, debounceTime, distinctUntilChanged } from 'rxjs';

@Component({
  selector: 'app-visit-queue',
  templateUrl: './visit-queue.component.html'
})
export class VisitQueueComponent implements OnInit {
  visits: Visit[] = [];
  loading = true;
  filterStatus = '';
  statuses = ['', 'Waiting', 'WithNurse', 'WithDoctor', 'ForLaboratory', 'ForBilling', 'Completed'];

  showNewVisit = false;
  patientSearch = '';
  patientSearchResults: Patient[] = [];
  selectedPatient: Patient | null = null;
  saving = false;
  formError = '';

  private searchSubject = new Subject<string>();

  visitForm = this.fb.group({ chiefComplaint: [''] });

  constructor(
    public auth: AuthService,
    private api: ApiService,
    private fb: FormBuilder,
    private router: Router,
    private route: ActivatedRoute
  ) {}

  ngOnInit(): void {
    this.loadVisits();
    this.searchSubject.pipe(debounceTime(300), distinctUntilChanged()).subscribe(s => {
      if (s.length >= 2) this.api.getPatients(s).subscribe(r => this.patientSearchResults = r.items);
      else this.patientSearchResults = [];
    });
  }

  loadVisits(): void {
    this.loading = true;
    this.api.getVisits(this.filterStatus || undefined).subscribe({
      next: v => { this.visits = v; this.loading = false; },
      error: () => this.loading = false
    });
  }

  searchPatients(): void { this.searchSubject.next(this.patientSearch); }

  selectPatient(p: Patient): void {
    this.selectedPatient = p;
    this.patientSearch = p.fullName;
    this.patientSearchResults = [];
  }

  createVisit(): void {
    if (!this.selectedPatient) return;
    this.saving = true;
    this.api.createVisit({
      patientId: this.selectedPatient.patientId,
      chiefComplaint: this.visitForm.value.chiefComplaint ?? undefined
    }).subscribe({
      next: res => {
        this.showNewVisit = false;
        this.saving = false;
        this.selectedPatient = null;
        this.patientSearch = '';
        this.visitForm.reset();
        this.loadVisits();
      },
      error: err => { this.formError = err.error?.message ?? 'Failed.'; this.saving = false; }
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
