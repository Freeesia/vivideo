using Hangfire;
using StackExchange.Redis;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;
using StudioFreesia.Vivideo.Core;
using StudioFreesia.Vivideo.Worker.Jobs;

namespace StudioFreesia.Vivideo.Worker
{
    public class Program
    {
        public static void Main(string[] args)
            => CreateHostBuilder(args).Build().Run();

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .UseWindowsService()
                .UseContentRootForSingleFile()
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
                });
    }
}
