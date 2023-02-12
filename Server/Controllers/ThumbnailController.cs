using Hangfire;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Options;
using Nito.AsyncEx;
using StudioFreesia.Vivideo.Core;
using Xabe.FFmpeg;

namespace StudioFreesia.Vivideo.Server.Controllers;

[ApiController]
[Route("[controller]")]
public class ThumbnailController : ControllerBase
{
    private readonly string contentDir;
    private readonly IDistributedCache cache;
    private readonly DistributedCacheEntryOptions cacheOption;
    private readonly ILogger<ThumbnailController> logger;
    private readonly TimeSpan ssTime;

    public ThumbnailController(IOptions<ContentDirSetting> contentOptions, IDistributedCache cache, ILogger<ThumbnailController> logger)
    {
        var content = contentOptions.Value;
        this.contentDir = content?.List ?? throw new ArgumentException(nameof(content.List));
        this.cache = cache;
        this.cacheOption = new DistributedCacheEntryOptions()
            .SetSlidingExpiration(TimeSpan.FromDays(1));
        this.logger = logger;
        this.ssTime = TimeSpan.FromSeconds(5);
    }

    [HttpGet]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesDefaultResponseType]
    [ResponseCache(Location = ResponseCacheLocation.Client, Duration = 60 * 60 * 2)]
    public async Task<IActionResult> Get(string path, CancellationToken cancellationToken = default)
    {
        var fullPath = Path.Combine(this.contentDir, path);
        if (Directory.Exists(fullPath))
        {
            try
            {
                var logo = Directory.GetFiles(fullPath, "logo.*").FirstOrDefault();
                if (string.IsNullOrEmpty(logo))
                {
                    return NotFound();
                }
                return PhysicalFile(logo, GetContentType(logo));
            }
            catch (UnauthorizedAccessException)
            {
                return Unauthorized();
            }
        }
        else if (System.IO.File.Exists(fullPath))
        {
            var buf = await this.cache.GetAsync(path, cancellationToken);
            if (buf == null)
            {
                buf = await CreateVideoThumbnail(fullPath, cancellationToken);
                await this.cache.SetAsync(path, buf, this.cacheOption, cancellationToken);
            }
            else
            {
                await this.cache.RefreshAsync(path, cancellationToken);
            }
            return File(buf, "image/png");
        }
        return NotFound();
    }

    private async Task<byte[]> CreateVideoThumbnail(string path, CancellationToken cancellationToken = default)
    {
        using var ms = new MemoryStream();
        var name = Path.GetFileNameWithoutExtension(path);
        this.logger.LogDebug($"サムネイル生成開始:{name}");
        try
        {
            var info = await FFmpeg.GetMediaInfo(path, cancellationToken);
            var videoStream = info.VideoStreams.First()
                .SetCodec(VideoCodec.png)
                .SetSeek(this.ssTime > info.Duration ? TimeSpan.Zero : this.ssTime)
                .SetOutputFramesCount(1);
            var conv = FFmpeg.Conversions
                .New()
                .AddStream(videoStream)
                .SetOutputFormat(Format.rawvideo)
                .PipeOutput();
            conv.OnVideoDataReceived += (s, e) => ms.Write(e.Data);
            await conv.Start(cancellationToken);
        }
        catch (Exception e)
        {
            throw new Exception($"「{name}」のサムネイル出力に失敗しました", e);
        }
        this.logger.LogDebug($"サムネイル生成終了:{name}");
        return ms.ToArray();
    }

    private string GetContentType(string path)
    {
        var ext = Path.GetExtension((ReadOnlySpan<char>)path);
        if (ext.Length == 0)
        {
            this.logger.LogError($"Unknown Logo Image: {path}");
            return "application/octet-stream";
        }
        return "image/" + ext[1..].ToString();
    }

    [HttpPost]
    public void SetLogo([FromBody] LogoQueue queue, [FromServices] IBackgroundJobClient jobClient)
        => jobClient.Enqueue<ILogoDownload>(d => d.DownLoad(queue));
}
