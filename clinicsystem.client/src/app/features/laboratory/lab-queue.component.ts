import { Component, OnInit } from '@angular/core';
import { FormBuilder, Validators } from '@angular/forms';
import { ApiService } from '../../core/services/api.service';
import { AuthService } from '../../core/services/auth.service';
import { LabRequest, Patient, Visit } from '../../core/models/models';

@Component({
  selector: 'app-lab-queue',
  templateUrl: './lab-queue.component.html'
})
export class LabQueueComponent implements OnInit {
  requests: LabRequest[] = [];
  loading = true;
  selectedRequest: LabRequest | null = null;
  saving = false;
  error = '';
  savedResultId = '';

  selectedFiles: File[] = [];
  uploading = false;
  uploadMessage = '';

  // Add new lab request
  showNewRequestModal = false;
  patients: Patient[] = [];
  patientVisits: Visit[] = [];
  loadingVisits = false;
  savingNewRequest = false;
  newRequestError = '';

  newRequestForm = this.fb.group({
    patientId: ['', Validators.required],
    visitId: ['', Validators.required],
    testType: ['', Validators.required],
    testName: ['', Validators.required],
    notes: ['']
  });

  resultForm = this.fb.group({
    findings: ['', Validators.required],
    notes: [''],
    resultDate: [this.getNowLocal(), Validators.required]
  });

  constructor(private api: ApiService, private fb: FormBuilder, public auth: AuthService) {}

  ngOnInit(): void { this.load(); }

  hasRole(...roles: string[]): boolean { return this.auth.hasRole(...roles); }

  load(): void {
    this.loading = true;
    this.api.getLabPendingRequests().subscribe({
      next: r => { this.requests = r; this.loading = false; },
      error: () => this.loading = false
    });
  }

  openNewRequestModal(): void {
    this.showNewRequestModal = true;
    this.newRequestError = '';
    this.patientVisits = [];
    this.newRequestForm.reset();
    this.api.getPatients(undefined, 1, 100).subscribe(r => this.patients = r.items);
  }

  onPatientChange(): void {
    const patientId = this.newRequestForm.value.patientId;
    if (!patientId) return;
    this.loadingVisits = true;
    this.newRequestForm.patchValue({ visitId: '' });
    this.api.getVisits(undefined, undefined, patientId).subscribe({
      next: visits => { this.patientVisits = visits; this.loadingVisits = false; },
      error: () => this.loadingVisits = false
    });
  }

  submitNewRequest(): void {
    if (this.newRequestForm.invalid) return;
    const { visitId, testType, testName, notes } = this.newRequestForm.value;
    this.savingNewRequest = true;
    this.newRequestError = '';
    this.api.createLabRequest(visitId!, { testType, testName, notes }).subscribe({
      next: () => {
        this.savingNewRequest = false;
        this.showNewRequestModal = false;
        this.load();
      },
      error: err => {
        this.newRequestError = err.error?.message ?? 'Failed to create lab request.';
        this.savingNewRequest = false;
      }
    });
  }

  openResultForm(req: LabRequest): void {
    this.selectedRequest = req;
    this.savedResultId = '';
    this.selectedFiles = [];
    this.uploadMessage = '';
    this.error = '';
    this.resultForm.reset({ findings: '', notes: '', resultDate: this.getNowLocal() });
  }

  saveResult(): void {
    if (!this.selectedRequest || this.resultForm.invalid) return;

    this.saving = true;
    this.error = '';

    this.api.saveLabResult(this.selectedRequest.requestId, this.resultForm.value as any).subscribe({
      next: res => {
        const resultId = (res as any)?.resultId as string | undefined;
        if (!resultId) {
          this.error = 'Result was saved, but no result ID was returned.';
          this.saving = false;
          return;
        }

        this.savedResultId = resultId;
        this.saving = false;

        if (this.selectedFiles.length > 0) {
          this.uploadFilesAfterSave(resultId);
        } else {
          this.closeResultModalAfterSave();
        }
      },
      error: err => {
        this.error = err.error?.message ?? 'Failed to save result.';
        this.saving = false;
      }
    });
  }

  private uploadFilesAfterSave(resultId: string): void {
    this.uploading = true;
    this.uploadMessage = '';

    let done = 0;
    const total = this.selectedFiles.length;

    for (const f of this.selectedFiles) {
      this.api.uploadLabAttachment(resultId, f).subscribe({
        next: () => {
          done++;
          if (done === total) {
            this.uploading = false;
            this.uploadMessage = 'All files uploaded successfully.';
            this.closeResultModalAfterSave();
          }
        },
        error: () => {
          this.uploading = false;
          this.error = 'Result saved, but one or more file uploads failed.';
        }
      });
    }
  }

  private closeResultModalAfterSave(): void {
    this.load();
    this.selectedRequest = null;
    this.selectedFiles = [];
    this.uploadMessage = '';
  }

  onFilesSelected(event: Event): void {
    const input = event.target as HTMLInputElement;
    this.selectedFiles = Array.from(input.files ?? []);
  }

  removeFile(index: number): void {
    this.selectedFiles = this.selectedFiles.filter((_, i) => i !== index);
  }

  uploadFiles(): void {
    if (!this.savedResultId || this.selectedFiles.length === 0) return;

    this.uploading = true;
    this.uploadMessage = '';

    let done = 0;
    for (const f of this.selectedFiles) {
      this.api.uploadLabAttachment(this.savedResultId, f).subscribe({
        next: () => {
          done++;
          if (done === this.selectedFiles.length) {
            this.uploading = false;
            this.uploadMessage = 'All files uploaded successfully.';
            this.selectedFiles = [];
          }
        },
        error: () => {
          this.uploading = false;
          this.error = 'One or more uploads failed.';
        }
      });
    }
  }

  private getNowLocal(): string {
    const d = new Date();
    const pad = (n: number) => n.toString().padStart(2, '0');
    return `${d.getFullYear()}-${pad(d.getMonth() + 1)}-${pad(d.getDate())}T${pad(d.getHours())}:${pad(d.getMinutes())}`;
  }
}
