import { Component, Input, OnInit } from '@angular/core';
import { FormBuilder } from '@angular/forms';
import { ApiService } from '../../core/services/api.service';
import { AuthService } from '../../core/services/auth.service';
import { PatientHistory } from '../../core/models/models';

@Component({
  selector: 'app-history-form',
  templateUrl: './history-form.component.html'
})
export class HistoryFormComponent implements OnInit {
  @Input() visitId!: string;
  @Input() patientId!: string;
  history?: PatientHistory;
  saving = false;
  saved = false;
  error = '';

  historyForm = this.fb.group({
    chiefComplaint: [''], presentIllness: [''], pastMedicalHistory: [''],
    familyHistory: [''], socialHistory: [''], allergies: [''],
    currentMedications: [''], reviewOfSystems: [''], physicalExamination: [''],
    assessment: [''], plan: ['']
  });

  constructor(private fb: FormBuilder, private api: ApiService, public auth: AuthService) {}

  ngOnInit(): void {
    this.api.getHistory(this.visitId).subscribe({
      next: h => { this.history = h; this.historyForm.patchValue(h as any); },
      error: () => {}
    });
  }

  hasRole(...roles: string[]): boolean { return this.auth.hasRole(...roles); }

  save(): void {
    this.saving = true;
    this.error = '';
    this.api.saveHistory(this.visitId, this.historyForm.value as any).subscribe({
      next: h => { this.history = h; this.saving = false; this.saved = true; setTimeout(() => this.saved = false, 3000); },
      error: err => { this.error = err.error?.message ?? 'Failed.'; this.saving = false; }
    });
  }
}
