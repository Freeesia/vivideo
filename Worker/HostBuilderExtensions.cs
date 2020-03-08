using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Hosting.WindowsServices;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Configuration;
using Microsoft.Extensions.Options;
using Sentry.AspNetCore;
using Sentry.Extensions.Logging;

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

        public static IHostBuilder UseSentry(this IHostBuilder builder, string dsn)
            => builder.UseSentry(o => o.Dsn = dsn);

        public static IHostBuilder UseSentry(
            this IHostBuilder builder,
            Action<SentryLoggingOptions>? configureOptions = null) =>
            builder.ConfigureLogging((ctx, logging) =>
                logging.AddSentry(configureOptions));
    }
}
