using System.IdentityModel.Tokens.Jwt;
using Core.Security.Encryption;
using Core.Security.JWT;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using WebApi.Configuration;
using WebApi.Modules.Notification.Realtime;

namespace WebApi.Authentication;

public static class KeycloakAuthenticationExtensions
{
    public static IServiceCollection AddBankAppJwtAuthentication(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<TokenOptions>(configuration.GetSection("TokenOptions"));
        services.Configure<KeycloakOptions>(configuration.GetSection(KeycloakOptions.SectionName));

        TokenOptions tokenOptions = configuration.GetSection("TokenOptions").Get<TokenOptions>()
            ?? throw new InvalidOperationException("TokenOptions configuration is missing.");
        KeycloakOptions keycloak = configuration.GetSection(KeycloakOptions.SectionName).Get<KeycloakOptions>()
            ?? new KeycloakOptions();

        AuthenticationBuilder auth = services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = "BankApp";
            options.DefaultChallengeScheme = "BankApp";
        });

        auth.AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, options =>
        {
            options.MapInboundClaims = false;
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = tokenOptions.Issuer,
                ValidAudience = tokenOptions.Audience,
                IssuerSigningKey = SecurityKeyHelper.CreateSecurityKey(tokenOptions.SecurityKey),
                ClockSkew = TimeSpan.Zero,
                NameClaimType = System.Security.Claims.ClaimTypes.NameIdentifier,
                RoleClaimType = System.Security.Claims.ClaimTypes.Role
            };
            options.Events = new JwtBearerEvents
            {
                OnMessageReceived = AttachSignalRAccessToken
            };
        });

        if (keycloak.Enabled)
        {
            auth.AddJwtBearer(KeycloakOptions.AuthenticationScheme, options =>
            {
                options.MapInboundClaims = false;
                options.RequireHttpsMetadata = false;
                options.Authority = keycloak.Authority;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuers = keycloak.ValidIssuers is { Length: > 0 }
                        ? keycloak.ValidIssuers
                        : [keycloak.Authority.TrimEnd('/')],
                    ValidateAudience = false,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ClockSkew = TimeSpan.FromMinutes(1),
                    NameClaimType = "sub",
                    RoleClaimType = KeycloakTokenEnricher.RoleClaim
                };
                options.Events = new JwtBearerEvents
                {
                    OnMessageReceived = AttachSignalRAccessToken,
                    OnTokenValidated = context =>
                        KeycloakTokenEnricher.EnrichAsync(context, context.HttpContext.RequestAborted)
                };
            });
        }

        auth.AddPolicyScheme("BankApp", "BankApp local or Keycloak JWT", options =>
        {
            options.ForwardDefaultSelector = context =>
            {
                if (!keycloak.Enabled)
                    return JwtBearerDefaults.AuthenticationScheme;

                string? header = context.Request.Headers.Authorization.FirstOrDefault();
                if (string.IsNullOrWhiteSpace(header)
                    || !header.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
                    return JwtBearerDefaults.AuthenticationScheme;

                string token = header["Bearer ".Length..].Trim();
                JwtSecurityTokenHandler handler = new();
                if (!handler.CanReadToken(token))
                    return JwtBearerDefaults.AuthenticationScheme;

                JwtSecurityToken jwt = handler.ReadJwtToken(token);
                if (!string.IsNullOrEmpty(jwt.Issuer)
                    && jwt.Issuer.Contains(keycloak.RealmPathMarker, StringComparison.OrdinalIgnoreCase))
                    return KeycloakOptions.AuthenticationScheme;

                return JwtBearerDefaults.AuthenticationScheme;
            };
        });

        services.AddAuthorization();
        return services;
    }

    private static Task AttachSignalRAccessToken(MessageReceivedContext context)
    {
        string? accessToken = context.Request.Query["access_token"];
        PathString path = context.HttpContext.Request.Path;
        if (!string.IsNullOrEmpty(accessToken)
            && path.StartsWithSegments(CreditApplicationsHub.Path))
        {
            context.Token = accessToken;
        }

        return Task.CompletedTask;
    }
}
