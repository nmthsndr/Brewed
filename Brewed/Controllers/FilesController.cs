using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Brewed.Services;

namespace Brewed.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin")]
    public class FilesController : ControllerBase
    {
        private readonly IFileUploadService _fileUploadService;

        public FilesController(IFileUploadService fileUploadService)
        {
            _fileUploadService = fileUploadService;
        }

        [HttpPost("upload")]
        public async Task<IActionResult> UploadImage(IFormFile file, [FromQuery] string folder = "products")
        {
            try
            {
                var imageUrl = await _fileUploadService.UploadImageAsync(file, folder);
                return Ok(new { Url = imageUrl });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Error = ex.Message });
            }
        }

        [HttpPost("upload-multiple")]
        public async Task<IActionResult> UploadMultipleImages(List<IFormFile> files, [FromQuery] string folder = "products")
        {
            try
            {
                if (files == null || files.Count == 0)
                {
                    return BadRequest(new { Error = "No files provided" });
                }

                var imageUrls = await _fileUploadService.UploadMultipleImagesAsync(files, folder);
                return Ok(new { Urls = imageUrls });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Error = ex.Message });
            }
        }

        [HttpDelete]
        public async Task<IActionResult> DeleteImage([FromQuery] string imageUrl)
        {
            try
            {
                var result = await _fileUploadService.DeleteImageAsync(imageUrl);

                if (result)
                {
                    return Ok(new { Message = "Image deleted successfully" });
                }

                return NotFound(new { Error = "Image not found" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Error = ex.Message });
            }
        }
    }
}