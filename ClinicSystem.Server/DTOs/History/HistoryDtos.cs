namespace ClinicSystem.Server.DTOs.History
{
    public class PatientHistoryDto
    {
        public Guid HistoryId { get; set; }
        public Guid VisitId { get; set; }
        public Guid PatientId { get; set; }
        public string? ChiefComplaint { get; set; }
        public string? PresentIllness { get; set; }
        public string? PastMedicalHistory { get; set; }
        public string? FamilyHistory { get; set; }
        public string? SocialHistory { get; set; }
        public string? Allergies { get; set; }
        public string? CurrentMedications { get; set; }
        public string? ReviewOfSystems { get; set; }
        public string? PhysicalExamination { get; set; }
        public string? Assessment { get; set; }
        public string? Plan { get; set; }
        public string? DoctorId { get; set; }
        public string? DoctorName { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }

    public class SavePatientHistoryRequest
    {
        public string? ChiefComplaint { get; set; }
        public string? PresentIllness { get; set; }
        public string? PastMedicalHistory { get; set; }
        public string? FamilyHistory { get; set; }
        public string? SocialHistory { get; set; }
        public string? Allergies { get; set; }
        public string? CurrentMedications { get; set; }
        public string? ReviewOfSystems { get; set; }
        public string? PhysicalExamination { get; set; }
        public string? Assessment { get; set; }
        public string? Plan { get; set; }
    }
}
