import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import {
  Patient, CreatePatientRequest, PagedResult,
  Visit, CreateVisitRequest,
  Vitals,
  PatientHistory,
  Prescription,
  LabRequest, LabResult,
  Bill,
  AISuggestion,
  User, RegisterRequest, UpdateUserRequest
} from '../models/models';

@Injectable({ providedIn: 'root' })
export class ApiService {
  constructor(private http: HttpClient) {}

  // ─── Patients ─────────────────────────────────────────────────────────────
  getPatients(search?: string, page = 1, pageSize = 20): Observable<PagedResult<Patient>> {
    let params = new HttpParams().set('page', page).set('pageSize', pageSize);
    if (search) params = params.set('search', search);
    return this.http.get<PagedResult<Patient>>('/api/patients', { params });
  }

  getPatient(id: string): Observable<Patient> {
    return this.http.get<Patient>(`/api/patients/${id}`);
  }

  createPatient(req: CreatePatientRequest): Observable<Patient> {
    return this.http.post<Patient>('/api/patients', req);
  }

  updatePatient(id: string, req: CreatePatientRequest): Observable<void> {
    return this.http.put<void>(`/api/patients/${id}`, req);
  }

  getPatientVisits(patientId: string): Observable<any[]> {
    return this.http.get<any[]>(`/api/patients/${patientId}/visits`);
  }

  // ─── Visits ───────────────────────────────────────────────────────────────
  getVisits(status?: string, date?: string, patientId?: string): Observable<Visit[]> {
    let params = new HttpParams();
    if (status) params = params.set('status', status);
    if (date) params = params.set('date', date);
    if (patientId) params = params.set('patientId', patientId);
    return this.http.get<Visit[]>('/api/visits', { params });
  }

  getVisit(id: string): Observable<Visit> {
    return this.http.get<Visit>(`/api/visits/${id}`);
  }

  createVisit(req: CreateVisitRequest): Observable<any> {
    return this.http.post<any>('/api/visits', req);
  }

  updateVisitStatus(id: string, status: string, doctorId?: string): Observable<any> {
    return this.http.patch<any>(`/api/visits/${id}/status`, { status, doctorId });
  }

  // ─── Vitals ───────────────────────────────────────────────────────────────
  getVitals(visitId: string): Observable<Vitals> {
    return this.http.get<Vitals>(`/api/visits/${visitId}/vitals`);
  }

  saveVitals(visitId: string, vitals: Partial<Vitals>): Observable<Vitals> {
    return this.http.post<Vitals>(`/api/visits/${visitId}/vitals`, vitals);
  }

  // ─── Medical History ──────────────────────────────────────────────────────
  getHistory(visitId: string): Observable<PatientHistory> {
    return this.http.get<PatientHistory>(`/api/visits/${visitId}/history`);
  }

  saveHistory(visitId: string, history: Partial<PatientHistory>): Observable<PatientHistory> {
    return this.http.post<PatientHistory>(`/api/visits/${visitId}/history`, history);
  }

  // ─── Prescriptions ────────────────────────────────────────────────────────
  getPrescriptions(visitId: string): Observable<Prescription[]> {
    return this.http.get<Prescription[]>(`/api/visits/${visitId}/prescriptions`);
  }

  createPrescription(visitId: string, data: any): Observable<Prescription> {
    return this.http.post<Prescription>(`/api/visits/${visitId}/prescriptions`, data);
  }

  deletePrescription(visitId: string, prescriptionId: string): Observable<void> {
    return this.http.delete<void>(`/api/visits/${visitId}/prescriptions/${prescriptionId}`);
  }

  // ─── Lab ──────────────────────────────────────────────────────────────────
  getLabRequests(visitId: string): Observable<LabRequest[]> {
    return this.http.get<LabRequest[]>(`/api/visits/${visitId}/lab-requests`);
  }

  getPendingLabRequests(status?: string): Observable<LabRequest[]> {
    let params = new HttpParams();
    if (status) params = params.set('status', status);
    return this.http.get<LabRequest[]>('/api/lab-requests', { params });
  }

