using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;

namespace StudioFreesia.Vivideo.Server
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                    webBuilder
                        .UseStartup<Startup>()
                        .UseSentry("https://6bd5217ab2e24414973357727d9df261@sentry.io/2409801"));
    }
}
