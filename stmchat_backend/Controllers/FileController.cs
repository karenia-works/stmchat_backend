using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using stmchat_backend.Services;

namespace stmchat_backend.Controllers
{
    [Route("api/v1/[controller]")]
    [ApiController]
    public class FileController : ControllerBase
    {
        private readonly FileService _service;

        public FileController(FileService service)
        {
            _service = service;
        }

        [HttpGet("{fileId}")]
        public async Task<IActionResult> Get(string fileId)
        {
            var res = await _service.GetFileInfo(fileId);
            if (res == null)
            {
                return NotFound();
            }

            return Ok(fileId);
        }

        [HttpPost, DisableRequestSizeLimit]
        public async Task<IActionResult> Post()
        {
            var files = Request.Form.Files;
            var fileResultList = new List<string>();

            if (files.Any(f => f.Length == 0))
            {
                return BadRequest();
            }

            foreach (var file in files)
            {
                var fileName = $@"{FileHash(file)}{Path.GetExtension(file.FileName)}";

                var fullPath = Path.Combine("data", fileName);
                await using var stream = new FileStream(fullPath, FileMode.Create);
                await file.CopyToAsync(stream);
                await _service.SaveInfo(fileName);
                fileResultList.Add(await _service.GetFileUri(fileName));
            }

            return Ok(new {fileResultList});
        }

        private string FileHash(IFormFile file)
        {
            MemoryStream stream = new MemoryStream();
            file.OpenReadStream().CopyTo(stream);

            byte[] bytes = MD5.Create().ComputeHash(stream.ToArray());
            return BitConverter.ToString(bytes).Replace("-", string.Empty).ToLower();
        }
    }
}