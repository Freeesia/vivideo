using System.Diagnostics;
using Microsoft.Extensions.Hosting.WindowsServices;
using Sentry.Extensions.Logging;

namespace StudioFreesia.Vivideo.Worker.Extensions;

public static class HostBuilderExtensions
{
    public static IHostBuilder UseContentRootForSingleFile(this IHostBuilder hostBuilder)
    {
        if (WindowsServiceHelpers.IsWindowsService())
        {
            using var process = Process.GetCurrentProcess();
            var pathToExe = process?.MainModule?.FileName ?? throw new InvalidOperationException("モジュールが取れない");
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
