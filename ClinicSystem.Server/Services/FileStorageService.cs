namespace ClinicSystem.Server.Services
{
    public interface IFileStorageService
    {
        Task<(bool Success, string RelativePath, string Error)> SaveFileAsync(IFormFile file, string subfolder);
        string GetAbsolutePath(string relativePath);
        bool DeleteFile(string relativePath);
    }

    public class FileStorageService : IFileStorageService
    {
        private readonly IConfiguration _configuration;
        private readonly IWebHostEnvironment _environment;
        private readonly ILogger<FileStorageService> _logger;

        private static readonly HashSet<string> AllowedExtensions = new(StringComparer.OrdinalIgnoreCase)
        {
            ".jpg", ".jpeg", ".png", ".pdf", ".dcm"
        };

        public FileStorageService(IConfiguration configuration, IWebHostEnvironment environment, ILogger<FileStorageService> logger)
        {
            _configuration = configuration;
            _environment = environment;
            _logger = logger;
        }

        public async Task<(bool Success, string RelativePath, string Error)> SaveFileAsync(IFormFile file, string subfolder)
        {
            var maxSizeMB = _configuration.GetValue<int>("FileStorage:MaxFileSizeMB", 20);
            var maxBytes = maxSizeMB * 1024 * 1024;

            if (file.Length > maxBytes)
                return (false, string.Empty, $"File exceeds maximum allowed size of {maxSizeMB} MB.");

            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!AllowedExtensions.Contains(extension))
                return (false, string.Empty, $"File type '{extension}' is not allowed.");

            var uploadRoot = _configuration["FileStorage:UploadPath"] ?? "wwwroot/uploads";
            var yearMonth = DateTime.UtcNow.ToString("yyyy/MM");
            var directory = Path.Combine(_environment.ContentRootPath, uploadRoot, subfolder, yearMonth);

            Directory.CreateDirectory(directory);

            var safeFileName = $"{Guid.NewGuid()}{extension}";
            var absolutePath = Path.Combine(directory, safeFileName);
            var relativePath = Path.Combine(uploadRoot, subfolder, yearMonth, safeFileName).Replace('\\', '/');

            try
            {
                await using var stream = new FileStream(absolutePath, FileMode.Create);
                await file.CopyToAsync(stream);
                return (true, relativePath, string.Empty);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to save file {FileName}", file.FileName);
                return (false, string.Empty, "Failed to save file.");
            }
        }

        public string GetAbsolutePath(string relativePath)
        {
            // Support both legacy paths ("uploads/...") and current paths ("wwwroot/uploads/...").
            var normalized = relativePath.Replace('\\', '/').TrimStart('/');
            if (normalized.StartsWith("uploads/", StringComparison.OrdinalIgnoreCase))
            {
                normalized = $"wwwroot/{normalized}";
            }

            return Path.Combine(_environment.ContentRootPath, normalized);
        }

        public bool DeleteFile(string relativePath)
        {
            try
            {
                var absolutePath = GetAbsolutePath(relativePath);
                if (File.Exists(absolutePath))
                {
                    File.Delete(absolutePath);
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to delete file {RelativePath}", relativePath);
                return false;
            }
        }
    }
}
