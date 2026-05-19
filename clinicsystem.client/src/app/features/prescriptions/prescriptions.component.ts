import { Component, Input, OnInit } from '@angular/core';
import { FormBuilder } from '@angular/forms';
import { ApiService } from '../../core/services/api.service';
import { AuthService } from '../../core/services/auth.service';
import { Prescription } from '../../core/models/models';

@Component({
  selector: 'app-prescriptions',
  templateUrl: './prescriptions.component.html'
})
export class PrescriptionsComponent implements OnInit {
  @Input() visitId!: string;
  @Input() patientId!: string;

  prescriptions: Prescription[] = [];
  showForm = false;
  saving = false;
  error = '';
  selectedForPrint?: Prescription;

  rxForm = this.fb.group({ instructions: [''] });
  rxItems: any[] = [{ medication: '', dosage: '', frequency: '', duration: '', instructions: '' }];

  constructor(private fb: FormBuilder, private api: ApiService, public auth: AuthService) {}

  ngOnInit(): void { this.load(); }

  hasRole(...roles: string[]): boolean { return this.auth.hasRole(...roles); }

  load(): void {
    this.api.getPrescriptions(this.visitId).subscribe(r => this.prescriptions = r);
  }

  addItem(): void { this.rxItems.push({ medication: '', dosage: '', frequency: '', duration: '', instructions: '' }); }
  removeItem(i: number): void { this.rxItems.splice(i, 1); }

  createPrescription(): void {
    if (this.rxItems.some(i => !i.medication?.trim())) {
      this.error = 'Medication name is required for all items.';
      return;
    }

    this.saving = true;
    this.error = '';

    this.api.createPrescription(this.visitId, {
      instructions: this.rxForm.value.instructions,
      items: this.rxItems
    }).subscribe({
      next: () => {
        this.saving = false;
        this.showForm = false;
        this.rxForm.reset();
        this.rxItems = [{ medication: '', dosage: '', frequency: '', duration: '', instructions: '' }];
        this.load();
      },
      error: err => {
        this.error = err.error?.message ?? 'Failed to create prescription.';
        this.saving = false;
      }
    });
  }

  deletePrescription(id: string): void {
    if (!confirm('Delete this prescription?')) return;
    this.api.deletePrescription(this.visitId, id).subscribe(() => this.load());
  }

  printPrescription(rx: Prescription): void {
    this.selectedForPrint = rx;
    setTimeout(() => window.print(), 100);
  }
}
