using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ClinicSystem.Server.Models
{
    public class LabResultAttachment
    {
        public Guid AttachmentId { get; set; } = Guid.NewGuid();

        public Guid ResultId { get; set; }

        [ForeignKey(nameof(ResultId))]
        public LabResult? Result { get; set; }

        [Required, MaxLength(255)]
        public string FileName { get; set; } = string.Empty;

        [Required, MaxLength(500)]
        public string FilePath { get; set; } = string.Empty;

        [MaxLength(50)]
        public string? FileType { get; set; }

        public long FileSize { get; set; }

        public DateTime UploadedAt { get; set; } = DateTime.UtcNow;
    }
}
