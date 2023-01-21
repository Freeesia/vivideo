using System.IO.Compression;
using Hangfire;
using StackExchange.Redis;
using StudioFreesia.Vivideo.Core;
using StudioFreesia.Vivideo.Worker.Extensions;
using StudioFreesia.Vivideo.Worker.Jobs;
using StudioFreesia.Vivideo.Worker.Model;

var host = Host.CreateDefaultBuilder(args)
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
            var con = hostContext.Configuration.GetConnectionString("Redis") ?? throw new InvalidOperationException();
            config.UseRedisStorage(ConnectionMultiplexer.Connect(con));
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

await host.RunAsync();
