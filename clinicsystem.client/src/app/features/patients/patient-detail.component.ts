import { Component, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { ApiService } from '../../core/services/api.service';
import { Patient } from '../../core/models/models';

@Component({
  selector: 'app-patient-detail',
  templateUrl: './patient-detail.component.html'
})
export class PatientDetailComponent implements OnInit {
  patient?: Patient;
  visits: any[] = [];
  loading = true;

  constructor(private route: ActivatedRoute, private router: Router, private api: ApiService) {}

  ngOnInit(): void {
    const id = this.route.snapshot.paramMap.get('id')!;
    this.api.getPatient(id).subscribe({
      next: p => {
        this.patient = p;
        this.api.getPatientVisits(id).subscribe(v => { this.visits = v; this.loading = false; });
      },
      error: () => this.loading = false
    });
  }

  goToVisit(visitId: string): void {
    this.router.navigate(['/app/visits', visitId]);
  }

  getStatusClass(status: string): string {
    const map: Record<string, string> = {
      Waiting: 'badge-waiting', WithNurse: 'badge-withnurse', WithDoctor: 'badge-withdoctor',
      ForLaboratory: 'badge-forlab', ForBilling: 'badge-forbilling', Completed: 'badge-completed'
    };
    return map[status] ?? 'badge-waiting';
  }
}
