using System;
using System.Threading.Tasks;
using Hangfire;
using Microsoft.AspNetCore.Mvc;
using StudioFreesia.Vivideo.Core;

namespace StrudioFreesia.Vivideo.Server
{
    [ApiController]
    [Route("[controller]/[action]")]
    public class VideoController
    {
        private readonly IBackgroundJobClient jobClient;

        public VideoController(IBackgroundJobClient jobClient)
            => this.jobClient = jobClient;

        [HttpPost]
        public string Transcode([FromBody]string path)
        {
            var queue = new TranscodeQueue(path, HashCode.Combine(path).ToString());
            this.jobClient.Enqueue<ITranscodeVideo>(t => t.Transcode(queue));
            return queue.Output;
        }
    }
}
