using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using StudioFreesia.Vivideo.Core;
using Microsoft.AspNetCore.Http;
using System.IO;
using System.Linq;
using StudioFreesia.Vivideo.Server.Model;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Hosting;
using System.Diagnostics;
using Microsoft.Extensions.Caching.Distributed;

namespace StudioFreesia.Vivideo.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ThumbnailController : ControllerBase
    {
        private readonly string contentDir;
        private readonly string file;
        private readonly string args;
        private readonly string tmpDir;
        private readonly string rootDir;
        private readonly IDistributedCache cache;
        private readonly DistributedCacheEntryOptions cacheOption;
        private readonly ILogger<ThumbnailController> logger;

        public ThumbnailController(IConfiguration config, IHostEnvironment env, IDistributedCache cache, ILogger<ThumbnailController> logger)
        {
            var content = config.GetSection("Content").Get<ContentDirSetting>();
            this.contentDir = content?.List ?? throw new ArgumentException();
            var trans = config.GetSection("Thumbnail").Get<ThumbnailSetting>();
            this.file = trans.File ?? throw new ArgumentException();
            this.args = trans.Args ?? throw new ArgumentException();
            this.tmpDir = Path.Combine(Path.GetTempPath(), "VivideoThumb");
            Directory.CreateDirectory(this.tmpDir);
            this.rootDir = env.ContentRootPath;
            this.cache = cache;
            this.cacheOption = new DistributedCacheEntryOptions()
                .SetSlidingExpiration(TimeSpan.FromDays(1));
            this.logger = logger;
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
                return PhysicalFile(logo, "image/" + Path.GetExtension((ReadOnlySpan<char>)logo).Slice(1).ToString());
            }
            else if (System.IO.File.Exists(fullPath))
            {
                var buf = await this.cache.GetAsync(path);
                if (buf == null)
                {
                    buf = await Task.Run(async () => await CreateVideoThumbnail(fullPath));
                    await this.cache.SetAsync(path, buf, this.cacheOption);
                }
                else
                {
                    await this.cache.RefreshAsync(path);
                }
                return File(buf, "image/png");
            }
            return NotFound();
        }

        private async Task<byte[]> CreateVideoThumbnail(string path)
        {
            var name = Path.GetFileNameWithoutExtension(path);
            var tmp = Path.Combine(this.tmpDir, Path.GetRandomFileName() + ".png");
            var info = new ProcessStartInfo(this.file)
            {
                CreateNoWindow = true,
                UseShellExecute = false,
                WorkingDirectory = this.rootDir,
                RedirectStandardError = true,
                Arguments = string.Format(this.args, path.Replace('\\', '/'), tmp.Replace('\\', '/')),
            };
            this.logger.LogDebug("サムネイル生成開始:{0}", name);
            using var p = Process.Start(info);
            p.ErrorDataReceived += (s, e) =>
                {
                    if (!string.IsNullOrEmpty(e.Data))
                    {
                        this.logger.LogError(e.Data);
                    }
                };
            p.BeginErrorReadLine();
            p.WaitForExit();
            this.logger.LogDebug("サムネイル生成終了:{0}", name);
            if (!System.IO.File.Exists(tmp))
            {
                throw new Exception($"「{name}」のサムネイル出力に失敗しました");
            }
            return await System.IO.File.ReadAllBytesAsync(tmp);
        }
    }
}
