using Scalar.AspNetCore;
using Shared;
using Shared.Modals.Request;
using Shared.Services;
using System.Net.Mime;
using System.Text;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();
builder.Services.AddRegistrations();

builder.Services.AddHttpClient("keycloak", configure =>
{
    configure.BaseAddress = new Uri("http://127.0.0.1:8080");
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference(options =>
    {
        options.Layout = ScalarLayout.Classic;
        options.WithTheme(ScalarTheme.BluePlanet);
    });
}

app.MapGet("/get-token", async (TokenRequestService tokenRequestService) =>
{
    var token = await tokenRequestService.GetAccessTokenAsync();
    return TypedResults.Ok(token);
});

#region Realm Yönetimi
app.MapPost("/create-realm", async (TokenRequestService tokenRequestService, IHttpClientFactory httpClientFactory, RealmRequest realmRequest) =>
{
    var token = await tokenRequestService.GetAccessTokenAsync();
    var httpClient = httpClientFactory.CreateClient("keycloak");

    httpClient.DefaultRequestHeaders.Add(Microsoft.Net.Http.Headers.HeaderNames.Authorization, $"Bearer {token.AccessToken}");
    var content = new StringContent(
    JsonSerializer.Serialize(realmRequest),
    Encoding.UTF8,
    MediaTypeNames.Application.Json);
    var response = await httpClient.PostAsync("/admin/realms", content);

    return TypedResults.StatusCode((int)response.StatusCode);
});

app.MapGet("/get-realms", async (TokenRequestService tokenRequestService, IHttpClientFactory httpClientFactory) =>
{
    var token = await tokenRequestService.GetAccessTokenAsync();
    var httpClient = httpClientFactory.CreateClient("keycloak");

    httpClient.DefaultRequestHeaders.Add(Microsoft.Net.Http.Headers.HeaderNames.Authorization, $"Bearer {token.AccessToken}");
    var response = await httpClient.GetAsync("/admin/realms");
    var data = await response.Content.ReadFromJsonAsync<dynamic>();

    return TypedResults.Ok(data);
});

app.MapDelete("/delete-realm/{realm}", async (TokenRequestService tokenRequestService, IHttpClientFactory httpClientFactory, string realm) =>
{
    var token = await tokenRequestService.GetAccessTokenAsync();
    var httpClient = httpClientFactory.CreateClient("keycloak");

    httpClient.DefaultRequestHeaders.Add(Microsoft.Net.Http.Headers.HeaderNames.Authorization, $"Bearer {token.AccessToken}");
    var response = await httpClient.DeleteAsync($"/admin/realms/{realm}");

    return TypedResults.StatusCode((int)response.StatusCode);
});

app.MapPut("/update-realm-settings/{realm}", async (TokenRequestService tokenRequestService, IHttpClientFactory httpClientFactory, string realm) =>
{
    var token = await tokenRequestService.GetAccessTokenAsync();
    var httpClient = httpClientFactory.CreateClient("keycloak");

    httpClient.DefaultRequestHeaders.Add(Microsoft.Net.Http.Headers.HeaderNames.Authorization, $"Bearer {token.AccessToken}");

    //Realm elde ediliyor
    var getRealmResponse = await httpClient.GetAsync($"/admin/realms/{realm}");
    var realmToUpdate = await getRealmResponse.Content.ReadFromJsonAsync<dynamic>();

    var content = new StringContent(
        JsonSerializer.Serialize(new
        {
            realm = "updated-example-realm",
            enabled = false,
            displayName = "Updated Example Realm",
            loginWithEmailAllowed = false,                      //Email ile login
            resetPasswordAllowed = true,                        //Þifre sýfýrlama
            verifyEmail = true,                                 //Email doðrulama
            accessTokenLifespan = 99999,                        //Access token ömrü
            bruteForceProtected = true,                         //Brute force korumasý
            supportedLocales = new string[] { "tr", "en" }      //Desteklenen diller
        }),
        Encoding.UTF8,
        MediaTypeNames.Application.Json);

    var response = await httpClient.PutAsync($"/admin/realms/{realm}", content);

    return TypedResults.StatusCode((int)response.StatusCode);
});
#endregion
#region Client Yönetimi
app.MapPost("/create-client/{realm}", async (TokenRequestService tokenRequestService, IHttpClientFactory httpClientFactory, ClientRequest clientRequest, string realm) =>
{
    var token = await tokenRequestService.GetAccessTokenAsync();
    var httpClient = httpClientFactory.CreateClient("keycloak");

    httpClient.DefaultRequestHeaders.Add(Microsoft.Net.Http.Headers.HeaderNames.Authorization, $"Bearer {token.AccessToken}");
    var content = new StringContent(
    JsonSerializer.Serialize(clientRequest),
    Encoding.UTF8,
    MediaTypeNames.Application.Json);
    var response = await httpClient.PostAsync($"/admin/realms/{realm}/clients", content);

    return TypedResults.StatusCode((int)response.StatusCode);
});

app.MapGet("/get-clients/{realm}", async (TokenRequestService tokenRequestService, IHttpClientFactory httpClientFactory, string realm) =>
{
    var token = await tokenRequestService.GetAccessTokenAsync();
    var httpClient = httpClientFactory.CreateClient("keycloak");

    httpClient.DefaultRequestHeaders.Add(Microsoft.Net.Http.Headers.HeaderNames.Authorization, $"Bearer {token.AccessToken}");
    var response = await httpClient.GetAsync($"/admin/realms/{realm}/clients");
    var data = await response.Content.ReadFromJsonAsync<dynamic>();

    return TypedResults.Ok(data);
});

app.MapDelete("/delete-client/{realm}/{clientId}", async (TokenRequestService tokenRequestService, IHttpClientFactory httpClientFactory, string realm, Guid clientId) =>
{
    var token = await tokenRequestService.GetAccessTokenAsync();
    var httpClient = httpClientFactory.CreateClient("keycloak");

    httpClient.DefaultRequestHeaders.Add(Microsoft.Net.Http.Headers.HeaderNames.Authorization, $"Bearer {token.AccessToken}");
    var response = await httpClient.DeleteAsync($"/admin/realms/{realm}/clients/{clientId}");

    return TypedResults.StatusCode((int)response.StatusCode);
});

app.MapPut("/update-client-settings/{realm}/{clientId}", async (TokenRequestService tokenRequestService, IHttpClientFactory httpClientFactory, string realm, Guid clientId) =>
{
    var token = await tokenRequestService.GetAccessTokenAsync();
    var httpClient = httpClientFactory.CreateClient("keycloak");

    httpClient.DefaultRequestHeaders.Add(Microsoft.Net.Http.Headers.HeaderNames.Authorization, $"Bearer {token.AccessToken}");

    //Client ilgili realm üzerinden elde ediliyor
    var getClientResponse = await httpClient.GetAsync($"/admin/realms/{realm}/clients/{clientId}");
    var clientToUpdate = await getClientResponse.Content.ReadFromJsonAsync<dynamic>();

    var content = new StringContent(
        JsonSerializer.Serialize(new
        {
            id = clientId,
            clientId = "Updated Client",
            enabled = false,
            serviceAccountsEnabled = false,
            directAccessGrantsEnabled = true
        }),
        Encoding.UTF8,
        MediaTypeNames.Application.Json);

    var response = await httpClient.PutAsync($"/admin/realms/{realm}/clients/{clientId}", content);

    return TypedResults.StatusCode((int)response.StatusCode);
});
#endregion
#region Kullanýcý Yönetimi
app.MapPost("/create-user/{realm}", async (TokenRequestService tokenRequestService, IHttpClientFactory httpClientFactory, string realm, UserRequest userRequest) =>
{
    var token = await tokenRequestService.GetAccessTokenAsync();
    var httpClient = httpClientFactory.CreateClient("keycloak");

    httpClient.DefaultRequestHeaders.Add(Microsoft.Net.Http.Headers.HeaderNames.Authorization, $"Bearer {token.AccessToken}");
    var content = new StringContent(
    JsonSerializer.Serialize(userRequest),
    Encoding.UTF8,
    MediaTypeNames.Application.Json);
    var response = await httpClient.PostAsync($"/admin/realms/{realm}/users", content);

    return TypedResults.StatusCode((int)response.StatusCode);
});

app.MapGet("/get-users/{realm}", async (TokenRequestService tokenRequestService, IHttpClientFactory httpClientFactory, string realm) =>
{
    var token = await tokenRequestService.GetAccessTokenAsync();
    var httpClient = httpClientFactory.CreateClient("keycloak");

    httpClient.DefaultRequestHeaders.Add(Microsoft.Net.Http.Headers.HeaderNames.Authorization, $"Bearer {token.AccessToken}");
    var response = await httpClient.GetAsync($"/admin/realms/{realm}/users");
    var data = await response.Content.ReadFromJsonAsync<dynamic>();

    return TypedResults.Ok(data);
});

app.MapDelete("/delete-user/{realm}/{userId}", async (TokenRequestService tokenRequestService, IHttpClientFactory httpClientFactory, string realm, Guid userId) =>
{
    var token = await tokenRequestService.GetAccessTokenAsync();
    var httpClient = httpClientFactory.CreateClient("keycloak");

    httpClient.DefaultRequestHeaders.Add(Microsoft.Net.Http.Headers.HeaderNames.Authorization, $"Bearer {token.AccessToken}");
    var response = await httpClient.DeleteAsync($"/admin/realms/{realm}/users/{userId}");

    return TypedResults.StatusCode((int)response.StatusCode);
});

app.MapPut("/user/active-passive/{realm}/{userId}/{status}", async (TokenRequestService tokenRequestService, IHttpClientFactory httpClientFactory, string realm, Guid userId, bool status) =>
{
    var token = await tokenRequestService.GetAccessTokenAsync();
    var httpClient = httpClientFactory.CreateClient("keycloak");

    httpClient.DefaultRequestHeaders.Add(Microsoft.Net.Http.Headers.HeaderNames.Authorization, $"Bearer {token.AccessToken}");

    //User çekiliyor.
    var userResponse = await httpClient.GetAsync($"/admin/realms/{realm}/users/{userId}");
    var user = await userResponse.Content.ReadFromJsonAsync<dynamic>();

    var content = new StringContent(
    JsonSerializer.Serialize(new
    {
        id = userId,
        username = user.GetProperty("username").GetString(),
        enabled = status
    }),
    Encoding.UTF8,
    MediaTypeNames.Application.Json);
    var response = await httpClient.PutAsync($"/admin/realms/{realm}/users/{userId}", content);

    return TypedResults.StatusCode((int)response.StatusCode);
});

app.MapPut("/user/update-to-email/{realm}/{userId}", async (TokenRequestService tokenRequestService, IHttpClientFactory httpClientFactory, string realm, Guid userId, EmailRequest emailRequest) =>
{
    var token = await tokenRequestService.GetAccessTokenAsync();
    var httpClient = httpClientFactory.CreateClient("keycloak");

    httpClient.DefaultRequestHeaders.Add(Microsoft.Net.Http.Headers.HeaderNames.Authorization, $"Bearer {token.AccessToken}");

    var content = new StringContent(
    JsonSerializer.Serialize(new
    {
        id = userId,
        email = emailRequest.Email,
        emailVerified = emailRequest.EmailVerified
    }),
    Encoding.UTF8,
    MediaTypeNames.Application.Json);
    var response = await httpClient.PutAsync($"/admin/realms/{realm}/users/{userId}", content);

    return TypedResults.StatusCode((int)response.StatusCode);
});

app.MapPut("/user/initial-password/{realm}/{userId}/{temporaryPassword}", async (TokenRequestService tokenRequestService, IHttpClientFactory httpClientFactory, string realm, Guid userId, string temporaryPassword) =>
{
    var token = await tokenRequestService.GetAccessTokenAsync();
    var httpClient = httpClientFactory.CreateClient("keycloak");

    httpClient.DefaultRequestHeaders.Add(Microsoft.Net.Http.Headers.HeaderNames.Authorization, $"Bearer {token.AccessToken}");

    var content = new StringContent(
    JsonSerializer.Serialize(new
    {
        type = "password",
        value = temporaryPassword,
        temporary = true
    }),
    Encoding.UTF8,
    MediaTypeNames.Application.Json);
    var response = await httpClient.PutAsync($"/admin/realms/{realm}/users/{userId}/reset-password", content);

    return TypedResults.StatusCode((int)response.StatusCode);
});
#endregion
#region Rol Yönetimi
app.MapPost("/create-role/{realm}", async (TokenRequestService tokenRequestService, IHttpClientFactory httpClientFactory, string realm, RoleRequest roleRequest) =>
{
    var token = await tokenRequestService.GetAccessTokenAsync();
    var httpClient = httpClientFactory.CreateClient("keycloak");

    httpClient.DefaultRequestHeaders.Add(Microsoft.Net.Http.Headers.HeaderNames.Authorization, $"Bearer {token.AccessToken}");
    var content = new StringContent(
    JsonSerializer.Serialize(roleRequest),
    Encoding.UTF8,
    MediaTypeNames.Application.Json);
    var response = await httpClient.PostAsync($"/admin/realms/{realm}/roles", content);

    return TypedResults.StatusCode((int)response.StatusCode);
});

app.MapPost("/create-role/{realm}/{clientId}", async (TokenRequestService tokenRequestService, IHttpClientFactory httpClientFactory, string realm, string clientId, RoleRequest roleRequest) =>
{
    var token = await tokenRequestService.GetAccessTokenAsync();
    var httpClient = httpClientFactory.CreateClient("keycloak");

    httpClient.DefaultRequestHeaders.Add(Microsoft.Net.Http.Headers.HeaderNames.Authorization, $"Bearer {token.AccessToken}");
    var content = new StringContent(
    JsonSerializer.Serialize(roleRequest),
    Encoding.UTF8,
    MediaTypeNames.Application.Json);
    var response = await httpClient.PostAsync($"/admin/realms/{realm}/clients/{clientId}/roles", content);

    return TypedResults.StatusCode((int)response.StatusCode);
});

app.MapGet("/get-roles/{realm}", async (TokenRequestService tokenRequestService, IHttpClientFactory httpClientFactory, string realm) =>
{
    var token = await tokenRequestService.GetAccessTokenAsync();
    var httpClient = httpClientFactory.CreateClient("keycloak");

    httpClient.DefaultRequestHeaders.Add(Microsoft.Net.Http.Headers.HeaderNames.Authorization, $"Bearer {token.AccessToken}");
    var response = await httpClient.GetAsync($"/admin/realms/{realm}/roles");
    var data = await response.Content.ReadFromJsonAsync<dynamic>();

    return TypedResults.Ok(data);
});

app.MapDelete("/delete-role/{realm}/{role}", async (TokenRequestService tokenRequestService, IHttpClientFactory httpClientFactory, string realm, string role) =>
{
    var token = await tokenRequestService.GetAccessTokenAsync();
    var httpClient = httpClientFactory.CreateClient("keycloak");

    httpClient.DefaultRequestHeaders.Add(Microsoft.Net.Http.Headers.HeaderNames.Authorization, $"Bearer {token.AccessToken}");
    var response = await httpClient.DeleteAsync($"/admin/realms/{realm}/roles/{role}");

    return TypedResults.StatusCode((int)response.StatusCode);
});

app.MapPut("/update-to-role/{realm}/{role}", async (TokenRequestService tokenRequestService, IHttpClientFactory httpClientFactory, string realm, string role, RoleRequest roleRequest) =>
{
    var token = await tokenRequestService.GetAccessTokenAsync();
    var httpClient = httpClientFactory.CreateClient("keycloak");

    httpClient.DefaultRequestHeaders.Add(Microsoft.Net.Http.Headers.HeaderNames.Authorization, $"Bearer {token.AccessToken}");
    var content = new StringContent(
    JsonSerializer.Serialize(roleRequest),
    Encoding.UTF8,
    MediaTypeNames.Application.Json);
    var response = await httpClient.PutAsync($"/admin/realms/{realm}/roles/{role}", content);

    return TypedResults.StatusCode((int)response.StatusCode);
});

app.MapPost("/role-mapping/{realm}/{userId}", async (TokenRequestService tokenRequestService, IHttpClientFactory httpClientFactory, string realm, Guid userId, RoleMappingRequest roleMappingRequest) =>
{
    var token = await tokenRequestService.GetAccessTokenAsync();
    var httpClient = httpClientFactory.CreateClient("keycloak");

    httpClient.DefaultRequestHeaders.Add(Microsoft.Net.Http.Headers.HeaderNames.Authorization, $"Bearer {token.AccessToken}");
    var content = new StringContent(
    JsonSerializer.Serialize(new List<RoleMappingRequest> { roleMappingRequest }),
    Encoding.UTF8,
    MediaTypeNames.Application.Json);
    var response = await httpClient.PostAsync($"/admin/realms/{realm}/users/{userId}/role-mappings/realm", content);

    return TypedResults.StatusCode((int)response.StatusCode);
});
#endregion
#region Scope Yönetimi
app.MapPost("/create-scope/{realm}", async (TokenRequestService tokenRequestService, IHttpClientFactory httpClientFactory, string realm, ScopeRequest scopeRequest) =>
{
    var token = await tokenRequestService.GetAccessTokenAsync();
    var httpClient = httpClientFactory.CreateClient("keycloak");

    httpClient.DefaultRequestHeaders.Add(Microsoft.Net.Http.Headers.HeaderNames.Authorization, $"Bearer {token.AccessToken}");
    var content = new StringContent(
    JsonSerializer.Serialize(scopeRequest),
    Encoding.UTF8,
    MediaTypeNames.Application.Json);
    var response = await httpClient.PostAsync($"/admin/realms/{realm}/client-scopes", content);

    return TypedResults.StatusCode((int)response.StatusCode);
});

app.MapPut("/default-client-scope/{realm}/{clientId}/{scopeId}", async (TokenRequestService tokenRequestService, IHttpClientFactory httpClientFactory, string realm, Guid clientId, Guid scopeId) =>
{
    var token = await tokenRequestService.GetAccessTokenAsync();
    var httpClient = httpClientFactory.CreateClient("keycloak");

    httpClient.DefaultRequestHeaders.Add(Microsoft.Net.Http.Headers.HeaderNames.Authorization, $"Bearer {token.AccessToken}");
    var response = await httpClient.PutAsync($"/admin/realms/{realm}/clients/{clientId}/default-client-scopes/{scopeId}", null);

    return TypedResults.StatusCode((int)response.StatusCode);
});

app.MapPut("/optional-client-scope/{realm}/{clientId}/{scopeId}", async (TokenRequestService tokenRequestService, IHttpClientFactory httpClientFactory, string realm, Guid clientId, Guid scopeId) =>
{
    var token = await tokenRequestService.GetAccessTokenAsync();
    var httpClient = httpClientFactory.CreateClient("keycloak");

    httpClient.DefaultRequestHeaders.Add(Microsoft.Net.Http.Headers.HeaderNames.Authorization, $"Bearer {token.AccessToken}");
    var response = await httpClient.PutAsync($"/admin/realms/{realm}/clients/{clientId}/optional-client-scopes/{scopeId}", null);

    return TypedResults.StatusCode((int)response.StatusCode);
});
#endregion
#region Login Ýþlemleri - Token Alma
app.MapPost("/login/{realm}", async (TokenRequestService tokenRequestService, IHttpClientFactory httpClientFactory, string realm) =>
{
    var httpClient = httpClientFactory.CreateClient("keycloak");

    var content = new FormUrlEncodedContent(new Dictionary<string, string>
    {
        ["grant_type"] = "password",
        ["client_id"] = "admin-cli",
        ["username"] = "abcd",
        ["password"] = "123",
        ["scope"] = "openid",
    });
    var response = await httpClient.PostAsync($"/realms/{realm}/protocol/openid-connect/token", content);
    var token = await response.Content.ReadFromJsonAsync<dynamic>();

    return TypedResults.Ok(token);
});
#endregion

app.Run();
