using System;
using System.IO;
using Hangfire;
using Hangfire.Dashboard;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using StudioFreesia.Vivideo.Core;

namespace StudioFreesia.Vivideo.Server
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
            => this.Configuration = configuration;

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers();
            services.AddHangfire(config =>
            {
                config.UseRedisStorage(this.Configuration.GetConnectionString("Redis"));
            });
            services.AddDirectoryBrowser();
            services.AddSpaStaticFiles(config =>
            {
                config.RootPath = "Client";
            });

            if (Environment.GetEnvironmentVariable("ASPNETCORE_FORWARDEDHEADERS_ENABLED") == "true")
            {
                services.Configure<ForwardedHeadersOptions>(options =>
                {
                    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor |
                        ForwardedHeaders.XForwardedProto;
                    // Only loopback proxies are allowed by default.
                    // Clear that restriction because forwarders are enabled by explicit
                    // configuration.
                    options.KnownNetworks.Clear();
                    options.KnownProxies.Clear();
                });
            }
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, IBackgroundJobClient jobClient)
        {
            app.UseForwardedHeaders();
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            // app.UseHttpsRedirection();

            app.Map("/stream", app1 =>
            {
                var content = this.Configuration.GetSection("Content").Get<ContentDirSetting>();
                var work = content.Work ?? throw new InvalidOperationException($"{nameof(content.Work)}が指定されていません");
                Directory.CreateDirectory(work);
                app1.UseFileServer(new FileServerOptions()
                {
                    FileProvider = new PhysicalFileProvider(work),
                    EnableDirectoryBrowsing = env.IsDevelopment(),
                    EnableDefaultFiles = false,
                    StaticFileOptions =
                    {
                        ContentTypeProvider = new FileExtensionContentTypeProvider()
                        {
                            Mappings = {
                                [".mpd"] = "application/dash+xml",
                                [".m3u8"] = "application/x-mpegURL",
                                [".m4s"] = "video/iso.segment"
                            }
                        }
                    },
                });
            });

            app.UseRouting();
            // app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                endpoints.MapHangfireDashboard(new DashboardOptions()
                {
                    Authorization = Array.Empty<IDashboardAuthorizationFilter>(),
                    IsReadOnlyFunc = _ => true,
                });
            });

            app.UseSpa(_ => { });
        }
    }
}
