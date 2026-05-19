namespace ClinicSystem.Server.DTOs.Visits
{
    public class VisitDto
    {
        public Guid VisitId { get; set; }
        public Guid PatientId { get; set; }
        public string PatientName { get; set; } = string.Empty;
        public DateTime VisitDate { get; set; }
        public string Status { get; set; } = string.Empty;
        public string? ChiefComplaint { get; set; }
        public string? NurseId { get; set; }
        public string? NurseName { get; set; }
        public string? DoctorId { get; set; }
        public string? DoctorName { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool HasVitals { get; set; }
        public bool HasHistory { get; set; }
        public bool HasLabRequests { get; set; }
        public bool HasBill { get; set; }
    }

    public class CreateVisitRequest
    {
        public Guid PatientId { get; set; }
        public string? ChiefComplaint { get; set; }
        public string? DoctorId { get; set; }
    }

    public class UpdateVisitStatusRequest
    {
        public string Status { get; set; } = string.Empty;
        public string? DoctorId { get; set; }
    }

    public class VisitSummaryDto
    {
        public VisitDto Visit { get; set; } = new();
        public object? Vitals { get; set; }
        public object? PatientHistory { get; set; }
        public List<object> LabRequests { get; set; } = new();
        public List<object> Prescriptions { get; set; } = new();
    }
}
