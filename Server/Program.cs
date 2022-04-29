
using AspNetCore.Firebase.Authentication.Extensions;
using Hangfire;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.FileProviders;
using StudioFreesia.Vivideo.Core;
using StudioFreesia.Vivideo.Server.ComponentModel;

var builder = WebApplication.CreateBuilder(args);

#if !DEBUG
builder.WebHost.UseSentry("https://6bd5217ab2e24414973357727d9df261@sentry.io/2409801");
#endif

builder.Services.AddControllers()
    .AddNewtonsoftJson();
builder.Services.AddHangfire(config
    => config.UseRedisStorage(builder.Configuration.GetConnectionString("Redis")));
builder.Services.AddDirectoryBrowser();
builder.Services.AddSpaStaticFiles(config => config.RootPath = "Client");

builder.Services.AddResponseCompression();
builder.Services.AddResponseCaching();

var firebase = builder.Configuration.GetSection("FirebaseAuthentication");
builder.Services.AddFirebaseAuthentication(firebase.GetValue<string>("Issuer"), firebase.GetValue<string>("Audience"));
builder.Services.AddAuthorization(op
    => op.DefaultPolicy = new AuthorizationPolicyBuilder(JwtBearerDefaults.AuthenticationScheme)
        .RequireAuthenticatedUser()
        .RequireClaim("invitationCodeVerified", "true")
        .Build());

if (Environment.GetEnvironmentVariable("ASPNETCORE_FORWARDEDHEADERS_ENABLED") == "true")
{
    builder.Services.Configure<ForwardedHeadersOptions>(options =>
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

builder.Services.AddCors(options
    => options.AddDefaultPolicy(builder
        => builder.WithOrigins("http://reference.dashif.org")));

builder.Services.AddDistributedMemoryCache();

var app = builder.Build();
var env = app.Environment;
var config = app.Configuration;
app.UseForwardedHeaders();
if (env.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}

// app.UseHttpsRedirection();

app.UseResponseCompression();
app.UseResponseCaching();
app.UseSpaStaticFiles();

app.Map("/stream", app1 =>
{
    var content = config.GetSection("Content").Get<ContentDirSetting>();
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

app.Map("/api", api =>
{
    api.UseRouting();
    api.UseAuthorization();
    api.UseEndpoints(ep => ep.MapControllers());
});

app.UseRouting();
app.UseAuthorization();

app.UseEndpoints(ep =>
{
    ep.MapHangfireDashboard(new DashboardOptions()
    {
        Authorization = new[] { new HangfireDashbordAuthFilter() },
    });
});

app.UseSpa(_ => { });

await app.RunAsync();
