using ClinicSystem.Server.Data;
using ClinicSystem.Server.DTOs.AI;
using ClinicSystem.Server.Models;
using ClinicSystem.Server.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Text;

namespace ClinicSystem.Server.Controllers
{
    [ApiController]
    [Route("api/visits/{visitId}/ai-suggestions")]
    [Authorize]
    public class AISuggestionsController : ControllerBase
    {
        private readonly AppDbContext _db;
        private readonly IOllamaService _ollama;

        public AISuggestionsController(AppDbContext db, IOllamaService ollama)
        {
            _db = db;
            _ollama = ollama;
        }

        [HttpGet]
        public async Task<IActionResult> GetSuggestions(Guid visitId)
        {
            var suggestions = await _db.AISuggestions
                .Include(a => a.RequestedByDoctor)
                .Include(a => a.EditedByUser)
                .Where(a => a.VisitId == visitId)
                .OrderByDescending(a => a.GeneratedAt)
                .Select(a => new AISuggestionDto
                {
                    SuggestionId = a.SuggestionId,
                    VisitId = a.VisitId,
                    Response = a.Response,
                    Model = a.Model,
                    RequestedByDoctorName = a.RequestedByDoctor != null ? a.RequestedByDoctor.FullName : null,
                    GeneratedAt = a.GeneratedAt,
                    IsManuallyEdited = a.EditedAt != null,
                    EditedByUserName = a.EditedByUser != null ? a.EditedByUser.FullName : null,
                    EditedAt = a.EditedAt
                }).ToListAsync();

            return Ok(suggestions);
        }

        [HttpPost]
        [Authorize(Roles = "Admin,Doctor")]
        public async Task<IActionResult> GenerateSuggestion(Guid visitId, [FromBody] GenerateAISuggestionRequest request)
        {
            var visit = await _db.Visits
                .Include(v => v.Patient)
                .Include(v => v.Vitals)
                .Include(v => v.PatientHistory)
                .Include(v => v.LabRequests).ThenInclude(lr => lr.Result)
                .Include(v => v.Prescriptions).ThenInclude(p => p.Items)
                .FirstOrDefaultAsync(v => v.VisitId == visitId);

            if (visit == null) return NotFound();

            var doctorId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                           ?? User.FindFirst(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Sub)?.Value;

            var prompt = BuildClinicalPrompt(visit, request.AdditionalContext);

            var response = await _ollama.GenerateAsync(prompt);

            var suggestion = new AISuggestion
            {
                VisitId = visitId,
                PatientId = visit.PatientId,
                PromptContext = prompt,
                Response = response,
                RequestedByDoctorId = doctorId
            };

            _db.AISuggestions.Add(suggestion);
            await _db.SaveChangesAsync();

            return Ok(new AISuggestionDto
            {
                SuggestionId = suggestion.SuggestionId,
                VisitId = visitId,
                Response = response,
                Model = suggestion.Model,
                GeneratedAt = suggestion.GeneratedAt,
                IsManuallyEdited = false,
                EditedByUserName = null,
                EditedAt = null
            });
        }

        [HttpPut("{suggestionId}")]
        [Authorize(Roles = "Admin,Doctor")]
        public async Task<IActionResult> UpdateSuggestion(Guid visitId, Guid suggestionId, [FromBody] UpdateAISuggestionRequest request)
        {
            var suggestion = await _db.AISuggestions
                .Include(a => a.RequestedByDoctor)
                .Include(a => a.EditedByUser)
                .FirstOrDefaultAsync(a => a.SuggestionId == suggestionId && a.VisitId == visitId);

            if (suggestion == null) return NotFound();

            var trimmedResponse = request.Response?.Trim();
            if (string.IsNullOrWhiteSpace(trimmedResponse))
            {
                return BadRequest(new { message = "Suggestion response cannot be empty." });
            }

            var editorId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                           ?? User.FindFirst(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Sub)?.Value;

            suggestion.Response = trimmedResponse;
            suggestion.EditedByUserId = editorId;
            suggestion.EditedAt = DateTime.UtcNow;

            await _db.SaveChangesAsync();

            var editedByName = suggestion.EditedByUser?.FullName;
            if (string.IsNullOrEmpty(editedByName) && !string.IsNullOrEmpty(editorId))
            {
                editedByName = await _db.Users
                    .Where(u => u.Id == editorId)
                    .Select(u => u.FullName)
                    .FirstOrDefaultAsync();
            }

            return Ok(new AISuggestionDto
            {
                SuggestionId = suggestion.SuggestionId,
                VisitId = suggestion.VisitId,
                Response = suggestion.Response,
                Model = suggestion.Model,
                RequestedByDoctorName = suggestion.RequestedByDoctor?.FullName,
                GeneratedAt = suggestion.GeneratedAt,
                IsManuallyEdited = suggestion.EditedAt != null,
                EditedByUserName = editedByName,
                EditedAt = suggestion.EditedAt
            });
        }

