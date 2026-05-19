namespace ClinicSystem.Server.DTOs.Prescriptions
{
    public class PrescriptionItemDto
    {
        public Guid ItemId { get; set; }
        public string Medication { get; set; } = string.Empty;
        public string? Dosage { get; set; }
        public string? Frequency { get; set; }
        public string? Duration { get; set; }
        public string? Instructions { get; set; }
    }

    public class PrescriptionDto
    {
        public Guid PrescriptionId { get; set; }
        public Guid VisitId { get; set; }
        public Guid PatientId { get; set; }
        public string PatientName { get; set; } = string.Empty;
        public int PatientAge { get; set; }
        public string PatientGender { get; set; } = string.Empty;
        public string? DoctorId { get; set; }
        public string? DoctorName { get; set; }
        public string? DoctorSpecialty { get; set; }
        public string? DoctorLicense { get; set; }
        public DateTime Date { get; set; }
        public string? Instructions { get; set; }
        public List<PrescriptionItemDto> Items { get; set; } = new();
        public DateTime CreatedAt { get; set; }
    }

    public class CreatePrescriptionRequest
    {
        public string? Instructions { get; set; }
        public List<CreatePrescriptionItemRequest> Items { get; set; } = new();
    }

    public class CreatePrescriptionItemRequest
    {
        public string Medication { get; set; } = string.Empty;
        public string? Dosage { get; set; }
        public string? Frequency { get; set; }
        public string? Duration { get; set; }
        public string? Instructions { get; set; }
    }
}
