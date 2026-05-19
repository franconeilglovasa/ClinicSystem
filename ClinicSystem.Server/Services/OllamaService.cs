using System.Text;
using System.Text.Json;

namespace ClinicSystem.Server.Services
{
    public interface IOllamaService
    {
        Task<string> GenerateAsync(string prompt);
    }

    public class OllamaService : IOllamaService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly ILogger<OllamaService> _logger;

        public OllamaService(HttpClient httpClient, IConfiguration configuration, ILogger<OllamaService> logger)
        {
            _httpClient = httpClient;
            _configuration = configuration;
            _logger = logger;
        }

        public async Task<string> GenerateAsync(string prompt)
        {
            var ollamaSettings = _configuration.GetSection("Ollama");
            var baseUrl = ollamaSettings["BaseUrl"] ?? "http://localhost:11434";
            var model = ollamaSettings["Model"] ?? "llama3";

            var requestBody = new
            {
                model,
                prompt,
                stream = false
            };

            var json = JsonSerializer.Serialize(requestBody);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            try
            {
                var response = await _httpClient.PostAsync($"{baseUrl}/api/generate", content);
                response.EnsureSuccessStatusCode();

                var responseJson = await response.Content.ReadAsStringAsync();
                using var doc = JsonDocument.Parse(responseJson);

                return doc.RootElement.GetProperty("response").GetString() ?? string.Empty;
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Failed to connect to Ollama at {BaseUrl}", baseUrl);
                return "AI service is currently unavailable. Please ensure Ollama is running at " + baseUrl;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error calling Ollama");
                return "An error occurred while generating AI suggestions.";
            }
        }
    }
}
