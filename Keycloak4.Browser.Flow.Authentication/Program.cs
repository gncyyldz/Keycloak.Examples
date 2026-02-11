using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();

builder.Services.AddAuthentication(options =>
    {
        //Varsayılan kimlik doğrulama şeması olarak Cookie tabanlı kimlik doğrulama kullanılmaktadır.
        //Yani kullanıcı oturum bilgileri cookie'de saklanır.
        options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
        //Kullanıcı, yetkisiz olarak kimlik doğrulama gerektiren bir sayfaya erişmeye çalıştığında
        //OpenID Connect protokolü kullanılarak kimlik doğrulama işlemi başlatılır ve Keycloak'a yönlendirme sağlanır.
        options.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;
    })
    .AddCookie(options =>
    {
        //Kullanıcı giriş yapmadığında yönlendirileceği sayfa.
        options.LoginPath = "/login";
        //Kullanıcı çıkış yaptığında yönlendirileceği sayfa.
        options.LogoutPath = "/logout";
    })
    //OpenID Connect protokolü kullanılarak Keycloak ile entegrasyon sağlanılmaktadır.
    .AddOpenIdConnect(options =>
    {
        options.Authority = "http://127.0.0.1:8080/realms/master";
        options.ClientId = "application-client";
        options.ClientSecret = "jtga52R3wM8T4DJvjWB5upQTeCl7NX8P";
        options.ResponseType = "code";

        options.SaveTokens = true;
        options.GetClaimsFromUserInfoEndpoint = true;
        options.RequireHttpsMetadata = false;

        options.Scope.Clear();
        options.Scope.Add("openid");
        options.Scope.Add("profile");
        options.Scope.Add("email");

        options.CallbackPath = "/signin-oidc";

        options.TokenValidationParameters = new()
        {
            NameClaimType = "preferred_username",
            RoleClaimType = "roles"
        };
    });


builder.Services.AddHttpClient("keycloak", configure =>
{
    configure.BaseAddress = new Uri("http://127.0.0.1:8080");
});

var app = builder.Build();

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapDefaultControllerRoute();

//app.MapGet("/", () => "Hello World!");

app.Run();
