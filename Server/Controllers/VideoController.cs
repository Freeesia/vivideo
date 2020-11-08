using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Hangfire;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using StudioFreesia.Vivideo.Core;
using StudioFreesia.Vivideo.Server.Model;
using Microsoft.AspNetCore.Http;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using StudioFreesia.Vivideo.Server.Extensions;
using Microsoft.Extensions.Logging;

namespace StudioFreesia.Vivideo.Server.Controllers
{
    [Authorize]
    [ApiController]
    [Route("[controller]")]
    public class VideoController : ControllerBase
    {
        private readonly IBackgroundJobClient jobClient;
        private readonly ILogger<VideoController> logger;
        private readonly string inputDir;
        private readonly string outDir;

        public VideoController(IConfiguration config, IBackgroundJobClient jobClient, ILogger<VideoController> logger)
        {
            var content = config.GetSection("Content").Get<ContentDirSetting>();
            this.inputDir = content?.List ?? throw new ArgumentException();
            this.outDir = content?.Work ?? throw new ArgumentException();
            this.jobClient = jobClient;
            this.logger = logger;
        }

        [HttpPost("[action]")]
        public async ValueTask<string> Transcode([FromQuery] string? path)
        {
            var hash = GetHash(path ?? throw new ArgumentNullException(nameof(path)));
            var outPath = Path.Combine(this.outDir, hash, "master.mpd");
            if (System.IO.File.Exists(outPath))
            {
                return "/stream/" + hash;
            }
            this.jobClient.Enqueue<ITranscodeVideo>(t => t.Transcode(new TranscodeQueue(path, hash)));
            for (int i = 0; i < 10; i++)
            {
                await Task.Delay(1000);
                if (System.IO.File.Exists(outPath))
                {
                    break;
                }
            }
            return "/stream/" + hash;
        }

        [HttpGet("{*path}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesDefaultResponseType]
        public async Task<ActionResult<IEnumerable<ContentNode>>> List([FromRoute] string? path)
        {
            var dir = new DirectoryInfo(Path.Combine(this.inputDir, path ?? string.Empty));
            if (!dir.Exists)
            {
                return NotFound();
            }

            return await Task.WhenAll(dir.GetFileSystemInfos()
                .Where(i => !i.Name.StartsWith('.') && i switch
                {
                    DirectoryInfo _ => true,
                    FileInfo f => Path.GetExtension(f.FullName).Or(".mp4", ".avi"),
                    _ => throw new InvalidOperationException(),
                })
                .Select(i => Task.Run(() =>
                {
                    var path = Path.GetRelativePath(this.inputDir, i.FullName);
                    var hash = GetHash(path);
                    var outPath = Path.Combine(this.outDir, hash, "master.mpd");
                    var exists = System.IO.File.Exists(outPath);
                    this.logger.LogTrace("{0}:{1}:{2}", path, exists, hash);
                    return new ContentNode(path, i is DirectoryInfo, i.LastWriteTimeUtc, exists);
                })));
        }

        private string GetHash(string path)
        {
            using var md5 = MD5.Create();
            return BitConverter.ToString(md5.ComputeHash(Encoding.UTF8.GetBytes(path))).Replace("-", string.Empty);
        }
    }

    public class TranscodeRequest
    {
        public string? Path { get; set; }
    }
}
