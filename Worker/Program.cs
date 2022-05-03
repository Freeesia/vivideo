using System.IO.Compression;
using Hangfire;
using StackExchange.Redis;
using StudioFreesia.Vivideo.Core;
using StudioFreesia.Vivideo.Worker;
using StudioFreesia.Vivideo.Worker.Jobs;
using StudioFreesia.Vivideo.Worker.Model;
using Xabe.FFmpeg;
using Xabe.FFmpeg.Downloader;

var host = Host.CreateDefaultBuilder(args)
    .UseWindowsService()
    .UseContentRootForSingleFile()
#if !DEBUG
    .UseSentry(op => {
        op.Dsn = "https://6bd5217ab2e24414973357727d9df261@sentry.io/2409801";
        op.RequestBodyCompressionLevel = CompressionLevel.Optimal;
        op.MinimumEventLevel = LogLevel.Error;
        op.Environment = "Production";
    })
#endif
    .ConfigureServices((hostContext, services) =>
    {
        services.Configure<ContentDirSetting>(hostContext.Configuration.GetSection("Content"));
        services.Configure<TranscodeSetting>(hostContext.Configuration.GetSection("Transcode"));

        services.AddHttpClient();
        services.AddHangfire(config =>
            {
                config.UseRedisStorage(ConnectionMultiplexer.Connect(hostContext.Configuration.GetConnectionString("Redis")));
            })
            .AddHangfireServer(config =>
            {
                // 同時に1つしかトランスコード出来ないので
                config.WorkerCount = 1;
            })
            .AddTransient<ITranscodeVideo, TranscodeVideoImpl>()
            .AddTransient<ILogoDownload, LogoDownloadImpl>();
    })
    .Build();


await using (var scope = host.Services.CreateAsyncScope())
{
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<FFmpegDownloader>>();
    await FFmpegDownloader.GetLatestVersion(
        FFmpegVersion.Official,
        "ffmpeg",
        new Progress<ProgressInfo>(p => logger.LogTrace($"{p.DownloadedBytes * 100f / p.TotalBytes:f2}% {p.DownloadedBytes / 1024f:f2}/{p.TotalBytes / 1024f:f2} MB")));
}

await host.RunAsync();
