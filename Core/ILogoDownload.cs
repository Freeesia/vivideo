using System;
using System.Threading.Tasks;

namespace StudioFreesia.Vivideo.Core
{
    public interface ILogoDownload
    {
        Task DownLoad(LogoQueue queue);
    }
}
