using ClinicSystem.Server.Data;
using ClinicSystem.Server.DTOs.Prescriptions;
using ClinicSystem.Server.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace ClinicSystem.Server.Controllers
{
    [ApiController]
    [Route("api/visits/{visitId}/prescriptions")]
    [Authorize]
    public class PrescriptionsController : ControllerBase
    {
        private readonly AppDbContext _db;
        private readonly IConfiguration _configuration;

        public PrescriptionsController(AppDbContext db, IConfiguration configuration)
        {
            _db = db;
            _configuration = configuration;
        }

        [HttpGet]
        public async Task<IActionResult> GetPrescriptions(Guid visitId)
        {
            var prescriptions = await _db.Prescriptions
                .Include(p => p.Doctor)
                .Include(p => p.Patient)
                .Include(p => p.Items)
                .Where(p => p.VisitId == visitId)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();

            return Ok(prescriptions.Select(MapToDto));
        }

        [HttpGet("{prescriptionId}")]
        public async Task<IActionResult> GetPrescription(Guid visitId, Guid prescriptionId)
        {
            var prescription = await _db.Prescriptions
                .Include(p => p.Doctor)
                .Include(p => p.Patient)
                .Include(p => p.Items)
                .FirstOrDefaultAsync(p => p.PrescriptionId == prescriptionId && p.VisitId == visitId);

            if (prescription == null) return NotFound();
            return Ok(MapToDto(prescription));
        }

        [HttpPost]
        [Authorize(Roles = "Admin,Doctor")]
        public async Task<IActionResult> CreatePrescription(Guid visitId, [FromBody] CreatePrescriptionRequest request)
        {
            var visit = await _db.Visits.Include(v => v.Patient).FirstOrDefaultAsync(v => v.VisitId == visitId);
            if (visit == null) return NotFound(new { message = "Visit not found." });

            var doctorId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                           ?? User.FindFirst(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Sub)?.Value;

            var prescription = new Prescription
            {
                VisitId = visitId,
                PatientId = visit.PatientId,
                DoctorId = doctorId,
                Instructions = request.Instructions,
                Items = request.Items.Select(i => new PrescriptionItem
                {
                    Medication = i.Medication,
                    Dosage = i.Dosage,
                    Frequency = i.Frequency,
                    Duration = i.Duration,
                    Instructions = i.Instructions
                }).ToList()
            };

            _db.Prescriptions.Add(prescription);
            await _db.SaveChangesAsync();

            var created = await _db.Prescriptions
                .Include(p => p.Doctor)
                .Include(p => p.Patient)
                .Include(p => p.Items)
                .FirstAsync(p => p.PrescriptionId == prescription.PrescriptionId);

            return CreatedAtAction(nameof(GetPrescription),
                new { visitId, prescriptionId = prescription.PrescriptionId },
                MapToDto(created));
        }

        [HttpDelete("{prescriptionId}")]
        [Authorize(Roles = "Admin,Doctor")]
        public async Task<IActionResult> DeletePrescription(Guid visitId, Guid prescriptionId)
        {
            var prescription = await _db.Prescriptions
                .FirstOrDefaultAsync(p => p.PrescriptionId == prescriptionId && p.VisitId == visitId);
            if (prescription == null) return NotFound();

            _db.Prescriptions.Remove(prescription);
            await _db.SaveChangesAsync();
            return NoContent();
        }

        private PrescriptionDto MapToDto(Prescription p) => new()
        {
            PrescriptionId = p.PrescriptionId,
            VisitId = p.VisitId,
            PatientId = p.PatientId,
            PatientName = p.Patient != null ? p.Patient.FirstName + " " + p.Patient.LastName : string.Empty,
            PatientAge = p.Patient != null ? (int)((DateTime.UtcNow - p.Patient.DateOfBirth).TotalDays / 365.25) : 0,
            PatientGender = p.Patient?.Gender.ToString() ?? string.Empty,
            DoctorId = p.DoctorId,
            DoctorName = p.Doctor?.FullName,
            DoctorSpecialty = p.Doctor?.Specialty,
            DoctorLicense = p.Doctor?.LicenseNumber,
            Date = p.Date,
            Instructions = p.Instructions,
            CreatedAt = p.CreatedAt,
            Items = p.Items.Select(i => new PrescriptionItemDto
            {
                ItemId = i.ItemId,
                Medication = i.Medication,
                Dosage = i.Dosage,
                Frequency = i.Frequency,
                Duration = i.Duration,
                Instructions = i.Instructions
            }).ToList()
        };
    }
}
