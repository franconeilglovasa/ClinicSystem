import { Component, Input, OnInit } from '@angular/core';
import { FormBuilder, Validators } from '@angular/forms';
import { DomSanitizer, SafeResourceUrl } from '@angular/platform-browser';
import { ApiService } from '../../core/services/api.service';
import { AuthService } from '../../core/services/auth.service';
import { LabAttachment, LabRequest } from '../../core/models/models';

@Component({
  selector: 'app-lab-view',
  templateUrl: './lab-view.component.html'
})
export class LabViewComponent implements OnInit {
  @Input() visitId!: string;
  @Input() patientId!: string;

  requests: LabRequest[] = [];
  showRequestForm = false;
  savingRequest = false;

  previewUrl: SafeResourceUrl | null = null;
  previewFileName = '';
  isImagePreview = true;

  requestForm = this.fb.group({
    testType: ['', Validators.required],
    testName: ['', Validators.required],
    notes: ['']
  });

  constructor(
    private fb: FormBuilder,
    private api: ApiService,
    private sanitizer: DomSanitizer,
    public auth: AuthService
  ) {}

  ngOnInit(): void { this.load(); }

  hasRole(...roles: string[]): boolean { return this.auth.hasRole(...roles); }

  load(): void {
    this.api.getLabRequests(this.visitId).subscribe(r => this.requests = r);
  }

  createRequest(): void {
    if (this.requestForm.invalid) return;
    this.savingRequest = true;
    this.api.createLabRequest(this.visitId, this.requestForm.value as any).subscribe({
      next: () => {
        this.savingRequest = false;
        this.showRequestForm = false;
        this.requestForm.reset();
        this.load();
      },
      error: () => this.savingRequest = false
    });
  }

  previewAttachment(resultId: string, attachment: LabAttachment): void {
    this.api.getLabAttachmentBlob(resultId, attachment.attachmentId).subscribe(blob => {
      const objectUrl = URL.createObjectURL(blob);
      this.previewUrl = this.sanitizer.bypassSecurityTrustResourceUrl(objectUrl);
      this.previewFileName = attachment.fileName;
      this.isImagePreview = attachment.fileType?.startsWith('image/');
    });
  }

  closePreview(): void {
    this.previewUrl = null;
    this.previewFileName = '';
  }

  formatFileSize(bytes: number): string {
    if (!bytes) return '0 B';
    const sizes = ['B', 'KB', 'MB', 'GB'];
    const i = Math.floor(Math.log(bytes) / Math.log(1024));
    return `${(bytes / Math.pow(1024, i)).toFixed(1)} ${sizes[i]}`;
  }
}
