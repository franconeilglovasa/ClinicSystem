using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ClinicSystem.Server.Models
{
    public class LabResult
    {
        public Guid ResultId { get; set; } = Guid.NewGuid();

        public Guid RequestId { get; set; }

        [ForeignKey(nameof(RequestId))]
        public LabRequest? Request { get; set; }

        public string? LabTechId { get; set; }

        [ForeignKey(nameof(LabTechId))]
        public ApplicationUser? LabTech { get; set; }

        public string? Findings { get; set; }

        [MaxLength(2000)]
        public string? Notes { get; set; }

        public DateTime ResultDate { get; set; } = DateTime.UtcNow;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public ICollection<LabResultAttachment> Attachments { get; set; } = new List<LabResultAttachment>();
    }
}
