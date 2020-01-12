using System;
using System.Threading.Tasks;
using Hangfire;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using StudioFreesia.Vivideo.Core;

namespace StrudioFreesia.Vivideo.Server
{
    [ApiController]
    [Route("[controller]/[action]")]
    public class VideoController
    {
        private readonly IBackgroundJobClient jobClient;
        private readonly string inputDir;

        public VideoController(IConfiguration config, IBackgroundJobClient jobClient)
        {
            var content = config.GetSection("Content").Get<ContentDirSetting>();
            this.inputDir = content?.List ?? throw new ArgumentException();
            this.jobClient = jobClient;
        }

        [HttpPost]
        public string Transcode([FromBody]string path)
        {
            var queue = new TranscodeQueue(path, HashCode.Combine(path).ToString());
            this.jobClient.Enqueue<ITranscodeVideo>(t => t.Transcode(queue));
            return queue.Output;
        }
    }
}
