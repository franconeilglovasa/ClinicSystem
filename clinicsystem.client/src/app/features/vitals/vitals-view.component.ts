import { Component, Input, OnInit } from '@angular/core';
import { ApiService } from '../../core/services/api.service';
import { Vitals } from '../../core/models/models';

@Component({
  selector: 'app-vitals-view',
  templateUrl: './vitals-view.component.html'
})
export class VitalsViewComponent implements OnInit {
  @Input() visitId!: string;
  @Input() patientId!: string;
  vitals?: Vitals;
  loading = true;

  constructor(private api: ApiService) {}

  ngOnInit(): void {
    this.api.getVitals(this.visitId).subscribe({
      next: v => { this.vitals = v; this.loading = false; },
      error: () => this.loading = false
    });
  }
}
