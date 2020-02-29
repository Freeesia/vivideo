using System.Diagnostics;
using System.Reflection;
using Microsoft.AspNetCore.Mvc;

namespace StudioFreesia.Vivideo.Server
{
    [ApiController]
    [Produces("application/json")]
    [Route("api/[controller]/[action]")]
    public class InfoController : ControllerBase
    {
        [HttpGet]
        public string Version()
            => FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly().Location).ProductVersion;
    }
}
