#define RPT_UMA_Authorization



#if Policy_Based_Authorization
using Keycloak8.Keycloakda_Advanced_Authorization_Services_UMA_Sistemini_İnceleyelim.Transformations;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using System.Security.Claims;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddScoped<IClaimsTransformation, KeycloakRolesTransformer>();

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, options =>
    {
        options.Authority = "http://127.0.0.1:8080/realms/master";
        options.Audience = "account";
        options.RequireHttpsMetadata = false;

        //Bunun yerine KeycloakRolesTransformer sınıfı da kullanılabilir.

        //options.Events = new JwtBearerEvents
        //{
        //    OnTokenValidated = context =>
        //    {
        //        var identity = (ClaimsIdentity)context.Principal!.Identity!;
        //        var realmAccess = context.Principal.FindFirst("realm_access")?.Value;

        //        if (realmAccess != null)
        //        {
        //            var doc = JsonDocument.Parse(realmAccess);
        //            if (doc.RootElement.TryGetProperty("roles", out var roles))
        //                foreach (var role in roles.EnumerateArray())
        //                    identity.AddClaim(new Claim(ClaimTypes.Role, role.GetString()!));
        //        }

        //        return Task.CompletedTask;
        //    }
        //};

        options.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
        {
            RoleClaimType = ClaimTypes.Role,
            NameClaimType = ClaimTypes.Name
        };
    });

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", policy =>
    {
        policy.RequireAuthenticatedUser()
              .RequireClaim(ClaimTypes.Role, "admin");
    });
});


var app = builder.Build();

app.UseAuthentication()
   .UseAuthorization();

app.MapGet("/", () => "Hello World!")
    .RequireAuthorization(policyNames: "AdminOnly");

app.Run();
#elif Resource_Based_Authorization
using Keycloak8.Keycloakda_Advanced_Authorization_Services_UMA_Sistemini_İnceleyelim.Entities;
using Keycloak8.Keycloakda_Advanced_Authorization_Services_UMA_Sistemini_İnceleyelim.Handlers;
using Keycloak8.Keycloakda_Advanced_Authorization_Services_UMA_Sistemini_İnceleyelim.Requirements;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddScoped<IAuthorizationHandler, EditPostHandler>();

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, options =>
    {
        options.Authority = "http://127.0.0.1:8080/realms/master";
        options.Audience = "account";
        options.RequireHttpsMetadata = false;

        options.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
        {
            RoleClaimType = ClaimTypes.Role,
            NameClaimType = ClaimTypes.Name
        };
    });

builder.Services.AddAuthorization();

var app = builder.Build();

app.UseAuthentication()
   .UseAuthorization();

app.MapPut("/post-edit", async (IAuthorizationService authorizationService, HttpContext httpContext) =>
{
    Post post = new()
    {
        OwnerId = new Guid("af3787e6-1c79-4ba4-a935-442851a4c529")
    };

    var result = await authorizationService.AuthorizeAsync(httpContext.User, post, new EditPostRequirement());

    if (!result.Succeeded)
        return Results.Forbid();

    return Results.Ok();
});

app.Run();
#elif RPT_UMA_Authorization
using Keycloak8.Keycloakda_Advanced_Authorization_Services_UMA_Sistemini_İnceleyelim.Middlewares;
using Keycloak8.Keycloakda_Advanced_Authorization_Services_UMA_Sistemini_İnceleyelim.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddHttpClient();
builder.Services.AddSingleton<UmaTokenService>();
builder.Services.AddSingleton<RptValidator>();
builder.Services.AddSingleton<UmaMiddleware>();

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, options =>
    {
        options.Authority = builder.Configuration["Keycloak:Authority"];
        options.Audience = builder.Configuration["Keycloak:ClientId"];
        options.RequireHttpsMetadata = false;
        options.TokenValidationParameters = new()
        {
            ValidateIssuer = true,
            ValidateAudience = false, //UMA'da audience kontrolü middleware'da yapılmaktadır. Burada sadece token'ın Keycloak tarafından imzalanıp imzalanmadığı kontrol edilmektedir.
            ValidateLifetime = true
        };
    });

var app = builder.Build();
app.UseAuthentication();

app.UseMiddleware<UmaMiddleware>();

app.MapGet("/api/documents/{id}", (string id) =>
{
    //Buraya gelindiyse middleware RPT'yi doğrulamış demektir...
    return Results.Ok(new
    {
        id,
        title = "Document",
        content = "Bu içerik UMA 2.0 ile korunmaktadır.",
        accessedAt = DateTime.UtcNow
    });
});

app.Run();
#endif