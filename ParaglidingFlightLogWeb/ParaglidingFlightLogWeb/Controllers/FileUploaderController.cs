using Microsoft.AspNetCore.Mvc;

namespace ParaglidingFlightLogWeb.Controllers;
/// <summary>
/// Handle the file upload from RadzenUpload
/// </summary>
[Route("api/[controller]")]
[ApiController]
public class FileUploaderController : ControllerBase
{
    private readonly ILogger<FileUploaderController> _logger;
    private readonly IWebHostEnvironment _webHostEnvironment;

    /// <summary>
    /// ctor
    /// </summary>
    /// <param name="logger"></param>
    /// <param name="webHostEnvironment"></param>
    public FileUploaderController(ILogger<FileUploaderController> logger, IWebHostEnvironment webHostEnvironment)
    {
        _logger = logger;
        _webHostEnvironment = webHostEnvironment;
    }
    /// <summary>
    /// Upload an Igc file
    /// </summary>
    /// <param name="files"></param>
    /// <returns></returns>
    [HttpPost("upload-igc", Name = "PostUploadIgc")]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> PostUploadIgc(IFormFile[] files)
    {
        try
        {
            List<string> uploadedFiles = [];
            foreach (IFormFile f in files)
            {
                _logger.LogInformation("Uploading file {FileName}", f.FileName);
                if (files.Length == 0) { return BadRequest("No file uploaded."); }
                var trustedFileNameForFileStorage = Path.GetRandomFileName();
                var path = Path.Combine(_webHostEnvironment.ContentRootPath, "tmp");
                Directory.CreateDirectory(path);
                path = Path.Combine(path, trustedFileNameForFileStorage);
            
                using (var stream = new FileStream(path, FileMode.Create))
                {
                    await f.CopyToAsync(stream);
                }
                uploadedFiles.Add(path);
            }
            return Ok(new { uploadedFiles });
        }
        catch (Exception e)
        {
            _logger.LogError(e, e.Message);
            return BadRequest();
        }
    }
}