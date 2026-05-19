using ClinicSystem.Server.Data;
using ClinicSystem.Server.DTOs.Patients;
using ClinicSystem.Server.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ClinicSystem.Server.Controllers
{
    [ApiController]
    [Route("api/patients")]
    [Authorize]
    public class PatientsController : ControllerBase
    {
        private readonly AppDbContext _db;

        public PatientsController(AppDbContext db)
        {
            _db = db;
        }

        [HttpGet]
        public async Task<IActionResult> GetPatients(
            [FromQuery] string? search,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20)
        {
            var query = _db.Patients.AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                var s = search.Trim().ToLower();
                query = query.Where(p =>
                    p.FirstName.ToLower().Contains(s) ||
                    p.LastName.ToLower().Contains(s) ||
                    (p.ContactNumber != null && p.ContactNumber.Contains(s)));
            }

            var total = await query.CountAsync();
            var items = await query
                .OrderBy(p => p.LastName).ThenBy(p => p.FirstName)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(p => new PatientDto
                {
                    PatientId = p.PatientId,
                    FirstName = p.FirstName,
                    LastName = p.LastName,
                    FullName = p.FirstName + " " + p.LastName,
                    DateOfBirth = p.DateOfBirth,
                    Age = (int)((DateTime.UtcNow - p.DateOfBirth).TotalDays / 365.25),
                    Gender = p.Gender.ToString(),
                    ContactNumber = p.ContactNumber,
                    Address = p.Address,
                    Email = p.Email,
                    CreatedAt = p.CreatedAt
                }).ToListAsync();

            return Ok(new PagedResult<PatientDto>
            {
                Items = items,
                TotalCount = total,
                Page = page,
                PageSize = pageSize
            });
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetPatient(Guid id)
        {
            var p = await _db.Patients.FindAsync(id);
            if (p == null) return NotFound();

            return Ok(new PatientDto
            {
                PatientId = p.PatientId,
                FirstName = p.FirstName,
                LastName = p.LastName,
                FullName = p.FirstName + " " + p.LastName,
                DateOfBirth = p.DateOfBirth,
                Age = (int)((DateTime.UtcNow - p.DateOfBirth).TotalDays / 365.25),
                Gender = p.Gender.ToString(),
                ContactNumber = p.ContactNumber,
                Address = p.Address,
                Email = p.Email,
                CreatedAt = p.CreatedAt
            });
        }

        [HttpPost]
        [Authorize(Roles = "Admin,Nurse")]
        public async Task<IActionResult> CreatePatient([FromBody] CreatePatientRequest request)
        {
            if (!Enum.TryParse<Gender>(request.Gender, true, out var gender))
                return BadRequest(new { message = "Invalid gender value." });

            var patient = new Patient
            {
                FirstName = request.FirstName.Trim(),
                LastName = request.LastName.Trim(),
                DateOfBirth = request.DateOfBirth.ToUniversalTime(),
                Gender = gender,
                ContactNumber = request.ContactNumber?.Trim(),
                Address = request.Address?.Trim(),
                Email = request.Email?.Trim()
            };

            _db.Patients.Add(patient);
            await _db.SaveChangesAsync();

            return CreatedAtAction(nameof(GetPatient), new { id = patient.PatientId }, new PatientDto
            {
                PatientId = patient.PatientId,
                FirstName = patient.FirstName,
                LastName = patient.LastName,
                FullName = patient.FirstName + " " + patient.LastName,
                DateOfBirth = patient.DateOfBirth,
                Age = (int)((DateTime.UtcNow - patient.DateOfBirth).TotalDays / 365.25),
                Gender = patient.Gender.ToString(),
                ContactNumber = patient.ContactNumber,
                Address = patient.Address,
                Email = patient.Email,
                CreatedAt = patient.CreatedAt
            });
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin,Nurse")]
        public async Task<IActionResult> UpdatePatient(Guid id, [FromBody] UpdatePatientRequest request)
        {
            var patient = await _db.Patients.FindAsync(id);
            if (patient == null) return NotFound();

            if (!Enum.TryParse<Gender>(request.Gender, true, out var gender))
                return BadRequest(new { message = "Invalid gender value." });

            patient.FirstName = request.FirstName.Trim();
            patient.LastName = request.LastName.Trim();
            patient.DateOfBirth = request.DateOfBirth.ToUniversalTime();
            patient.Gender = gender;
            patient.ContactNumber = request.ContactNumber?.Trim();
            patient.Address = request.Address?.Trim();
            patient.Email = request.Email?.Trim();

            await _db.SaveChangesAsync();
            return NoContent();
        }

        [HttpGet("{id}/visits")]
        public async Task<IActionResult> GetPatientVisits(Guid id)
        {
            var visits = await _db.Visits
                .Where(v => v.PatientId == id)
                .OrderByDescending(v => v.VisitDate)
                .Select(v => new
                {
                    v.VisitId,
                    v.VisitDate,
                    Status = v.Status.ToString(),
                    v.ChiefComplaint,
                    DoctorName = v.Doctor != null ? v.Doctor.FullName : null
                }).ToListAsync();

            return Ok(visits);
        }
    }
}
