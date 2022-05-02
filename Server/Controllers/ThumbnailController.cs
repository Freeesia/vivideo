using System.Diagnostics;
using Hangfire;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Options;
using StudioFreesia.Vivideo.Core;
using StudioFreesia.Vivideo.Server.Model;

namespace StudioFreesia.Vivideo.Server.Controllers;

[ApiController]
[Route("[controller]")]
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

    public ThumbnailController(IOptions<ContentDirSetting> contentOptions, IOptions<ThumbnailSetting> thumbOptions, IHostEnvironment env, IDistributedCache cache, ILogger<ThumbnailController> logger)
    {
        var content = contentOptions.Value;
        this.contentDir = content?.List ?? throw new ArgumentException(nameof(content.List));
        var trans = thumbOptions.Value;
        this.file = trans.File ?? throw new ArgumentException(nameof(trans.File));
        this.args = trans.Args ?? throw new ArgumentException(nameof(trans.Args));
        this.tmpDir = Path.Combine(Path.GetTempPath(), "VivideoThumb");
        Directory.CreateDirectory(this.tmpDir);
        this.rootDir = env.ContentRootPath;
        this.cache = cache;
        this.cacheOption = new DistributedCacheEntryOptions()
            .SetSlidingExpiration(TimeSpan.FromDays(1));
        this.logger = logger;
    }

    [HttpGet]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesDefaultResponseType]
    [ResponseCache(Location = ResponseCacheLocation.Client, Duration = 60 * 60 * 2)]
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
            return PhysicalFile(logo, GetContentType(logo));
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
        this.logger.LogDebug($"サムネイル生成開始:{name}");
        using var p = Process.Start(info);
        p.ErrorDataReceived += (s, e) =>
            {
                if (!string.IsNullOrEmpty(e.Data))
                {
                    this.logger.LogInformation(e.Data);
                }
            };
        p.BeginErrorReadLine();
        p.WaitForExit();
        if (!System.IO.File.Exists(tmp))
        {
            throw new Exception($"「{name}」のサムネイル出力に失敗しました");
        }
        this.logger.LogDebug($"サムネイル生成終了:{name}");
        return await System.IO.File.ReadAllBytesAsync(tmp);
    }

    private string GetContentType(string path)
    {
        var ext = Path.GetExtension((ReadOnlySpan<char>)path);
        if (ext.Length == 0)
        {
            this.logger.LogError("Unknown Logo Image: {path}", path);
            return "application/octet-stream";
        }
        return "image/" + ext[1..].ToString();
    }

    [HttpPost]
    public void SetLogo([FromBody] LogoQueue queue, [FromServices] IBackgroundJobClient jobClient)
        => jobClient.Enqueue<ILogoDownload>(d => d.DownLoad(queue));
}
