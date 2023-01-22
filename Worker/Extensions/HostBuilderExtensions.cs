using Sentry.Extensions.Logging;

namespace StudioFreesia.Vivideo.Worker.Extensions;

public static class HostBuilderExtensions
{
    public static IHostBuilder UseSentry(this IHostBuilder builder, string dsn)
        => builder.UseSentry(o => o.Dsn = dsn);

    public static IHostBuilder UseSentry(
        this IHostBuilder builder,
        Action<SentryLoggingOptions>? configureOptions = null) =>
        builder.ConfigureLogging((ctx, logging) =>
            logging.AddSentry(configureOptions));
}
