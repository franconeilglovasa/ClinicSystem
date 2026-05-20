using ClinicSystem.Server.Data;
using ClinicSystem.Server.DTOs.Lab;
using ClinicSystem.Server.Models;
using ClinicSystem.Server.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace ClinicSystem.Server.Controllers
{
    [ApiController]
    [Authorize]
    public class LabController : ControllerBase
    {
        private readonly AppDbContext _db;
        private readonly IFileStorageService _fileStorage;

        public LabController(AppDbContext db, IFileStorageService fileStorage)
        {
            _db = db;
            _fileStorage = fileStorage;
        }

        [HttpGet("api/visits/{visitId}/lab-requests")]
        public async Task<IActionResult> GetLabRequestsByVisit(Guid visitId)
        {
            var requests = await _db.LabRequests
                .Include(r => r.Patient)
                .Include(r => r.RequestedByDoctor)
                .Include(r => r.Result).ThenInclude(r => r != null ? r.Attachments : null!)
                .Include(r => r.Result).ThenInclude(r => r != null ? r.LabTech : null!)
                .Where(r => r.VisitId == visitId)
                .OrderByDescending(r => r.RequestedAt)
                .ToListAsync();

            return Ok(requests.Select(MapToDto));
        }

        [HttpGet("api/lab-requests")]
        [Authorize(Roles = "Admin,Laboratory")]
        public async Task<IActionResult> GetPendingLabRequests([FromQuery] string? status)
        {
            var query = _db.LabRequests
                .Include(r => r.Patient)
                .Include(r => r.RequestedByDoctor)
                .Include(r => r.Result)
                .AsQueryable();

            if (!string.IsNullOrEmpty(status) && Enum.TryParse<LabRequestStatus>(status, true, out var s))
                query = query.Where(r => r.Status == s);
            else
                query = query.Where(r => r.Status == LabRequestStatus.Pending || r.Status == LabRequestStatus.InProgress);

            var requests = await query.OrderBy(r => r.RequestedAt).ToListAsync();
            return Ok(requests.Select(MapToDto));
        }

        [HttpPost("api/visits/{visitId}/lab-requests")]
        [Authorize(Roles = "Admin,Doctor")]
        public async Task<IActionResult> CreateLabRequest(Guid visitId, [FromBody] CreateLabRequestRequest request)
        {
            var visit = await _db.Visits.FindAsync(visitId);
            if (visit == null) return NotFound(new { message = "Visit not found." });

            var doctorId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                           ?? User.FindFirst(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Sub)?.Value;

            var labRequest = new LabRequest
            {
                VisitId = visitId,
                PatientId = visit.PatientId,
                RequestedByDoctorId = doctorId,
                TestType = request.TestType,
                TestName = request.TestName,
                Notes = request.Notes
            };

            _db.LabRequests.Add(labRequest);

            if (visit.Status == VisitStatus.WithDoctor)
                visit.Status = VisitStatus.ForLaboratory;

            await _db.SaveChangesAsync();
            return Created(string.Empty, new { labRequest.RequestId });
        }

        [HttpPost("api/lab-requests/{requestId}/results")]
        [Authorize(Roles = "Admin,Laboratory")]
        public async Task<IActionResult> SaveLabResult(Guid requestId, [FromBody] SaveLabResultRequest request)
        {
            var labRequest = await _db.LabRequests
                .Include(r => r.Result)
                .FirstOrDefaultAsync(r => r.RequestId == requestId);

            if (labRequest == null) return NotFound();

            var techId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                         ?? User.FindFirst(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Sub)?.Value;

            if (!string.IsNullOrWhiteSpace(techId))
            {
                var userExists = await _db.Users.AnyAsync(u => u.Id == techId);
                if (!userExists)
                {
                    return Unauthorized(new { message = "Session is out of date. Please sign in again." });
                }
            }

            Guid resultId;

            if (labRequest.Result != null)
            {
                labRequest.Result.Findings = request.Findings;
                labRequest.Result.Notes = request.Notes;
                labRequest.Result.ResultDate = request.ResultDate.ToUniversalTime();
                labRequest.Result.LabTechId = techId;
                resultId = labRequest.Result.ResultId;
            }
            else
            {
                var result = new LabResult
                {
                    RequestId = requestId,
                    LabTechId = techId,
                    Findings = request.Findings,
                    Notes = request.Notes,
                    ResultDate = request.ResultDate.ToUniversalTime()
                };
                _db.LabResults.Add(result);
                resultId = result.ResultId;
            }

            labRequest.Status = LabRequestStatus.Completed;

            await _db.SaveChangesAsync();
            return Ok(new { resultId });
        }

        [HttpPost("api/lab-results/{resultId}/attachments")]
        [Authorize(Roles = "Admin,Laboratory")]
        [RequestSizeLimit(20 * 1024 * 1024)]
        public async Task<IActionResult> UploadAttachment(Guid resultId, IFormFile file)
        {
            var result = await _db.LabResults.FindAsync(resultId);
            if (result == null) return NotFound();

            var (success, path, error) = await _fileStorage.SaveFileAsync(file, "lab");
            if (!success) return BadRequest(new { message = error });

            var attachment = new LabResultAttachment
            {
                ResultId = resultId,
                FileName = file.FileName,
                FilePath = path,
                FileType = Path.GetExtension(file.FileName).ToLowerInvariant(),
                FileSize = file.Length
            };

            _db.LabResultAttachments.Add(attachment);
            await _db.SaveChangesAsync();

            return Ok(new LabAttachmentDto
            {
                AttachmentId = attachment.AttachmentId,
                FileName = attachment.FileName,
                FileType = attachment.FileType ?? string.Empty,
                FileSize = attachment.FileSize,
                UploadedAt = attachment.UploadedAt
            });
        }

        [HttpGet("api/lab-results/{resultId}/attachments/{attachmentId}")]
        public async Task<IActionResult> DownloadAttachment(Guid resultId, Guid attachmentId)
        {
            var attachment = await _db.LabResultAttachments
                .FirstOrDefaultAsync(a => a.AttachmentId == attachmentId && a.ResultId == resultId);

            if (attachment == null) return NotFound();

            var absolutePath = _fileStorage.GetAbsolutePath(attachment.FilePath);
            if (!System.IO.File.Exists(absolutePath))
                return NotFound(new { message = "File not found on disk." });

            var contentType = attachment.FileType?.ToLower() switch
            {
                ".jpg" or ".jpeg" => "image/jpeg",
                ".png" => "image/png",
                ".pdf" => "application/pdf",
                ".dcm" => "application/dicom",
                _ => "application/octet-stream"
            };

            var bytes = await System.IO.File.ReadAllBytesAsync(absolutePath);
            return File(bytes, contentType, attachment.FileName);
        }

        [HttpDelete("api/lab-results/{resultId}/attachments/{attachmentId}")]
        [Authorize(Roles = "Admin,Laboratory")]
        public async Task<IActionResult> DeleteAttachment(Guid resultId, Guid attachmentId)
        {
            var attachment = await _db.LabResultAttachments
                .FirstOrDefaultAsync(a => a.AttachmentId == attachmentId && a.ResultId == resultId);

            if (attachment == null) return NotFound();

            _fileStorage.DeleteFile(attachment.FilePath);
            _db.LabResultAttachments.Remove(attachment);
            await _db.SaveChangesAsync();
            return NoContent();
        }

        private static LabRequestDto MapToDto(LabRequest r) => new()
        {
            RequestId = r.RequestId,
            VisitId = r.VisitId,
            PatientId = r.PatientId,
            PatientName = r.Patient != null ? r.Patient.FirstName + " " + r.Patient.LastName : string.Empty,
            RequestedByDoctorId = r.RequestedByDoctorId,
            RequestedByDoctorName = r.RequestedByDoctor?.FullName,
            TestType = r.TestType,
            TestName = r.TestName,
            Notes = r.Notes,
            Status = r.Status.ToString(),
            RequestedAt = r.RequestedAt,
            Result = r.Result == null ? null : new LabResultDto
            {
                ResultId = r.Result.ResultId,
                RequestId = r.Result.RequestId,
                LabTechId = r.Result.LabTechId,
                LabTechName = r.Result.LabTech?.FullName,
                Findings = r.Result.Findings,
                Notes = r.Result.Notes,
                ResultDate = r.Result.ResultDate,
                CreatedAt = r.Result.CreatedAt,
                Attachments = r.Result.Attachments?.Select(a => new LabAttachmentDto
                {
                    AttachmentId = a.AttachmentId,
                    FileName = a.FileName,
                    FileType = a.FileType ?? string.Empty,
                    FileSize = a.FileSize,
                    UploadedAt = a.UploadedAt
                }).ToList() ?? new()
            }
        };
    }
}
