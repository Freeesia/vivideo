using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Protocols;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;

namespace StudioFreesia.Vivideo.Server.Extensions;

public static class FirebaseAuthExtensions
{
    public static IServiceCollection AddFirebaseAuthentication(this IServiceCollection services, FirebaseAuthOptions options)
    {
        var (issuer, audience, useEmulator) = options;
        var oidConfig = $"{issuer}/.well-known/openid-configuration";
        var configurationManager =
        services.AddAuthentication(o =>
        {
            o.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            o.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            o.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(o =>
        {
            o.IncludeErrorDetails = true;
            o.RefreshOnIssuerKeyNotFound = true;
            o.MetadataAddress = oidConfig;
            o.ConfigurationManager = new ConfigurationManager<OpenIdConnectConfiguration>(oidConfig, new OpenIdConnectConfigurationRetriever()); ;
            o.Audience = audience;
            o.TokenValidationParameters.RequireSignedTokens = !useEmulator;
        });
        return services;
    }
}

public record FirebaseAuthOptions(string Issuer, string Audience, bool UseEmulator = false);
