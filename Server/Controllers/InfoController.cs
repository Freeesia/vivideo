using System.Diagnostics;
using System.Reflection;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace StudioFreesia.Vivideo.Server.Controllers;

[ApiController]
[AllowAnonymous]
[Route("[controller]/[action]")]
public class InfoController : ControllerBase
{
    [HttpGet]
    public string? Version()
        => FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly().Location).ProductVersion;
}
