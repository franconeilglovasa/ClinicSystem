using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ClinicSystem.Server.Models
{
    public enum VisitStatus
    {
        Waiting,
        WithNurse,
        WithDoctor,
        ForLaboratory,
        ForBilling,
        Completed
    }

    public class Visit
    {
        public Guid VisitId { get; set; } = Guid.NewGuid();

        public Guid PatientId { get; set; }

        [ForeignKey(nameof(PatientId))]
        public Patient? Patient { get; set; }

        public DateTime VisitDate { get; set; } = DateTime.UtcNow;

        public VisitStatus Status { get; set; } = VisitStatus.Waiting;

        [MaxLength(500)]
        public string? ChiefComplaint { get; set; }

        public string? NurseId { get; set; }

        [ForeignKey(nameof(NurseId))]
        public ApplicationUser? Nurse { get; set; }

        public string? DoctorId { get; set; }

        [ForeignKey(nameof(DoctorId))]
        public ApplicationUser? Doctor { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public Vitals? Vitals { get; set; }
        public PatientHistory? PatientHistory { get; set; }
        public ICollection<Prescription> Prescriptions { get; set; } = new List<Prescription>();
        public ICollection<LabRequest> LabRequests { get; set; } = new List<LabRequest>();
        public Bill? Bill { get; set; }
        public ICollection<AISuggestion> AISuggestions { get; set; } = new List<AISuggestion>();
    }
}
