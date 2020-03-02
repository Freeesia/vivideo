using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using StudioFreesia.Vivideo.Core;

namespace StudioFreesia.Vivideo.Worker.Jobs
{
    public class LogoDownloadImpl : ILogoDownload
    {
        public string listDir { get; }

        private readonly ILogger<LogoDownloadImpl> logger;
        private readonly IHttpClientFactory httpClientFactory;

        public LogoDownloadImpl(IConfiguration config, ILogger<LogoDownloadImpl> logger, IHttpClientFactory httpClientFactory)
        {
            var content = config.GetSection("Content").Get<ContentDirSetting>();
            this.listDir = content?.List ?? throw new ArgumentException();
            this.logger = logger;
            this.httpClientFactory = httpClientFactory;
        }

        public async Task DownLoad(LogoQueue queue)
        {
            var client = this.httpClientFactory.CreateClient();
            this.logger.LogInformation("path: {0} target: {1}", queue.Output, queue.Url);
            using var stream = await client.GetStreamAsync(queue.Url);
            using var fs = new FileStream(Path.Combine(this.listDir, queue.Output, "logo" + Path.GetExtension(queue.Url.LocalPath)), FileMode.Create, FileAccess.Write);
            stream.CopyTo(fs);
        }
    }
}