  getLabPendingRequests(status?: string): Observable<LabRequest[]> {
    return this.getPendingLabRequests(status);
  }

  createLabRequest(visitId: string, data: any): Observable<any> {
    return this.http.post<any>(`/api/visits/${visitId}/lab-requests`, data);
  }

  saveLabResult(requestId: string, data: any): Observable<LabResult> {
    return this.http.post<LabResult>(`/api/lab-requests/${requestId}/results`, data);
  }

  uploadLabAttachment(resultId: string, file: File): Observable<any> {
    const form = new FormData();
    form.append('file', file);
    return this.http.post<any>(`/api/lab-results/${resultId}/attachments`, form);
  }

  deleteLabAttachment(resultId: string, attachmentId: string): Observable<void> {
    return this.http.delete<void>(`/api/lab-results/${resultId}/attachments/${attachmentId}`);
  }

  getAttachmentUrl(resultId: string, attachmentId: string): string {
    return `/api/lab-results/${resultId}/attachments/${attachmentId}`;
  }

  getLabAttachmentBlob(resultId: string, attachmentId: string): Observable<Blob> {
    return this.http.get(this.getAttachmentUrl(resultId, attachmentId), { responseType: 'blob' });
  }

  // ─── Billing ──────────────────────────────────────────────────────────────
  getBill(visitId: string): Observable<Bill> {
    return this.http.get<Bill>(`/api/visits/${visitId}/bill`);
  }

  getBillByVisit(visitId: string): Observable<Bill> {
    return this.getBill(visitId);
  }

  getBills(status?: string): Observable<Bill[]> {
    let params = new HttpParams();
    if (status) params = params.set('status', status);
    return this.http.get<Bill[]>('/api/bills', { params });
  }

  createBill(visitId: string, data: any): Observable<Bill> {
    return this.http.post<Bill>(`/api/visits/${visitId}/bill`, data);
  }

  addBillItem(billId: string, item: any): Observable<any> {
    return this.http.post<any>(`/api/bills/${billId}/items`, item);
  }

  removeBillItem(billId: string, itemId: string): Observable<void> {
    return this.http.delete<void>(`/api/bills/${billId}/items/${itemId}`);
  }

  deleteBillItem(billId: string, itemId: string): Observable<void> {
    return this.removeBillItem(billId, itemId);
  }

  recordPayment(billId: string, amount: number, notes?: string): Observable<any> {
    return this.http.patch<any>(`/api/bills/${billId}/payment`, { amount, notes });
  }

  recordBillPayment(billId: string, data: { amount: number; notes?: string }): Observable<any> {
    return this.recordPayment(billId, data.amount, data.notes);
  }

  // ─── AI ───────────────────────────────────────────────────────────────────
  getAISuggestions(visitId: string): Observable<AISuggestion[]> {
    return this.http.get<AISuggestion[]>(`/api/visits/${visitId}/ai-suggestions`);
  }

  generateAISuggestion(
    visitId: string,
    data: { additionalContext?: string } | string | undefined
  ): Observable<AISuggestion> {
    const payload = typeof data === 'string' ? { additionalContext: data } : (data ?? {});
    return this.http.post<AISuggestion>(`/api/visits/${visitId}/ai-suggestions`, payload);
  }

  // ─── Users (Admin) ────────────────────────────────────────────────────────
  getUsers(): Observable<User[]> {
    return this.http.get<User[]>('/api/users');
  }

  getUserById(id: string): Observable<User> {
    return this.http.get<User>(`/api/users/${id}`);
  }

  registerUser(req: RegisterRequest): Observable<User> {
    return this.http.post<User>('/api/auth/register', req);
  }

  updateUser(id: string, req: UpdateUserRequest): Observable<User> {
    return this.http.put<User>(`/api/users/${id}`, req);
  }

  toggleUserActive(id: string): Observable<any> {
    return this.http.patch<any>(`/api/users/${id}/toggle-active`, {});
  }
}
