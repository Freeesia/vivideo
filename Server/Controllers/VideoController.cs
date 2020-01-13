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

namespace StrudioFreesia.Vivideo.Server
{
    [ApiController]
    [Route("[controller]")]
    public class VideoController : ControllerBase
    {
        private readonly IBackgroundJobClient jobClient;
        private readonly string inputDir;

        public VideoController(IConfiguration config, IBackgroundJobClient jobClient)
        {
            var content = config.GetSection("Content").Get<ContentDirSetting>();
            this.inputDir = content?.List ?? throw new ArgumentException();
            this.jobClient = jobClient;
        }

        [HttpPost("[action]")]
        public string Transcode([FromBody]string path)
        {
            var queue = new TranscodeQueue(path, HashCode.Combine(path).ToString());
            this.jobClient.Enqueue<ITranscodeVideo>(t => t.Transcode(queue));
            return "/stream/" + queue.Output;
        }

        [HttpGet("{*path}")]
        public ActionResult<IEnumerable<ContentNode>> List([FromRoute]string? path)
        {
            var dir = new DirectoryInfo(Path.Combine(this.inputDir, path ?? string.Empty));
            if (!dir.Exists)
            {
                return NotFound();
            }

            return Ok(dir.GetFileSystemInfos()
                .Where(i => !i.Name.StartsWith('.'))
                .Select(i => new ContentNode(Path.GetRelativePath(this.inputDir, i.FullName), i is DirectoryInfo)));
        }
    }
}
