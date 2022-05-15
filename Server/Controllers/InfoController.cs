using System.Diagnostics;
using System.Reflection;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace StudioFreesia.Vivideo.Server.Controllers;

[ApiController]
[AllowAnonymous]
[Route("[controller]/[action]")]
public class InfoController : ControllerBase
{
    [HttpGet]
    public string? Version()
        => FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly().Location).ProductVersion;

    [HttpGet]
    public ClientInfo ClientInfo([FromServices] IOptions<ClientInfo> options)
        => options.Value;
}

public class ClientInfo
{
    public string? ReCaptchaPublicKey { get; set; }
}
