namespace ClinicSystem.Server.DTOs.Vitals
{
    public class VitalsDto
    {
        public Guid VitalsId { get; set; }
        public Guid VisitId { get; set; }
        public string? BloodPressure { get; set; }
        public int? HeartRate { get; set; }
        public decimal? Temperature { get; set; }
        public decimal? Weight { get; set; }
        public decimal? Height { get; set; }
        public decimal? Bmi { get; set; }
        public int? OxygenSaturation { get; set; }
        public int? RespiratoryRate { get; set; }
        public string? Notes { get; set; }
        public string? RecordedByNurseId { get; set; }
        public string? RecordedByNurseName { get; set; }
        public DateTime RecordedAt { get; set; }
    }

    public class CreateVitalsRequest
    {
        public string? BloodPressure { get; set; }
        public int? HeartRate { get; set; }
        public decimal? Temperature { get; set; }
        public decimal? Weight { get; set; }
        public decimal? Height { get; set; }
        public int? OxygenSaturation { get; set; }
        public int? RespiratoryRate { get; set; }
        public string? Notes { get; set; }
    }
}
