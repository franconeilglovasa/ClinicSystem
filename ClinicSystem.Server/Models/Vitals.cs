using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ClinicSystem.Server.Models
{
    public class Vitals
    {
        public Guid VitalsId { get; set; } = Guid.NewGuid();

        public Guid VisitId { get; set; }

        [ForeignKey(nameof(VisitId))]
        public Visit? Visit { get; set; }

        [MaxLength(20)]
        public string? BloodPressure { get; set; }

        public int? HeartRate { get; set; }

        public decimal? Temperature { get; set; }

        public decimal? Weight { get; set; }

        public decimal? Height { get; set; }

        public decimal? Bmi
        {
            get
            {
                if (Weight.HasValue && Height.HasValue && Height.Value > 0)
                {
                    var heightM = Height.Value / 100m;
                    return Math.Round(Weight.Value / (heightM * heightM), 1);
                }
                return null;
            }
        }

        public int? OxygenSaturation { get; set; }

        public int? RespiratoryRate { get; set; }

        [MaxLength(1000)]
        public string? Notes { get; set; }

        public string? RecordedByNurseId { get; set; }

        [ForeignKey(nameof(RecordedByNurseId))]
        public ApplicationUser? RecordedByNurse { get; set; }

        public DateTime RecordedAt { get; set; } = DateTime.UtcNow;
    }
}
