using Microsoft.Extensions.Options;
using StudioFreesia.Vivideo.Core;

namespace StudioFreesia.Vivideo.Worker.Jobs;

public class LogoDownloadImpl : ILogoDownload
{
    public string listDir { get; }

    private readonly ILogger<LogoDownloadImpl> logger;
    private readonly IHttpClientFactory httpClientFactory;

    public LogoDownloadImpl(IOptions<ContentDirSetting> contentOptions, ILogger<LogoDownloadImpl> logger, IHttpClientFactory httpClientFactory)
    {
        var content = contentOptions.Value;
        this.listDir = content?.List ?? throw new ArgumentException(nameof(content.List));
        this.logger = logger;
        this.httpClientFactory = httpClientFactory;
    }

    public async Task DownLoad(LogoQueue queue)
    {
        var client = this.httpClientFactory.CreateClient();
        this.logger.LogInformation($"path: {queue.Output} target: {queue.Url}");
        var ext = Path.GetExtension(queue.Url.LocalPath);
        var res = await client.GetAsync(queue.Url);
        res.EnsureSuccessStatusCode();
        using var stream = await res.Content.ReadAsStreamAsync();
        if (string.IsNullOrEmpty(ext) && !string.IsNullOrEmpty(res.Content.Headers.ContentType?.MediaType))
        {
            var mt = res.Content.Headers.ContentType.MediaType.Split("/");
            ext = "." + mt[1];
        }
        using var fs = new FileStream(Path.Combine(this.listDir, queue.Output, "logo" + ext), FileMode.OpenOrCreate, FileAccess.Write);
        stream.CopyTo(fs);
    }
}
