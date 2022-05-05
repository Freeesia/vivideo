using Microsoft.AspNetCore.Mvc;
using StudioFreesia.Vivideo.Server.Modules;

namespace StudioFreesia.Vivideo.Server.Controllers;

[ApiController]
[Route("[controller]")]
public class StreamController : ControllerBase
{
    private readonly ITranscodedCache cache;

    public StreamController(ITranscodedCache cache) => this.cache = cache;

    [HttpGet("{key}/{file}")]
    [ResponseCache(NoStore = true)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesDefaultResponseType]
    public async Task<IActionResult> Get([FromRoute] string key, [FromRoute] string file)
    {
        var buf = await this.cache.Get(key, file);
        if (buf.IsEmpty)
        {
            return NotFound();
        }
        return File(buf.ToArray(), Path.GetExtension(file) switch
        {
            ".mpd" => "application/dash+xml",
            ".m3u8" => "application/x-mpegURL",
            ".m4v" => "video/mp4",
            ".mp4" => "video/mp4",
            ".m4a" => "audio/mp4",
            ".m4s" => "video/iso.segment",
            ".init" => "video/mp4",
            ".header" => "video/mp4",
            ".ts" => "video/MP2T",
            _ => "application/octet-stream",
        });
    }

    [HttpPost("{key}/{file}")]
    public async Task Store([FromRoute] string key, [FromRoute] string file)
    {
        using var ms = new MemoryStream();
        await this.Request.BodyReader.CopyToAsync(ms);
        ReadOnlyMemory<byte> buf = ms.GetBuffer();
        await this.cache.Set(key, file, buf[..(int)ms.Length]);
    }
}
