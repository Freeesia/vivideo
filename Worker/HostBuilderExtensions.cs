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
            Action<SentryAspNetCoreOptions> configureOptions)
            => builder.UseSentry((context, options) => configureOptions?.Invoke(options));

        public static IHostBuilder UseSentry(
            this IHostBuilder builder,
            Action<HostBuilderContext, SentryAspNetCoreOptions>? configureOptions = null) =>
            builder.ConfigureLogging((ctx, logging) =>
            {
                logging.AddConfiguration();
                var section = ctx.Configuration.GetSection("Sentry");
                logging.Services.Configure<SentryAspNetCoreOptions>(section);

                if (configureOptions != null)
                {
                    logging.Services.Configure<SentryAspNetCoreOptions>(op =>
                    {
                        configureOptions(ctx, op);
                    });
                }
                logging.AddSentry();
            });
    }
}
