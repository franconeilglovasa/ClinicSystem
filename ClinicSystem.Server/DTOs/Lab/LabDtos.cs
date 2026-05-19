namespace ClinicSystem.Server.DTOs.Lab
{
    public class LabAttachmentDto
    {
        public Guid AttachmentId { get; set; }
        public string FileName { get; set; } = string.Empty;
        public string FileType { get; set; } = string.Empty;
        public long FileSize { get; set; }
        public DateTime UploadedAt { get; set; }
    }

    public class LabResultDto
    {
        public Guid ResultId { get; set; }
        public Guid RequestId { get; set; }
        public string? LabTechId { get; set; }
        public string? LabTechName { get; set; }
        public string? Findings { get; set; }
        public string? Notes { get; set; }
        public DateTime ResultDate { get; set; }
        public DateTime CreatedAt { get; set; }
        public List<LabAttachmentDto> Attachments { get; set; } = new();
    }

    public class LabRequestDto
    {
        public Guid RequestId { get; set; }
        public Guid VisitId { get; set; }
        public Guid PatientId { get; set; }
        public string PatientName { get; set; } = string.Empty;
        public string? RequestedByDoctorId { get; set; }
        public string? RequestedByDoctorName { get; set; }
        public string TestType { get; set; } = string.Empty;
        public string TestName { get; set; } = string.Empty;
        public string? Notes { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime RequestedAt { get; set; }
        public LabResultDto? Result { get; set; }
    }

    public class CreateLabRequestRequest
    {
        public string TestType { get; set; } = string.Empty;
        public string TestName { get; set; } = string.Empty;
        public string? Notes { get; set; }
    }

    public class SaveLabResultRequest
    {
        public string? Findings { get; set; }
        public string? Notes { get; set; }
        public DateTime ResultDate { get; set; } = DateTime.UtcNow;
    }
}
