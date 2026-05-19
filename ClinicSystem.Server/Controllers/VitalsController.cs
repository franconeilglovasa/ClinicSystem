using ClinicSystem.Server.Data;
using ClinicSystem.Server.DTOs.Vitals;
using ClinicSystem.Server.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace ClinicSystem.Server.Controllers
{
    [ApiController]
    [Route("api/visits/{visitId}/vitals")]
    [Authorize]
    public class VitalsController : ControllerBase
    {
        private readonly AppDbContext _db;

        public VitalsController(AppDbContext db)
        {
            _db = db;
        }

        [HttpGet]
        public async Task<IActionResult> GetVitals(Guid visitId)
        {
            var vitals = await _db.Vitals
                .Include(v => v.RecordedByNurse)
                .FirstOrDefaultAsync(v => v.VisitId == visitId);

            if (vitals == null) return NotFound();

            return Ok(MapToDto(vitals));
        }

        [HttpPost]
        [Authorize(Roles = "Admin,Nurse")]
        public async Task<IActionResult> RecordVitals(Guid visitId, [FromBody] CreateVitalsRequest request)
        {
            var visit = await _db.Visits.FindAsync(visitId);
            if (visit == null) return NotFound(new { message = "Visit not found." });

            var existing = await _db.Vitals.FirstOrDefaultAsync(v => v.VisitId == visitId);
            var nurseId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                          ?? User.FindFirst(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Sub)?.Value;

            if (existing != null)
            {
                // Update existing
                existing.BloodPressure = request.BloodPressure;
                existing.HeartRate = request.HeartRate;
                existing.Temperature = request.Temperature;
                existing.Weight = request.Weight;
                existing.Height = request.Height;
                existing.OxygenSaturation = request.OxygenSaturation;
                existing.RespiratoryRate = request.RespiratoryRate;
                existing.Notes = request.Notes;
                existing.RecordedByNurseId = nurseId;
                existing.RecordedAt = DateTime.UtcNow;
                await _db.SaveChangesAsync();
                return Ok(MapToDto(existing));
            }

            var vitals = new Vitals
            {
                VisitId = visitId,
                BloodPressure = request.BloodPressure,
                HeartRate = request.HeartRate,
                Temperature = request.Temperature,
                Weight = request.Weight,
                Height = request.Height,
                OxygenSaturation = request.OxygenSaturation,
                RespiratoryRate = request.RespiratoryRate,
                Notes = request.Notes,
                RecordedByNurseId = nurseId
            };

            _db.Vitals.Add(vitals);

            // Move visit to WithDoctor after vitals recorded
            if (visit.Status == VisitStatus.Waiting || visit.Status == VisitStatus.WithNurse)
                visit.Status = VisitStatus.WithDoctor;

            await _db.SaveChangesAsync();
            return Ok(MapToDto(vitals));
        }

        private static VitalsDto MapToDto(Vitals v) => new()
        {
            VitalsId = v.VitalsId,
            VisitId = v.VisitId,
            BloodPressure = v.BloodPressure,
            HeartRate = v.HeartRate,
            Temperature = v.Temperature,
            Weight = v.Weight,
            Height = v.Height,
            Bmi = v.Bmi,
            OxygenSaturation = v.OxygenSaturation,
            RespiratoryRate = v.RespiratoryRate,
            Notes = v.Notes,
            RecordedByNurseId = v.RecordedByNurseId,
            RecordedByNurseName = v.RecordedByNurse?.FullName,
            RecordedAt = v.RecordedAt
        };
    }
}
