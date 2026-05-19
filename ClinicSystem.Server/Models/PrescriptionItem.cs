using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ClinicSystem.Server.Models
{
    public class PrescriptionItem
    {
        public Guid ItemId { get; set; } = Guid.NewGuid();

        public Guid PrescriptionId { get; set; }

        [ForeignKey(nameof(PrescriptionId))]
        public Prescription? Prescription { get; set; }

        [Required, MaxLength(200)]
        public string Medication { get; set; } = string.Empty;

        [MaxLength(100)]
        public string? Dosage { get; set; }

        [MaxLength(100)]
        public string? Frequency { get; set; }

        [MaxLength(100)]
        public string? Duration { get; set; }

        [MaxLength(500)]
        public string? Instructions { get; set; }
    }
}
