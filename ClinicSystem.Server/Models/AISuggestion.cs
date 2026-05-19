using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ClinicSystem.Server.Models
{
    public class AISuggestion
    {
        [Key] 
        public Guid SuggestionId { get; set; } = Guid.NewGuid();

        public Guid VisitId { get; set; }

        [ForeignKey(nameof(VisitId))]
        public Visit? Visit { get; set; }

        public Guid PatientId { get; set; }

        [ForeignKey(nameof(PatientId))]
        public Patient? Patient { get; set; }

        public string? PromptContext { get; set; }

        public string? Response { get; set; }

        [MaxLength(50)]
        public string Model { get; set; } = "llama3";

        public string? RequestedByDoctorId { get; set; }

        [ForeignKey(nameof(RequestedByDoctorId))]
        public ApplicationUser? RequestedByDoctor { get; set; }

        public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;
    }
}
