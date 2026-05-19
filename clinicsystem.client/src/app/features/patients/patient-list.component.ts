import { Component, OnInit } from '@angular/core';
import { FormBuilder, Validators } from '@angular/forms';
import { HttpErrorResponse } from '@angular/common/http';
import { ApiService } from '../../core/services/api.service';
import { Patient, PagedResult } from '../../core/models/models';
import { Subject, debounceTime, distinctUntilChanged } from 'rxjs';

@Component({
  selector: 'app-patient-list',
  templateUrl: './patient-list.component.html'
})
export class PatientListComponent implements OnInit {
  patients: Patient[] = [];
  result?: PagedResult<Patient>;
  loading = true;
  search = '';
  currentPage = 1;
  showForm = false;
  editingPatient: Patient | null = null;
  saving = false;
  formError = '';

  private searchSubject = new Subject<string>();

  patientForm = this.fb.group({
    firstName: ['', Validators.required],
    lastName: ['', Validators.required],
    dateOfBirth: ['', Validators.required],
    gender: ['', Validators.required],
    contactNumber: [''],
    address: [''],
    email: ['']
  });

  constructor(private api: ApiService, private fb: FormBuilder) {}

  ngOnInit(): void {
    this.loadPatients();
    this.searchSubject.pipe(debounceTime(300), distinctUntilChanged())
      .subscribe(s => { this.search = s; this.currentPage = 1; this.loadPatients(); });
  }

  loadPatients(): void {
    this.loading = true;
    this.api.getPatients(this.search, this.currentPage).subscribe({
      next: r => { this.result = r; this.patients = r.items; this.loading = false; },
      error: () => this.loading = false
    });
  }

  onSearch(): void { this.searchSubject.next(this.search); }

  goPage(page: number): void {
    this.currentPage = page;
    this.loadPatients();
  }

  edit(p: Patient): void {
    this.editingPatient = p;
    const dob = new Date(p.dateOfBirth).toISOString().substring(0, 10);
    this.patientForm.patchValue({ ...p, dateOfBirth: dob });
    this.showForm = true;
  }

  resetForm(): void {
    this.patientForm.reset();
    this.formError = '';
  }

  save(): void {
    if (this.patientForm.invalid) return;
    this.saving = true;
    this.formError = '';
    const val = this.patientForm.value as any;

    if (this.editingPatient) {
      this.api.updatePatient(this.editingPatient.patientId, val).subscribe({
        next: () => {
          this.showForm = false;
          this.saving = false;
          this.loadPatients();
        },
        error: (err: HttpErrorResponse) => {
          this.formError = err.error?.message ?? 'Failed to save.';
          this.saving = false;
        }
      });
      return;
    }

    this.api.createPatient(val).subscribe({
      next: () => {
        this.showForm = false;
        this.saving = false;
        this.loadPatients();
      },
      error: (err: HttpErrorResponse) => {
        this.formError = err.error?.message ?? 'Failed to save.';
        this.saving = false;
      }
    });
  }
}