        private static string BuildClinicalPrompt(Visit visit, string? additionalContext)
        {
            var sb = new StringBuilder();
            sb.AppendLine("You are a clinical decision support assistant. Based on the following patient information, provide structured clinical suggestions. These are SUGGESTIONS ONLY — final clinical decisions rest with the attending physician.");
            sb.AppendLine();
            sb.AppendLine("=== PATIENT INFORMATION ===");

            if (visit.Patient != null)
            {
                sb.AppendLine($"Name: {visit.Patient.FirstName} {visit.Patient.LastName}");
                sb.AppendLine($"Age: {(int)((DateTime.UtcNow - visit.Patient.DateOfBirth).TotalDays / 365.25)} years");
                sb.AppendLine($"Gender: {visit.Patient.Gender}");
            }

            sb.AppendLine($"Chief Complaint: {visit.ChiefComplaint ?? "Not specified"}");
            sb.AppendLine();

            if (visit.Vitals != null)
            {
                sb.AppendLine("=== VITALS ===");
                if (!string.IsNullOrEmpty(visit.Vitals.BloodPressure)) sb.AppendLine($"Blood Pressure: {visit.Vitals.BloodPressure} mmHg");
                if (visit.Vitals.HeartRate.HasValue) sb.AppendLine($"Heart Rate: {visit.Vitals.HeartRate} bpm");
                if (visit.Vitals.Temperature.HasValue) sb.AppendLine($"Temperature: {visit.Vitals.Temperature}°C");
                if (visit.Vitals.Weight.HasValue) sb.AppendLine($"Weight: {visit.Vitals.Weight} kg");
                if (visit.Vitals.Height.HasValue) sb.AppendLine($"Height: {visit.Vitals.Height} cm");
                if (visit.Vitals.Bmi.HasValue) sb.AppendLine($"BMI: {visit.Vitals.Bmi}");
                if (visit.Vitals.OxygenSaturation.HasValue) sb.AppendLine($"Oxygen Saturation: {visit.Vitals.OxygenSaturation}%");
                if (visit.Vitals.RespiratoryRate.HasValue) sb.AppendLine($"Respiratory Rate: {visit.Vitals.RespiratoryRate} breaths/min");
                if (!string.IsNullOrEmpty(visit.Vitals.Notes)) sb.AppendLine($"Vitals Notes: {visit.Vitals.Notes}");
                sb.AppendLine();
            }

            if (visit.PatientHistory != null)
            {
                sb.AppendLine("=== CLINICAL HISTORY ===");
                if (!string.IsNullOrEmpty(visit.PatientHistory.PresentIllness)) sb.AppendLine($"History of Present Illness: {visit.PatientHistory.PresentIllness}");
                if (!string.IsNullOrEmpty(visit.PatientHistory.PastMedicalHistory)) sb.AppendLine($"Past Medical History: {visit.PatientHistory.PastMedicalHistory}");
                if (!string.IsNullOrEmpty(visit.PatientHistory.FamilyHistory)) sb.AppendLine($"Family History: {visit.PatientHistory.FamilyHistory}");
                if (!string.IsNullOrEmpty(visit.PatientHistory.Allergies)) sb.AppendLine($"Allergies: {visit.PatientHistory.Allergies}");
                if (!string.IsNullOrEmpty(visit.PatientHistory.CurrentMedications)) sb.AppendLine($"Current Medications: {visit.PatientHistory.CurrentMedications}");
                if (!string.IsNullOrEmpty(visit.PatientHistory.PhysicalExamination)) sb.AppendLine($"Physical Examination: {visit.PatientHistory.PhysicalExamination}");
                if (!string.IsNullOrEmpty(visit.PatientHistory.Assessment)) sb.AppendLine($"Doctor's Assessment: {visit.PatientHistory.Assessment}");
                sb.AppendLine();
            }

            if (visit.LabRequests?.Any(r => r.Result != null) == true)
            {
                sb.AppendLine("=== LABORATORY RESULTS ===");
                foreach (var lab in visit.LabRequests.Where(r => r.Result != null))
                {
                    sb.AppendLine($"Test: {lab.TestName} ({lab.TestType})");
                    if (!string.IsNullOrEmpty(lab.Result!.Findings)) sb.AppendLine($"  Findings: {lab.Result.Findings}");
                    if (!string.IsNullOrEmpty(lab.Result.Notes)) sb.AppendLine($"  Notes: {lab.Result.Notes}");
                }
                sb.AppendLine();
            }

            if (!string.IsNullOrEmpty(additionalContext))
            {
                sb.AppendLine("=== ADDITIONAL CONTEXT FROM DOCTOR ===");
                sb.AppendLine(additionalContext);
                sb.AppendLine();
            }

            sb.AppendLine("=== REQUESTED OUTPUT ===");
            sb.AppendLine("Please provide:");
            sb.AppendLine("1. DIFFERENTIAL DIAGNOSES (list 3-5 with brief reasoning)");
            sb.AppendLine("2. CLINICAL MANAGEMENT PLAN (immediate steps, monitoring)");
            sb.AppendLine("3. SUGGESTED PRESCRIPTION (drug name, dosage, frequency, duration — if applicable)");
            sb.AppendLine("4. RECOMMENDED ADDITIONAL WORKUP (labs, imaging — if needed)");
            sb.AppendLine("5. PATIENT EDUCATION POINTS (brief key points to tell the patient)");
            sb.AppendLine();
            sb.AppendLine("Format each section clearly with headers. Keep suggestions evidence-based and concise.");

            return sb.ToString();
        }
        [HttpDelete("{suggestionId}")]
        [Authorize(Roles = "Admin,Doctor")]
        public async Task<IActionResult> DeleteSuggestion(Guid visitId, Guid suggestionId)
        {
            var suggestion = await _db.AISuggestions.FirstOrDefaultAsync(a => a.SuggestionId == suggestionId && a.VisitId == visitId);
            if (suggestion == null) return NotFound();
            _db.AISuggestions.Remove(suggestion);
            await _db.SaveChangesAsync();
            return NoContent();
        }
    }
}
