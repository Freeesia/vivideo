using System;
using System.Diagnostics;
using System.IO;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Hosting.WindowsServices;

namespace StudioFreesia.Vivideo.Worker
{
    public static class HostBuilderExtensions
    {
        public static IHostBuilder UseContentRootForSingleFile(this IHostBuilder hostBuilder)
        {
            if (WindowsServiceHelpers.IsWindowsService())
            {
                using var process = Process.GetCurrentProcess();
                var pathToExe = process.MainModule.FileName;
                hostBuilder = hostBuilder.UseContentRoot(Path.GetDirectoryName(pathToExe));
            }
            return hostBuilder;
        }
    }
}
