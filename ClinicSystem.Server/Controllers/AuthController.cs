using ClinicSystem.Server.DTOs.Auth;
using ClinicSystem.Server.Models;
using ClinicSystem.Server.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ClinicSystem.Server.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IConfiguration _configuration;

        public AuthController(IAuthService authService, UserManager<ApplicationUser> userManager, IConfiguration configuration)
        {
            _authService = authService;
            _userManager = userManager;
            _configuration = configuration;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            var (success, token, error) = await _authService.LoginAsync(request.Email, request.Password);
            if (!success)
                return Unauthorized(new { message = error });

            var user = await _userManager.FindByEmailAsync(request.Email);
            var expiryMinutes = double.Parse(_configuration["JwtSettings:ExpiryMinutes"] ?? "480");

            return Ok(new LoginResponse
            {
                Token = token,
                UserId = user!.Id,
                FullName = user.FullName,
                Email = user.Email!,
                Role = user.Role.ToString(),
                ExpiresAt = DateTime.UtcNow.AddMinutes(expiryMinutes)
            });
        }

        [HttpPost("register")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        {
            if (!Enum.TryParse<UserRole>(request.Role, true, out var role))
                return BadRequest(new { message = "Invalid role." });

            var user = new ApplicationUser
            {
                UserName = request.Email,
                Email = request.Email,
                FullName = request.FullName,
                Role = role,
                Specialty = request.Specialty,
                LicenseNumber = request.LicenseNumber,
                IsActive = true,
                EmailConfirmed = true
            };

            var result = await _userManager.CreateAsync(user, request.Password);
            if (!result.Succeeded)
                return BadRequest(new { errors = result.Errors.Select(e => e.Description) });

            await _userManager.AddToRoleAsync(user, role.ToString());

            return CreatedAtAction(nameof(GetMe), new { }, new UserDto
            {
                Id = user.Id,
                FullName = user.FullName,
                Email = user.Email!,
                Role = user.Role.ToString(),
                Specialty = user.Specialty,
                LicenseNumber = user.LicenseNumber,
                IsActive = user.IsActive,
                CreatedAt = user.CreatedAt
            });
        }

        [HttpGet("me")]
        [Authorize]
        public async Task<IActionResult> GetMe()
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value
                         ?? User.FindFirst(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Sub)?.Value;
            if (userId == null) return Unauthorized();

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return NotFound();

            return Ok(new UserDto
            {
                Id = user.Id,
                FullName = user.FullName,
                Email = user.Email!,
                Role = user.Role.ToString(),
                Specialty = user.Specialty,
                LicenseNumber = user.LicenseNumber,
                IsActive = user.IsActive,
                CreatedAt = user.CreatedAt
            });
        }
    }
}
