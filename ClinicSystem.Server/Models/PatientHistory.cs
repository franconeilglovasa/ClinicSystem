using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ClinicSystem.Server.Models
{
    public class PatientHistory
    {
        public Guid HistoryId { get; set; } = Guid.NewGuid();

        public Guid VisitId { get; set; }

        [ForeignKey(nameof(VisitId))]
        public Visit? Visit { get; set; }

        public Guid PatientId { get; set; }

        [ForeignKey(nameof(PatientId))]
        public Patient? Patient { get; set; }

        [MaxLength(500)]
        public string? ChiefComplaint { get; set; }

        public string? PresentIllness { get; set; }

        public string? PastMedicalHistory { get; set; }

        public string? FamilyHistory { get; set; }

        public string? SocialHistory { get; set; }

        [MaxLength(500)]
        public string? Allergies { get; set; }

        public string? CurrentMedications { get; set; }

        public string? ReviewOfSystems { get; set; }

        public string? PhysicalExamination { get; set; }

        public string? Assessment { get; set; }

        public string? Plan { get; set; }

        public string? DoctorId { get; set; }

        [ForeignKey(nameof(DoctorId))]
        public ApplicationUser? Doctor { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }
    }
}
