namespace ClinicSystem.Server.DTOs.AI
{
    public class AISuggestionDto
    {
        public Guid SuggestionId { get; set; }
        public Guid VisitId { get; set; }
        public string? Response { get; set; }
        public string Model { get; set; } = string.Empty;
        public string? RequestedByDoctorName { get; set; }
        public DateTime GeneratedAt { get; set; }
    }

    public class GenerateAISuggestionRequest
    {
        public string? AdditionalContext { get; set; }
    }
}
