using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Scalar.AspNetCore;
using Shared.Modals.Request;
using System.IdentityModel.Tokens.Jwt;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, options =>
                {
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

builder.Services.AddAuthorization();

builder.Services.AddHttpClient("keycloak", configure =>
{
    configure.BaseAddress = new Uri("http://127.0.0.1:8080");
});

var app = builder.Build();

app.UseAuthentication()
   .UseAuthorization();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference(options =>
    {
        options.Layout = ScalarLayout.Classic;
        options.WithTheme(ScalarTheme.BluePlanet);
    });
}

app.MapPost("/login/{realm}", async (LoginRequest loginRequest, IHttpClientFactory httpClientFactory, string realm = "master") =>
{
    var httpClient = httpClientFactory.CreateClient("keycloak");

    var parameters = new Dictionary<string, string>
    {
        ["grant_type"] = "password",
        ["client_id"] = "application-client",
        //["client_secret"] = "TSQ7dhxgymeqxPunV18S3WQY6dmBY9Il",
        ["username"] = loginRequest.Username,
        ["password"] = loginRequest.Password
    };

    var response = await httpClient.PostAsync($"realms/{realm}/protocol/openid-connect/token", new FormUrlEncodedContent(parameters));

    if (!response.IsSuccessStatusCode)
        return Results.Unauthorized();

    var token = await response.Content.ReadFromJsonAsync<Shared.Modals.Responses.AccessTokenResponse>();
    return Results.Ok(token);
});

app.MapGet("/profile", (HttpContext httpContext) =>
{
    return Results.Ok(new
    {
        httpContext.User.Identity?.Name,
        Claims = httpContext.User.Claims.Select(claim => new { claim.Type, claim.Value })
    });
}).RequireAuthorization();

app.Run();
