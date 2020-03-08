using System;
using System.IO;
using AspNetCore.Firebase.Authentication.Extensions;
using Hangfire;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using StudioFreesia.Vivideo.Core;
using StudioFreesia.Vivideo.Server.ComponentModel;

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
            services.AddControllers()
                .AddNewtonsoftJson();
            services.AddHangfire(config =>
            {
                config.UseRedisStorage(this.Configuration.GetConnectionString("Redis"));
            });
            services.AddDirectoryBrowser();
            services.AddSpaStaticFiles(config =>
            {
                config.RootPath = "Client";
            });

            var firebase = this.Configuration.GetSection("FirebaseAuthentication");
            services.AddFirebaseAuthentication(firebase.GetValue<string>("Issuer"), firebase.GetValue<string>("Audience"));
            services.AddAuthorization(op =>
            {
                op.DefaultPolicy = new AuthorizationPolicyBuilder(JwtBearerDefaults.AuthenticationScheme)
                    .RequireAuthenticatedUser()
                    .RequireClaim("invitationCodeVerified", "true")
                    .Build();
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

            services.AddCors(options =>
            {
                options.AddDefaultPolicy(builder =>
                {
                    builder.WithOrigins("http://reference.dashif.org");
                });
            });

            services.AddDistributedMemoryCache();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseForwardedHeaders();
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            // app.UseHttpsRedirection();

            app.UseSpaStaticFiles();

            app.Map("/stream", app1 =>
            {
                // app1.UseCors();
                var content = this.Configuration.GetSection("Content").Get<ContentDirSetting>();
                var work = content.Work ?? throw new InvalidOperationException($"{nameof(content.Work)}が指定されていません");
                Directory.CreateDirectory(work);
                app1.UseFileServer(new FileServerOptions()
                {
                    FileProvider = new PhysicalFileProvider(work)
                    {
                        UseActivePolling = true,
                        UsePollingFileWatcher = true,
                    },
                    EnableDirectoryBrowsing = env.IsDevelopment(),
                    EnableDefaultFiles = false,
                    StaticFileOptions =
                    {
                        ContentTypeProvider = new FileExtensionContentTypeProvider()
                        {
                            Mappings = {
                                [".mpd"] = "application/dash+xml",
                                [".m3u8"] = "application/x-mpegURL",
                            }
                        },
                        OnPrepareResponse = ctx => {
                            ctx.Context.Response.Headers["Cache-Control"] = "public, no-store";
                        },
                        ServeUnknownFileTypes = true,
                    },
                });
            });

            app.UseRouting();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                endpoints.MapHangfireDashboard(new DashboardOptions()
                {
                    Authorization = new []{ new HangfireDashbordAuthFilter() },
                });
            });

            app.UseSpa(_ => { });
        }
    }
}
