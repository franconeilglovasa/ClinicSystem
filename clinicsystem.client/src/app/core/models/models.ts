export interface LoginRequest {
  email: string;
  password: string;
}

export interface LoginResponse {
  token: string;
  userId: string;
  fullName: string;
  email: string;
  role: string;
  expiresAt: string;
}

export interface User {
  id: string;
  fullName: string;
  email: string;
  role: string;
  specialty?: string;
  licenseNumber?: string;
  isActive: boolean;
  createdAt: string;
}

export interface RegisterRequest {
  fullName: string;
  email: string;
  password: string;
  role: string;
  specialty?: string;
  licenseNumber?: string;
}

export interface UpdateUserRequest {
  fullName: string;
  specialty?: string;
  licenseNumber?: string;
  isActive: boolean;
}

export interface Patient {
  patientId: string;
  firstName: string;
  lastName: string;
  fullName: string;
  dateOfBirth: string;
  age: number;
  gender: string;
  contactNumber?: string;
  address?: string;
  email?: string;
  createdAt: string;
}

export interface CreatePatientRequest {
  firstName: string;
  lastName: string;
  dateOfBirth: string;
  gender: string;
  contactNumber?: string;
  address?: string;
  email?: string;
}

export interface PagedResult<T> {
  items: T[];
  totalCount: number;
  page: number;
  pageSize: number;
  totalPages: number;
}

export interface Visit {
  visitId: string;
  patientId: string;
  patientName: string;
  visitDate: string;
  status: string;
  chiefComplaint?: string;
  nurseId?: string;
  nurseName?: string;
  doctorId?: string;
  doctorName?: string;
  createdAt: string;
  hasVitals: boolean;
  hasHistory: boolean;
  hasLabRequests: boolean;
  hasBill: boolean;
}

export interface CreateVisitRequest {
  patientId: string;
  chiefComplaint?: string;
  doctorId?: string;
}

export interface Vitals {
  vitalsId: string;
  visitId: string;
  bloodPressure?: string;
  heartRate?: number;
  temperature?: number;
  weight?: number;
  height?: number;
  bmi?: number;
  oxygenSaturation?: number;
  respiratoryRate?: number;
  notes?: string;
  recordedByNurseId?: string;
  recordedByNurseName?: string;
  recordedAt: string;
}

export interface PatientHistory {
  historyId: string;
  visitId: string;
  patientId: string;
  chiefComplaint?: string;
  presentIllness?: string;
  pastMedicalHistory?: string;
  familyHistory?: string;
  socialHistory?: string;
  allergies?: string;
  currentMedications?: string;
  reviewOfSystems?: string;
  physicalExamination?: string;
  assessment?: string;
  plan?: string;
  doctorId?: string;
  doctorName?: string;
  createdAt: string;
  updatedAt?: string;
}

export interface PrescriptionItem {
  itemId: string;
  medication: string;
  dosage?: string;
  frequency?: string;
  duration?: string;
  instructions?: string;
}

export interface Prescription {
  prescriptionId: string;
  visitId: string;
  patientId: string;
  patientName: string;
  patientAge: number;
  patientGender: string;
  doctorId?: string;
  doctorName?: string;
  doctorSpecialty?: string;
  doctorLicense?: string;
  date: string;
  instructions?: string;
  items: PrescriptionItem[];
  createdAt: string;
}

export interface LabAttachment {
  attachmentId: string;
  fileName: string;
  fileType: string;
  fileSize: number;
  uploadedAt: string;
}

export interface LabResult {
  resultId: string;
  requestId: string;
  labTechId?: string;
  labTechName?: string;
  findings?: string;
  notes?: string;
  resultDate: string;
  createdAt: string;
  attachments: LabAttachment[];
}

export interface LabRequest {
  requestId: string;
  visitId: string;
  patientId: string;
  patientName: string;
  requestedByDoctorId?: string;
  requestedByDoctorName?: string;
  testType: string;
  testName: string;
  notes?: string;
  status: string;
  requestedAt: string;
  result?: LabResult;
}

export interface BillItem {
  itemId: string;
  description: string;
  category: string;
  unitPrice: number;
  quantity: number;
  totalPrice: number;
}

export interface Bill {
  billId: string;
  visitId: string;
  patientId: string;
  patientName: string;
  cashierId?: string;
  cashierName?: string;
  totalAmount: number;
  paidAmount: number;
  balance: number;
  status: string;
  notes?: string;
  createdAt: string;
  paidAt?: string;
  items: BillItem[];
}

export interface AISuggestion {
  suggestionId: string;
  visitId: string;
  response?: string;
  model: string;
  requestedByDoctorName?: string;
  generatedAt: string;
}
