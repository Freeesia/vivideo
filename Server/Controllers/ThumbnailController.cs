using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using StudioFreesia.Vivideo.Core;
using Microsoft.AspNetCore.Http;
using System.IO;
using System.Linq;

namespace StudioFreesia.Vivideo.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ThumbnailController : ControllerBase
    {
        private readonly string contentDir;

        public ThumbnailController(IConfiguration config)
        {
            var content = config.GetSection("Content").Get<ContentDirSetting>();
            this.contentDir = content?.List ?? throw new ArgumentException();
        }

        [HttpGet("{*path}")]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesDefaultResponseType]
        public async Task<IActionResult> Get(string path)
        {
            var fullPath = Path.Combine(this.contentDir, path);
            if (Directory.Exists(fullPath))
            {
                var logo = Directory.GetFiles(fullPath, "logo.*").FirstOrDefault();
                if (string.IsNullOrEmpty(logo))
                {
                    return NotFound();
                }
                return PhysicalFile(logo, "image/" + Path.GetExtension(logo));
            }
            return NotFound();
        }
    }
}
