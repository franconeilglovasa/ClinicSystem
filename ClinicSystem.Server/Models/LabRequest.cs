using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ClinicSystem.Server.Models
{
    public enum LabRequestStatus
    {
        Pending,
        InProgress,
        Completed
    }

    public class LabRequest
    {
        public Guid RequestId { get; set; } = Guid.NewGuid();

        public Guid VisitId { get; set; }

        [ForeignKey(nameof(VisitId))]
        public Visit? Visit { get; set; }

        public Guid PatientId { get; set; }

        [ForeignKey(nameof(PatientId))]
        public Patient? Patient { get; set; }

        public string? RequestedByDoctorId { get; set; }

        [ForeignKey(nameof(RequestedByDoctorId))]
        public ApplicationUser? RequestedByDoctor { get; set; }

        [Required, MaxLength(100)]
        public string TestType { get; set; } = string.Empty;

        [Required, MaxLength(200)]
        public string TestName { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? Notes { get; set; }

        public LabRequestStatus Status { get; set; } = LabRequestStatus.Pending;

        public DateTime RequestedAt { get; set; } = DateTime.UtcNow;

        public LabResult? Result { get; set; }
    }
}
