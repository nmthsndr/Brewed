using Microsoft.AspNetCore.Http;

namespace Brewed.Services
{
    public interface IFileUploadService
    {
        Task<string> UploadImageAsync(IFormFile file, string folder = "products");
        Task<bool> DeleteImageAsync(string imageUrl);
        Task<List<string>> UploadMultipleImagesAsync(List<IFormFile> files, string folder = "products");
    }

    public class FileUploadService : IFileUploadService
    {
        private readonly string _uploadPath;
        private readonly long _maxFileSize = 5 * 1024 * 1024; // 5MB
        private readonly string[] _allowedExtensions = { ".jpg", ".jpeg", ".png", ".webp", ".gif" };

        public FileUploadService(string webRootPath)
        {
            _uploadPath = Path.Combine(webRootPath, "images");

            // Create upload directory if it doesn't exist
            if (!Directory.Exists(_uploadPath))
            {
                Directory.CreateDirectory(_uploadPath);
            }
        }

        public async Task<string> UploadImageAsync(IFormFile file, string folder = "products")
        {
            if (file == null || file.Length == 0)
            {
                throw new Exception("No file provided");
            }

            // Validate file size
            if (file.Length > _maxFileSize)
            {
                throw new Exception($"File size exceeds maximum allowed size of {_maxFileSize / (1024 * 1024)}MB");
            }

            // Validate file extension
            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!_allowedExtensions.Contains(extension))
            {
                throw new Exception($"File type not allowed. Allowed types: {string.Join(", ", _allowedExtensions)}");
            }

            // Create folder if it doesn't exist
            var folderPath = Path.Combine(_uploadPath, folder);
            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }

            // Generate unique filename
            var fileName = $"{Guid.NewGuid()}{extension}";
            var filePath = Path.Combine(folderPath, fileName);

            // Save file
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            // Return relative URL
            return $"/images/{folder}/{fileName}";
        }

        public async Task<List<string>> UploadMultipleImagesAsync(List<IFormFile> files, string folder = "products")
        {
            var uploadedUrls = new List<string>();

            foreach (var file in files)
            {
                try
                {
                    var url = await UploadImageAsync(file, folder);
                    uploadedUrls.Add(url);
                }
                catch
                {
                    // If any upload fails, delete previously uploaded files
                    foreach (var uploadedUrl in uploadedUrls)
                    {
                        await DeleteImageAsync(uploadedUrl);
                    }
                    throw;
                }
            }

            return uploadedUrls;
        }

        public Task<bool> DeleteImageAsync(string imageUrl)
        {
            try
            {
                if (string.IsNullOrEmpty(imageUrl))
                {
                    return Task.FromResult(false);
                }

                // Convert URL to physical path
                // imageUrl format: /images/products/filename.jpg
                var relativePath = imageUrl.TrimStart('/').Replace('/', Path.DirectorySeparatorChar);
                var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", relativePath);

                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                    return Task.FromResult(true);
                }

                return Task.FromResult(false);
            }
            catch
            {
                return Task.FromResult(false);
            }
        }
    }
}