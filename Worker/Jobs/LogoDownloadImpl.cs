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
            var ext = Path.GetExtension(queue.Url.LocalPath);
            var res = await client.GetAsync(queue.Url);
            res.EnsureSuccessStatusCode();
            using var stream = await res.Content.ReadAsStreamAsync();
            if (string.IsNullOrEmpty(ext))
            {
                var mt = res.Content.Headers.ContentType.MediaType.Split("/");
                ext = "." + mt[1];
            }
            using var fs = new FileStream(Path.Combine(this.listDir, queue.Output, "logo" + ext), FileMode.OpenOrCreate, FileAccess.Write);
            stream.CopyTo(fs);
        }
    }
}
