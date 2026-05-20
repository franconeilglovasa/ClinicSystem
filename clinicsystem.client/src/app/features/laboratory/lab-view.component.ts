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
  private objectUrl: string | null = null;

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
      if (this.objectUrl) {
        URL.revokeObjectURL(this.objectUrl);
      }

      const objectUrl = URL.createObjectURL(blob);
      this.objectUrl = objectUrl;
      this.previewUrl = this.sanitizer.bypassSecurityTrustResourceUrl(objectUrl);
      this.previewFileName = attachment.fileName;
      this.isImagePreview = this.isImageAttachment(attachment, blob.type);
    });
  }

  openAttachmentInNewTab(resultId: string, attachment: LabAttachment): void {
    this.api.getLabAttachmentBlob(resultId, attachment.attachmentId).subscribe(blob => {
      const objectUrl = URL.createObjectURL(blob);
      window.open(objectUrl, '_blank', 'noopener,noreferrer');

      // Revoke later to give the new tab enough time to load the blob URL.
      window.setTimeout(() => URL.revokeObjectURL(objectUrl), 60_000);
    });
  }

  closePreview(): void {
    if (this.objectUrl) {
      URL.revokeObjectURL(this.objectUrl);
      this.objectUrl = null;
    }

    this.previewUrl = null;
    this.previewFileName = '';
  }

  private isImageAttachment(attachment: LabAttachment, mimeType?: string): boolean {
    if (mimeType?.startsWith('image/')) return true;

    const fileType = (attachment.fileType ?? '').toLowerCase();
    return fileType === '.jpg' || fileType === '.jpeg' || fileType === '.png' || fileType === '.gif' || fileType === '.webp';
  }

  formatFileSize(bytes: number): string {
    if (!bytes) return '0 B';
    const sizes = ['B', 'KB', 'MB', 'GB'];
    const i = Math.floor(Math.log(bytes) / Math.log(1024));
    return `${(bytes / Math.pow(1024, i)).toFixed(1)} ${sizes[i]}`;
  }
}
