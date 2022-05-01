using System.IO.Compression;
using Hangfire;
using StackExchange.Redis;
using StudioFreesia.Vivideo.Core;
using StudioFreesia.Vivideo.Worker;
using StudioFreesia.Vivideo.Worker.Jobs;

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

await host.RunAsync();
