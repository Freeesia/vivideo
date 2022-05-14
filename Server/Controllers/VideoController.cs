using System.Security.Cryptography;
using System.Text;
using Hangfire;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using StudioFreesia.Vivideo.Core;
using StudioFreesia.Vivideo.Server.Extensions;
using StudioFreesia.Vivideo.Server.Model;
using StudioFreesia.Vivideo.Server.Modules;
using ValueTaskSupplement;
using Xabe.FFmpeg;

namespace StudioFreesia.Vivideo.Server.Controllers;

[Authorize]
[ApiController]
[Route("[controller]")]
public class VideoController : ControllerBase
{
    private readonly ITranscodedCache cache;
    private readonly IBackgroundJobClient jobClient;
    private readonly ILogger<VideoController> logger;
    private readonly string inputDir;

    public VideoController(ITranscodedCache cache, IOptions<ContentDirSetting> options, IBackgroundJobClient jobClient, ILogger<VideoController> logger)
    {
        var setting = options.Value;
        this.inputDir = setting.List ?? throw new ArgumentException(nameof(setting.List));
        this.cache = cache;
        this.jobClient = jobClient;
        this.logger = logger;
    }

    [HttpPost("[action]")]
    public async ValueTask<string> Transcode([FromQuery] string? path)
    {
        var hash = GetHash(path ?? throw new ArgumentNullException(nameof(path)));
        var output = "/api/stream/" + hash;
        if (await this.cache.Exist(hash))
        {
            return output;
        }
        this.jobClient.Enqueue<ITranscodeVideo>(t => t.Transcode(new(path, output)));
        for (int i = 0; i < 10; i++)
        {
            await Task.Delay(1000);
            if (await this.cache.Exist(hash))
            {
                break;
            }
        }
        return output;
    }

    [HttpPost("transcode/all/{*path}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesDefaultResponseType]
    public async ValueTask<IActionResult> TranscodeAll([FromQuery] string? path)
    {
        if (string.IsNullOrEmpty(path))
        {
            return BadRequest();
        }
        var dirPath = Path.Combine(this.inputDir, path);
        if (!Directory.Exists(dirPath))
        {
            return NotFound();
        }
        var files = Directory.GetFiles(dirPath, "*", SearchOption.AllDirectories)
            .Where(f => Path.GetExtension(f).Or(".mp4", ".avi"));

        foreach (var filePath in files)
        {
            var relPath = Path.GetRelativePath(this.inputDir, filePath);
            var hash = GetHash(relPath);
            if (await this.cache.Exist(hash))
            {
                continue;
            }
            this.jobClient.Enqueue<ITranscodeVideo>(t => t.Transcode(new(relPath, hash)));
        }

        return Ok();
    }

    [HttpGet("{*path}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesDefaultResponseType]
    public async IAsyncEnumerable<ContentNode> List([FromRoute] string? path)
    {
        var dir = new DirectoryInfo(Path.Combine(this.inputDir, path ?? string.Empty));
        if (!dir.Exists)
        {
            yield break;
        }

        var nodes = dir.GetFileSystemInfos()
            .Where(i => !i.Name.StartsWith('.') && i switch
            {
                DirectoryInfo _ => true,
                FileInfo f => Path.GetExtension(f.FullName).Or(".mp4", ".avi", ".wmv"),
                _ => throw new InvalidOperationException(),
            })
            .Select(i => Task.Run(async () =>
            {
                var path = Path.GetRelativePath(this.inputDir, i.FullName);
                var hash = GetHash(path);
                var (exists, duration) = await ValueTaskEx.WhenAll(this.cache.Exist(hash), GetVideoDuration(i));
                this.logger.LogTrace($"{path}:{exists}:{hash}:{duration}");
                return new ContentNode(path, i is DirectoryInfo, i.LastWriteTimeUtc, exists, duration.TotalSeconds);
            }))
            .ToArray();

        await foreach (var node in nodes.WhenEach().ConfigureAwait(false))
        {
            yield return node;
        }
    }

    private static async ValueTask<TimeSpan> GetVideoDuration(FileSystemInfo fsInfo, CancellationToken cancellationToken = default)
    {
        if (fsInfo is FileInfo)
        {
            var info = await FFmpeg.GetMediaInfo(fsInfo.FullName, cancellationToken);
            return info.Duration;
        }
        else
        {
            return TimeSpan.Zero;
        }
    }

    [HttpDelete("{*path}")]
    public async Task DeleteVideoCache([FromRoute] string path)
    {
        var key = GetHash(path);
        if (await this.cache.Exist(key))
        {
            await this.cache.Delete(key);
        }
    }

    private static string GetHash(string path)
        => BitConverter.ToString(MD5.HashData(Encoding.UTF8.GetBytes(path))).Replace("-", string.Empty);
}
