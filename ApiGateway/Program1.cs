using ApiGateway.Service;
using AspNetCoreRateLimit;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Identity.Web;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Yarp.ReverseProxy;
using Yarp.ReverseProxy.Transforms;

var builder = WebApplication.CreateBuilder(args);

var config = builder.Configuration;

// Auth (incoming token) for downstreaming to respective api's
builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddMicrosoftIdentityWebApi(builder.Configuration.GetSection("AzureAd"))
    .EnableTokenAcquisitionToCallDownstreamApi()
    .AddInMemoryTokenCaches();

// JWT Auth setup
/*builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddMicrosoftIdentityWebApi(builder.Configuration.GetSection("AzureAd"));*/
/*builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = false,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = config["Jwt:Issuer"],
        IssuerSigningKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(config["Jwt:Key"])
        )
    };
});*/

builder.Services.AddAuthorization();

//  Rate Limiting
builder.Services.AddMemoryCache();

builder.Services.Configure<IpRateLimitOptions>(
    builder.Configuration.GetSection("IpRateLimiting"));
/*
// moved to appsettings
builder.Services.Configure<IpRateLimitOptions>(options =>
{
    options.GeneralRules = new List<RateLimitRule>
    {
        new RateLimitRule
        {
            Endpoint = "*",
            Limit = 100,   // 100 requests
            Period = "1m"  // per minute
        }
    };
});*/

builder.Services.AddSingleton<IRateLimitConfiguration, RateLimitConfiguration>();
builder.Services.AddInMemoryRateLimiting();
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy
            .AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader();
    });
});

builder.Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"))
    .AddTransforms(transformBuilder =>
    {
        transformBuilder.AddRequestTransform(async context =>
        {
            var feature = context.HttpContext.GetReverseProxyFeature();
            var route = feature.Route.Config;

            if (!route.Metadata.TryGetValue("Scope", out var scopeObj))
                return;

            var scope = scopeObj?.ToString();
            if (string.IsNullOrEmpty(scope))
                return;

            var tokenService = context.HttpContext.RequestServices
                .GetRequiredService<OBOTokenService>();

            var token = await tokenService.GetTokenAsync(scope);

            context.ProxyRequest.Headers.Remove("Authorization");
            context.ProxyRequest.Headers.Add("Authorization", $"Bearer {token}");
        });
    });
// Yarp reverse proxy
// Yarp automatically passes the authorization token to downstream service
/*builder.Services
    .AddReverseProxy()
    .LoadFromConfig(config.GetSection("ReverseProxy"));*/

//manually/force passing the token to service
/*
 builder.Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"))
    .AddTransforms(transformBuilder =>
    {
        transformBuilder.AddRequestTransform(context =>
        {
            var auth = context.HttpContext.Request.Headers.Authorization.ToString();

            if (!string.IsNullOrEmpty(auth))
            {
                context.ProxyRequest.Headers.Remove("Authorization");
                context.ProxyRequest.Headers.Add("Authorization", auth);
            }

            return ValueTask.CompletedTask;
        });
    });
 */

// OBO token provider
builder.Services.AddScoped<OBOTokenService>();


var app = builder.Build();

// Order

app.UseCors("AllowAll");
app.UseAuthentication();   // 1. Authenticate
app.UseAuthorization();    // 2. Authorize
app.UseIpRateLimiting();   // 3. Rate limiting


app.Use(async (context, next) =>
{
    foreach (var header in context.Request.Headers)
    {
        Console.WriteLine($"{header.Key}: {header.Value}");
    }

    var authHeader = context.Request.Headers["Authorization"].ToString();

    Console.WriteLine("Authorization Header:");
    Console.WriteLine(authHeader);

    if (string.IsNullOrEmpty(authHeader))
    {
        Console.WriteLine("Token NOT received");
    }
    else
    {
        Console.WriteLine("Token received successfully");
    }

    await next();
});

// Request validation middleware
app.Use(async (context, next) =>
{
    if (!context.Request.Headers.ContainsKey("User-Agent"))
    {
        context.Response.StatusCode = 400;
        await context.Response.WriteAsync("Missing User-Agent header");
        return;
    }

    // example for blocking suspicious requests
    foreach (var queryParam in context.Request.Query)
    {
        var value = queryParam.Value.ToString();

        if (value.Contains("DROP TABLE",
            StringComparison.OrdinalIgnoreCase))
        {
            context.Response.StatusCode = 400;
            await context.Response.WriteAsync("Invalid request");
            return;
        }
    }
    await next();
});

// Map YARP applying authorization
app.MapReverseProxy().RequireAuthorization();

app.Run();