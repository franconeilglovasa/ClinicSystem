using ClinicSystem.Server.Data;
using ClinicSystem.Server.DTOs.History;
using ClinicSystem.Server.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace ClinicSystem.Server.Controllers
{
    [ApiController]
    [Route("api/visits/{visitId}/history")]
    [Authorize]
    public class MedicalHistoryController : ControllerBase
    {
        private readonly AppDbContext _db;

        public MedicalHistoryController(AppDbContext db)
        {
            _db = db;
        }

        [HttpGet]
        public async Task<IActionResult> GetHistory(Guid visitId)
        {
            var h = await _db.PatientHistories
                .Include(h => h.Doctor)
                .FirstOrDefaultAsync(h => h.VisitId == visitId);

            if (h == null) return NotFound();
            return Ok(MapToDto(h));
        }

        [HttpPost]
        [Authorize(Roles = "Admin,Doctor")]
        public async Task<IActionResult> SaveHistory(Guid visitId, [FromBody] SavePatientHistoryRequest request)
        {
            var visit = await _db.Visits.FindAsync(visitId);
            if (visit == null) return NotFound(new { message = "Visit not found." });

            var doctorId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                           ?? User.FindFirst(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Sub)?.Value;

            var existing = await _db.PatientHistories.FirstOrDefaultAsync(h => h.VisitId == visitId);
            if (existing != null)
            {
                existing.ChiefComplaint = request.ChiefComplaint;
                existing.PresentIllness = request.PresentIllness;
                existing.PastMedicalHistory = request.PastMedicalHistory;
                existing.FamilyHistory = request.FamilyHistory;
                existing.SocialHistory = request.SocialHistory;
                existing.Allergies = request.Allergies;
                existing.CurrentMedications = request.CurrentMedications;
                existing.ReviewOfSystems = request.ReviewOfSystems;
                existing.PhysicalExamination = request.PhysicalExamination;
                existing.Assessment = request.Assessment;
                existing.Plan = request.Plan;
                existing.DoctorId = doctorId;
                existing.UpdatedAt = DateTime.UtcNow;
                await _db.SaveChangesAsync();

                var updated = await _db.PatientHistories.Include(h => h.Doctor).FirstAsync(h => h.HistoryId == existing.HistoryId);
                return Ok(MapToDto(updated));
            }

            var history = new PatientHistory
            {
                VisitId = visitId,
                PatientId = visit.PatientId,
                ChiefComplaint = request.ChiefComplaint,
                PresentIllness = request.PresentIllness,
                PastMedicalHistory = request.PastMedicalHistory,
                FamilyHistory = request.FamilyHistory,
                SocialHistory = request.SocialHistory,
                Allergies = request.Allergies,
                CurrentMedications = request.CurrentMedications,
                ReviewOfSystems = request.ReviewOfSystems,
                PhysicalExamination = request.PhysicalExamination,
                Assessment = request.Assessment,
                Plan = request.Plan,
                DoctorId = doctorId
            };

            _db.PatientHistories.Add(history);
            await _db.SaveChangesAsync();

            var created = await _db.PatientHistories.Include(h => h.Doctor).FirstAsync(h => h.HistoryId == history.HistoryId);
            return Ok(MapToDto(created));
        }

        private static PatientHistoryDto MapToDto(PatientHistory h) => new()
        {
            HistoryId = h.HistoryId,
            VisitId = h.VisitId,
            PatientId = h.PatientId,
            ChiefComplaint = h.ChiefComplaint,
            PresentIllness = h.PresentIllness,
            PastMedicalHistory = h.PastMedicalHistory,
            FamilyHistory = h.FamilyHistory,
            SocialHistory = h.SocialHistory,
            Allergies = h.Allergies,
            CurrentMedications = h.CurrentMedications,
            ReviewOfSystems = h.ReviewOfSystems,
            PhysicalExamination = h.PhysicalExamination,
            Assessment = h.Assessment,
            Plan = h.Plan,
            DoctorId = h.DoctorId,
            DoctorName = h.Doctor?.FullName,
            CreatedAt = h.CreatedAt,
            UpdatedAt = h.UpdatedAt
        };
    }
}
