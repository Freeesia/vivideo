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

namespace StudioFreesia.Vivideo.Server.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class VideoController : ControllerBase, IDisposable
    {
        private readonly IBackgroundJobClient jobClient;
        private readonly MD5 md5;
        private readonly string inputDir;
        private readonly string outDir;

        public VideoController(IConfiguration config, IBackgroundJobClient jobClient)
        {
            var content = config.GetSection("Content").Get<ContentDirSetting>();
            this.inputDir = content?.List ?? throw new ArgumentException();
            this.outDir = content?.Work ?? throw new ArgumentException();
            this.jobClient = jobClient;
            this.md5 = MD5.Create();
        }

        public void Dispose()
            => this.md5.Dispose();

        [HttpPost("[action]")]
        public async ValueTask<string> Transcode([FromQuery]string path)
        {
            var hash = BitConverter.ToString(
                this.md5.ComputeHash(
                    Encoding.UTF8.GetBytes(
                        path ?? throw new ArgumentException())))
                .Replace("-", string.Empty);
            var outPath = Path.Combine(this.outDir, hash, "master.mpd");
            if (!System.IO.File.Exists(outPath))
            {
                this.jobClient.Enqueue<ITranscodeVideo>(t => t.Transcode(new TranscodeQueue(path, hash)));
                for (int i = 0; i < 10; i++)
                {
                    await Task.Delay(1000);
                    if (System.IO.File.Exists(outPath))
                    {
                        break;
                    }
                }
            }
            return "/stream/" + hash;
        }

        [HttpGet("{*path}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesDefaultResponseType]
        public ActionResult<IEnumerable<ContentNode>> List([FromRoute]string? path)
        {
            var dir = new DirectoryInfo(Path.Combine(this.inputDir, path ?? string.Empty));
            if (!dir.Exists)
            {
                return NotFound();
            }

            return Ok(dir.GetFileSystemInfos()
                .Where(i => !i.Name.StartsWith('.') && i switch
                {
                    DirectoryInfo _ => true,
                    FileInfo f => Path.GetExtension(f.FullName).Or(".mp4", ".avi"),
                    _ => throw new InvalidOperationException(),
                })
                .Select(i => new ContentNode(Path.GetRelativePath(this.inputDir, i.FullName), i is DirectoryInfo, i.CreationTimeUtc)));
        }
    }

    public class TranscodeRequest
    {
        public string? Path { get; set; }
    }
}
