using ClinicSystem.Server.Data;
using ClinicSystem.Server.DTOs.Visits;
using ClinicSystem.Server.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace ClinicSystem.Server.Controllers
{
    [ApiController]
    [Route("api/visits")]
    [Authorize]
    public class VisitsController : ControllerBase
    {
        private readonly AppDbContext _db;

        public VisitsController(AppDbContext db)
        {
            _db = db;
        }

        [HttpGet]
        public async Task<IActionResult> GetVisits(
            [FromQuery] string? status,
            [FromQuery] DateTime? date,
            [FromQuery] Guid? patientId)
        {
            var query = _db.Visits
                .Include(v => v.Patient)
                .Include(v => v.Nurse)
                .Include(v => v.Doctor)
                .AsQueryable();

            var targetDate = date?.ToUniversalTime().Date ?? DateTime.UtcNow.Date;
            query = query.Where(v => v.VisitDate.Date == targetDate);

            if (!string.IsNullOrWhiteSpace(status) && Enum.TryParse<VisitStatus>(status, true, out var visitStatus))
                query = query.Where(v => v.Status == visitStatus);

            if (patientId.HasValue)
                query = query.Where(v => v.PatientId == patientId.Value);

            var visits = await query
                .OrderBy(v => v.CreatedAt)
                .Select(v => new VisitDto
                {
                    VisitId = v.VisitId,
                    PatientId = v.PatientId,
                    PatientName = v.Patient != null ? v.Patient.FirstName + " " + v.Patient.LastName : string.Empty,
                    VisitDate = v.VisitDate,
                    Status = v.Status.ToString(),
                    ChiefComplaint = v.ChiefComplaint,
                    NurseId = v.NurseId,
                    NurseName = v.Nurse != null ? v.Nurse.FullName : null,
                    DoctorId = v.DoctorId,
                    DoctorName = v.Doctor != null ? v.Doctor.FullName : null,
                    CreatedAt = v.CreatedAt,
                    HasVitals = v.Vitals != null,
                    HasHistory = v.PatientHistory != null,
                    HasLabRequests = v.LabRequests.Any(),
                    HasBill = v.Bill != null
                }).ToListAsync();

            return Ok(visits);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetVisit(Guid id)
        {
            var v = await _db.Visits
                .Include(v => v.Patient)
                .Include(v => v.Nurse)
                .Include(v => v.Doctor)
                .Include(v => v.Vitals)
                .Include(v => v.PatientHistory)
                .Include(v => v.LabRequests)
                .Include(v => v.Bill)
                .FirstOrDefaultAsync(v => v.VisitId == id);

            if (v == null) return NotFound();

            return Ok(new VisitDto
            {
                VisitId = v.VisitId,
                PatientId = v.PatientId,
                PatientName = v.Patient != null ? v.Patient.FirstName + " " + v.Patient.LastName : string.Empty,
                VisitDate = v.VisitDate,
                Status = v.Status.ToString(),
                ChiefComplaint = v.ChiefComplaint,
                NurseId = v.NurseId,
                NurseName = v.Nurse?.FullName,
                DoctorId = v.DoctorId,
                DoctorName = v.Doctor?.FullName,
                CreatedAt = v.CreatedAt,
                HasVitals = v.Vitals != null,
                HasHistory = v.PatientHistory != null,
                HasLabRequests = v.LabRequests.Any(),
                HasBill = v.Bill != null
            });
        }

        [HttpPost]
        [Authorize(Roles = "Admin,Nurse")]
        public async Task<IActionResult> CreateVisit([FromBody] CreateVisitRequest request)
        {
            var patient = await _db.Patients.FindAsync(request.PatientId);
            if (patient == null) return BadRequest(new { message = "Patient not found." });

            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                         ?? User.FindFirst(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Sub)?.Value;

            var visit = new Visit
            {
                PatientId = request.PatientId,
                ChiefComplaint = request.ChiefComplaint?.Trim(),
                NurseId = userId,
                DoctorId = request.DoctorId,
                Status = VisitStatus.Waiting
            };

            _db.Visits.Add(visit);
            await _db.SaveChangesAsync();

            return CreatedAtAction(nameof(GetVisit), new { id = visit.VisitId }, new { visit.VisitId });
        }

        [HttpPatch("{id}/status")]
        [Authorize(Roles = "Admin,Nurse,Doctor")]
        public async Task<IActionResult> UpdateStatus(Guid id, [FromBody] UpdateVisitStatusRequest request)
        {
            var visit = await _db.Visits.FindAsync(id);
            if (visit == null) return NotFound();

            if (!Enum.TryParse<VisitStatus>(request.Status, true, out var newStatus))
                return BadRequest(new { message = "Invalid status." });

            visit.Status = newStatus;
            if (!string.IsNullOrEmpty(request.DoctorId))
                visit.DoctorId = request.DoctorId;

            await _db.SaveChangesAsync();
            return Ok(new { status = visit.Status.ToString() });
        }
    }
}
