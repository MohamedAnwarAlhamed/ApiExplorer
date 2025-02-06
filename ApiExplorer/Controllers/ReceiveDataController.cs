using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace ApiExplorer.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ReceiveDataController : ControllerBase
    {
        [HttpPost("via-form")]
        public IActionResult PostViaForm([FromForm] string name, [FromForm] int age)
        {
            if (string.IsNullOrEmpty(name) || age <= 0)
            {
                return BadRequest("Name and age are required.");
            }

            return Ok($"Received name: {name}, age: {age}");
        }

        [HttpGet("via-header")]
        public IActionResult GetViaHeader([FromHeader(Name = "Custom-Header")] string customHeaderValue)
        {
            if (string.IsNullOrEmpty(customHeaderValue))
            {
                return BadRequest("Custom-Header is required.");
            }

            return Ok($"Received value from header: {customHeaderValue}");
        }

        [HttpGet("via-query")]
        public IActionResult GetViaQuery([FromQuery] string paramValue)
        {
            if (string.IsNullOrEmpty(paramValue))
            {
                return BadRequest("paramValue is required.");
            }

            return Ok($"Received value from query string: {paramValue}");
        }

        [HttpGet("via-route/{paramValue}")]
        public IActionResult GetViaRoute([FromRoute] string paramValue)
        {
            if (string.IsNullOrEmpty(paramValue))
            {
                return BadRequest("paramValue is required.");
            }

            return Ok($"Received value from route: {paramValue}");
        }

        [HttpPost("via-header-query-or-body")]
        public IActionResult PostViaHeaderQueryOrBody(
    [FromHeader(Name = "Custom-Header")] string customHeaderValue,
    [FromQuery] string paramValue,
    [FromBody] dynamic model)
        {
            if (string.IsNullOrEmpty(customHeaderValue) && string.IsNullOrEmpty(paramValue) && model == null)
            {
                return BadRequest("Either Custom-Header, paramValue, or model is required.");
            }

            var receivedValue = !string.IsNullOrEmpty(customHeaderValue) ? customHeaderValue :
                                !string.IsNullOrEmpty(paramValue) ? paramValue :
                                JsonSerializer.Serialize(model);

            return Ok($"Received value: {receivedValue}");
        }

        [HttpGet("from-header")]
        public IActionResult GetDataFromHeader()
        {
            // قراءة القيمة من الهيدر
            if (Request.Headers.TryGetValue("My-Custom-Header", out var headerValue))
            {
                return Ok(new { Message = "Header received", Value = headerValue.ToString() });
            }

            return BadRequest(new { Message = "Header is missing" });
        }

        [HttpPost("upload-file")]
        //[RequestSizeLimit(10 * 1024 * 1024)] // 10 MB
        public async Task<IActionResult> UploadFile(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest("No file uploaded.");
            }

            // التحقق من نوع الملف (اختياري)
            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".pdf" };
            var fileExtension = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!allowedExtensions.Contains(fileExtension))
            {
                return BadRequest("Invalid file type. Only JPG, JPEG, PNG, and PDF are allowed.");
            }

            if (!Directory.Exists("Uploads"))
            {
                Directory.CreateDirectory("Uploads");
            }
            // حفظ الملف على الخادم
            var filePath = Path.Combine("Uploads", file.FileName);
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }
            var fileUrl = $"{Request.Scheme}://{Request.Host}/Uploads/{file.FileName}";
            return Ok(new { Message = "File uploaded successfully", FileUrl = fileUrl });

            //return Ok($"File uploaded successfully: {file.FileName}, Size: {file.Length} bytes");
        }

        [HttpPost("upload-multiple-files")]
        public async Task<IActionResult> UploadMultipleFiles(List<IFormFile> files)
        {
            if (files == null || files.Count == 0)
            {
                return BadRequest("No files uploaded.");
            }

            var uploadedFiles = new List<string>();

            foreach (var file in files)
            {
                if (file.Length > 0)
                {
                    var filePath = Path.Combine("Uploads", file.FileName);
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await file.CopyToAsync(stream);
                    }
                    uploadedFiles.Add(file.FileName);
                }
            }

            return Ok($"Files uploaded successfully: {string.Join(", ", uploadedFiles)}");
        }
    }
}
