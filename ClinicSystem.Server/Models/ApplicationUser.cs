using Microsoft.AspNetCore.Identity;

namespace ClinicSystem.Server.Models
{
    public enum UserRole
    {
        Admin,
        Doctor,
        Nurse,
        Laboratory,
        Cashier
    }

    public class ApplicationUser : IdentityUser
    {
        public string FullName { get; set; } = string.Empty;
        public UserRole Role { get; set; }
        public string? Specialty { get; set; }
        public string? LicenseNumber { get; set; }
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
