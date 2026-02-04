using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Scalar.AspNetCore;
using Shared;
using Shared.Services.Authentication;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddOpenApi();
builder.Services.AddRegistrations();

#region Profesyonel Yol – .AddAuthentication().AddJwtBearer()
// Authentication mekanizmasının JWT Bearer olacağını belirtiyoruz.
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                // JWT Bearer token'larını işleyecek ve doğrulayacak handler'ı yapılandırıyoruz.
                .AddJwtBearer(options =>
                {
                    // Token'ı doğrulayacak otorite (Keycloak server) adresini belirtiyoruz. Public key, ilgili kütüphane tarafından bu otoritenin certs adresinden otomatik alınacaktır.
                    options.Authority = "http://127.0.0.1:8080/realms/master";

                    options.Audience = "account";

                    options.RequireHttpsMetadata = false;

                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,
                        NameClaimType = JwtRegisteredClaimNames.PreferredUsername
                    };
                });
#endregion

builder.Services.AddHttpClient("keycloak", configure =>
{
    configure.BaseAddress = new Uri("http://127.0.0.1:8080");
});

var app = builder.Build();
app.UseAuthentication();

#region Yarı Manuel Olarak Token’ı Validate Edelim…
//app.UseMiddleware<JwtAuthenticationMiddleware>();
#endregion

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference(options =>
    {
        options.Layout = ScalarLayout.Classic;
        options.WithTheme(ScalarTheme.BluePlanet);
    });
}

app.MapGet("/", (HttpContext httpContext) => httpContext.User.Identity?.IsAuthenticated);


app.MapGet("/secure/{realm}", async (ManualJwtValidator manualJwtValidator, [FromHeader(Name = "Authorization")] string authorization, string realm = "master") =>
{
    if (!authorization.StartsWith("Bearer "))
        return Results.Unauthorized();

    var token = authorization.Substring("Bearer ".Length);
    var principal = await manualJwtValidator.ValidateAsync(token, realm);

    return TypedResults.Ok(new
    {
        GivenName = principal.Claims.FirstOrDefault(claim => claim.Type == ClaimTypes.GivenName)?.Value,
        UserName = principal.Identity?.Name,
        Claims = principal.Claims.Select(c => new { c.Type, c.Value })
    });
});

app.Run();