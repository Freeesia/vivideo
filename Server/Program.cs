
using AspNetCore.Firebase.Authentication.Extensions;
using Hangfire;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.HttpOverrides;
using StudioFreesia.Vivideo.Core;
using StudioFreesia.Vivideo.Server.ComponentModel;
using StudioFreesia.Vivideo.Server.Modules;

var builder = WebApplication.CreateBuilder(args);

#if !DEBUG
builder.WebHost.UseSentry("https://6bd5217ab2e24414973357727d9df261@sentry.io/2409801");
#endif

builder.Services.Configure<ContentDirSetting>(builder.Configuration.GetSection("Content"));

builder.Services.AddControllers();
builder.Services.AddHangfire(config
    => config.UseRedisStorage(builder.Configuration.GetConnectionString("Redis")));
builder.Services.AddSingleton<ITranscodedCache, TranscodedCache>();
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

app.Map("/api", api =>
{
    api.UseRouting();
    api.UseAuthorization();
    api.UseEndpoints(ep => ep.MapControllers());
});

app.UseRouting();
#pragma warning disable ASP0001
app.UseAuthorization();
#pragma warning restore ASP0001

app.UseEndpoints(ep =>
{
    ep.MapHangfireDashboard(new DashboardOptions()
    {
        Authorization = new[] { new HangfireDashbordAuthFilter() },
    });
});

app.UseSpa(_ => { });

await app.RunAsync();
