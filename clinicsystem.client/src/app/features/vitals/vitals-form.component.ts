import { Component, OnInit } from '@angular/core';
import { FormBuilder } from '@angular/forms';
import { ActivatedRoute } from '@angular/router';
import { ApiService } from '../../core/services/api.service';

@Component({
  selector: 'app-vitals-form',
  templateUrl: './vitals-form.component.html'
})
export class VitalsFormComponent implements OnInit {
  visitId!: string;
  saving = false;
  saved = false;
  error = '';

  vitalsForm = this.fb.group({
    bloodPressure: [''],
    heartRate: [null as number | null],
    temperature: [null as number | null],
    weight: [null as number | null],
    height: [null as number | null],
    oxygenSaturation: [null as number | null],
    respiratoryRate: [null as number | null],
    notes: ['']
  });

  constructor(private fb: FormBuilder, private api: ApiService, private route: ActivatedRoute) {}

  ngOnInit(): void {
    this.visitId = this.route.snapshot.paramMap.get('visitId')!;
    this.api.getVitals(this.visitId).subscribe({
      next: v => this.vitalsForm.patchValue(v as any),
      error: () => {}
    });
  }

  get computedBmi(): number | null {
    const w = this.vitalsForm.value.weight;
    const h = this.vitalsForm.value.height;
    if (w && h && h > 0) {
      const hm = h / 100;
      return Math.round((w / (hm * hm)) * 10) / 10;
    }
    return null;
  }

  save(): void {
    this.saving = true;
    this.error = '';
    this.api.saveVitals(this.visitId, this.vitalsForm.value as any).subscribe({
      next: () => { this.saving = false; this.saved = true; setTimeout(() => this.saved = false, 3000); },
      error: err => { this.error = err.error?.message ?? 'Failed to save.'; this.saving = false; }
    });
  }
}
