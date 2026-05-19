import { Component, OnInit } from '@angular/core';
import { FormBuilder, Validators } from '@angular/forms';
import { ApiService } from '../../core/services/api.service';
import { LabRequest } from '../../core/models/models';

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

  resultForm = this.fb.group({
    findings: ['', Validators.required],
    notes: [''],
    resultDate: [this.getNowLocal(), Validators.required]
  });

  constructor(private api: ApiService, private fb: FormBuilder) {}

  ngOnInit(): void { this.load(); }

  load(): void {
    this.loading = true;
    this.api.getLabPendingRequests().subscribe({
      next: r => { this.requests = r; this.loading = false; },
      error: () => this.loading = false
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
        this.savedResultId = res.resultId;
        this.saving = false;
        this.load();
      },
      error: err => {
        this.error = err.error?.message ?? 'Failed to save result.';
        this.saving = false;
      }
    });
  }

  onFilesSelected(event: Event): void {
    const input = event.target as HTMLInputElement;
    this.selectedFiles = Array.from(input.files ?? []);
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
