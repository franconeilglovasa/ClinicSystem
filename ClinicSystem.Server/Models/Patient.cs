using System.ComponentModel.DataAnnotations;

namespace ClinicSystem.Server.Models
{
    public enum Gender
    {
        Male,
        Female,
        Other
    }

    public class Patient
    {
        public Guid PatientId { get; set; } = Guid.NewGuid();

        [Required, MaxLength(100)]
        public string FirstName { get; set; } = string.Empty;

        [Required, MaxLength(100)]
        public string LastName { get; set; } = string.Empty;

        public string FullName => $"{FirstName} {LastName}";

        public DateTime DateOfBirth { get; set; }

        public int Age => (int)((DateTime.UtcNow - DateOfBirth).TotalDays / 365.25);

        public Gender Gender { get; set; }

        [MaxLength(20)]
        public string? ContactNumber { get; set; }

        [MaxLength(255)]
        public string? Address { get; set; }

        [MaxLength(100), EmailAddress]
        public string? Email { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public ICollection<Visit> Visits { get; set; } = new List<Visit>();
    }
}
