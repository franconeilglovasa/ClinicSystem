using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ClinicSystem.Server.Models
{
    public class Prescription
    {
        public Guid PrescriptionId { get; set; } = Guid.NewGuid();

        public Guid VisitId { get; set; }

        [ForeignKey(nameof(VisitId))]
        public Visit? Visit { get; set; }

        public Guid PatientId { get; set; }

        [ForeignKey(nameof(PatientId))]
        public Patient? Patient { get; set; }

        public string? DoctorId { get; set; }

        [ForeignKey(nameof(DoctorId))]
        public ApplicationUser? Doctor { get; set; }

        public DateTime Date { get; set; } = DateTime.UtcNow;

        [MaxLength(1000)]
        public string? Instructions { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public ICollection<PrescriptionItem> Items { get; set; } = new List<PrescriptionItem>();
    }
}
