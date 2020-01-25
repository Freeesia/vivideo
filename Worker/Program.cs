using Hangfire;
using StackExchange.Redis;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;
using StudioFreesia.Vivideo.Core;

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
                    services.AddHangfire(config =>
                            {
                                config.UseRedisStorage(ConnectionMultiplexer.Connect(hostContext.Configuration.GetConnectionString("Redis")));
                            })
                            .AddHangfireServer()
                            .AddTransient<ITranscodeVideo, TranscodeVideoImpl>();
                });
    }
}
