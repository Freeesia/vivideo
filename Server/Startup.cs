using System;
using Hangfire;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
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
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, IBackgroundJobClient jobClient)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            // app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            app.UseFileServer();
            var content = this.Configuration.GetSection("Content").Get<ContentDirSetting>();
            var work = content.Work ?? throw new InvalidOperationException($"{nameof(content.Work)}が指定されていません");
            app.UseFileServer(new FileServerOptions()
            {
                FileProvider = new PhysicalFileProvider(work),
                RequestPath = "/stream",
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


            app.UseHangfireDashboard();


            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
