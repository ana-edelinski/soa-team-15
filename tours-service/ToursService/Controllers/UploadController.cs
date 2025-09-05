using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ToursService.Controllers
{
    [ApiController]
    [Authorize] // treba token
    [Route("api/uploads/reviews")]
    public class UploadController : ControllerBase
    {
        private readonly IWebHostEnvironment _env;
        public UploadController(IWebHostEnvironment env) { _env = env; }

        [HttpPost]
        [RequestSizeLimit(25_000_000)] // 25 MB
        public async Task<ActionResult<List<string>>> Upload([FromForm] List<IFormFile> files)
        {
            if (files == null || files.Count == 0) return BadRequest("No files.");
            var webRoot = _env.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
            var target = Path.Combine(webRoot, "uploads", "reviews");
            Directory.CreateDirectory(target);

            var urls = new List<string>();
            foreach (var f in files)
            {
                var ext = Path.GetExtension(f.FileName);
                var name = $"{Guid.NewGuid()}{ext}";
                var full = Path.Combine(target, name);
                await using var s = System.IO.File.Create(full);
                await f.CopyToAsync(s);
                urls.Add($"/uploads/reviews/{name}");
            }
            return Ok(urls);
        }
    }
}
