using ApiGateway.Service;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.Identity.Web;
using Yarp.ReverseProxy.Transforms;

var builder = WebApplication.CreateBuilder(args);

// Configure Kestrel to allow larger request headers (Default is 32,768 bytes)
builder.WebHost.ConfigureKestrel(options =>
{
    options.Limits.MaxRequestHeadersTotalSize = 64 * 1024; // Increase to 64 KB
    options.Limits.Http2.MaxRequestHeaderFieldSize  = 16 * 1024;  // Increase individual field to 16 KB
});

builder.Services.AddCors(options =>
{
    options.AddPolicy("LocalDevCorsPolicy", policy =>
    {
        policy.WithOrigins("http://localhost:3000") // Trust your React frontend origin
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials(); // Crucial to allow passing the secure authentication cookie
    });
});


builder.Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;
})
.AddMicrosoftIdentityWebApp(
    microsoftIdentityOptions => {
        builder.Configuration.GetSection("AzureAd").Bind(microsoftIdentityOptions);

        // === THE CRITICAL FIX ===
        // This forces the middleware to cache and save the incoming access token 
        // into the authentication cookie, preventing YARP from crashing.
        microsoftIdentityOptions.SaveTokens = true;
    },
    cookieOptions => {
        cookieOptions.Cookie.Name = "BFF-Auth-Session";
        cookieOptions.Cookie.SameSite = SameSiteMode.Lax;
        cookieOptions.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
        cookieOptions.Cookie.HttpOnly = true;

        cookieOptions.Events.OnRedirectToLogin = context =>
        {
            // Catch ANY path starting with /api and convert the redirect to a 401
            if (context.Request.Path.Value != null &&
                context.Request.Path.Value.StartsWith("/api", StringComparison.OrdinalIgnoreCase))
            {
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            }
            else
            {
                context.Response.Redirect(context.RedirectUri);
            }
            return Task.CompletedTask;
        };
    }

)
.EnableTokenAcquisitionToCallDownstreamApi(options =>
{
    builder.Configuration.GetSection("AzureAd").Bind(options);
}).AddInMemoryTokenCaches();

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AuthenticatedUserPolicy", policy => policy.RequireAuthenticatedUser());
});


// OBO token provider
builder.Services.AddScoped<OBOTokenService>();

var app = builder.Build();


app.UseCors("LocalDevCorsPolicy");
app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

// Login / Logout Endpoints for React App
/*app.MapGet("/bff/login", () =>
    Results.Challenge(
        properties: new AuthenticationProperties { RedirectUri = "http://localhost:3000/" },
        authenticationSchemes: new[] { Microsoft.AspNetCore.Authentication.OpenIdConnect.OpenIdConnectDefaults.AuthenticationScheme }
    ));*/
app.MapGet("/bff/login", async (HttpContext context) =>
{
    var returnUrl = context.Request.Query["redirect_uri"].ToString();

    if (string.IsNullOrWhiteSpace(returnUrl))
        returnUrl = "/admin/products";

    await context.ChallengeAsync(
        OpenIdConnectDefaults.AuthenticationScheme,
        new AuthenticationProperties
        {
            RedirectUri = returnUrl // IMPORTANT: relative only
        });
});

app.MapGet("/bff/logout", () =>
    Results.SignOut(new AuthenticationProperties { RedirectUri = "/" },
        new[] { CookieAuthenticationDefaults.AuthenticationScheme, "OpenIdConnect" }));


// 3. BFF Authentication Endpoint Hub for React/Next.js Client
/*app.MapGet("/bff/login", () => Results.Challenge(new() { RedirectUri = "/" }));

app.MapGet("/auth/logout", () => Results.SignOut(
    new() { RedirectUri = "/" },
    new[] { CookieAuthenticationDefaults.AuthenticationScheme, OpenIdConnectDefaults.AuthenticationScheme }
));

app.MapGet("/auth/user", (System.Security.Claims.ClaimsPrincipal user) =>
{
    if (!user.Identity?.IsAuthenticated ?? true) return Results.Unauthorized();

    var claims = user.Claims.Select(c => new { type = c.Type, value = c.Value });
    return Results.Ok(new { name = user.Identity.Name, claims = claims });
});*/

// Map YARP Gateway and authorization
app.MapReverseProxy().RequireAuthorization();

app.Run();
