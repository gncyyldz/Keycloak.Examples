using Scalar.AspNetCore;
using Shared;
using Shared.Modals.Request;
using Shared.Services;
using System.Net.Http.Headers;
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

#region Varsayılan Olarak Sunulan Kullanıcı Bilgilerine Ek Alan (Attribute) Ekleme
app.MapPost("/realm/{realm}/configure-user-attributes", async (string realm, TokenRequestService tokenRequestService, IHttpClientFactory httpClientFactory, RealmAttributeConfigRequest request) =>
{
    var token = await tokenRequestService.GetAccessTokenAsync();
    using var httpClient = httpClientFactory.CreateClient("keycloak");

    httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token.AccessToken);

    // 1. Mevcut User Profile yapılandırmasını al
    var getProfileResponse = await httpClient.GetAsync($"/admin/realms/{realm}/users/profile");

    if (!getProfileResponse.IsSuccessStatusCode)
        return Results.BadRequest(new { error = "User Profile yapılandırması alınamadı!" });

    var profileContent = await getProfileResponse.Content.ReadAsStringAsync();
    var existingProfile = JsonSerializer.Deserialize<JsonElement>(profileContent);

    // 2. Mevcut attributes listesini al
    List<object> attributesList = null;

    if (existingProfile.TryGetProperty("attributes", out var attributesElement))
        attributesList = attributesElement
                    .EnumerateArray()
                    .Select(attr => JsonSerializer.Deserialize<object>(attr.GetRawText()))
                    .OfType<object>()
                    .ToList();

    // 3. Yeni attribute'ları ekle
    request.NewAttributes.ForEach(newAttribute =>
    {
        // Validation objesini oluştur
        var validations = new Dictionary<string, object>();

        if (newAttribute.Validations is not null)
            newAttribute.Validations.ForEach(validation =>
        {
            validations[validation.Type.ToLower()] = validation.Type.ToLower() switch
            {
                "length" => new { min = validation.Min, max = validation.Max },
                "email" => new { },
                "pattern" => new { pattern = validation.Pattern, errorMessage = validation.ErrorMessage ?? "Geçersiz format" },
                "options" => new { options = validation.Options },
                "uri" => new { },
                "integer" => new { min = validation.Min, max = validation.Max },
                "number" => new { min = validation.Min, max = validation.Max }
            };
        });

        var attributeConfig = new Dictionary<string, object>
        {
            ["name"] = newAttribute.Name,
            ["displayName"] = newAttribute.DisplayName ?? newAttribute.Name,
            ["validations"] = validations,
            ["permissions"] = new
            {
                view = new[] { "admin", "user" },
                edit = new[] { "admin", "user" }
            },
            ["multivalued"] = newAttribute.Multivalued
        };

        // Required sadece true olduğunda ekle
        if (newAttribute.Required)
            attributeConfig["required"] = new
            {
                roles = new[] { "user" }
            };

        attributesList!.Add(attributeConfig);
    });

    // 4. Güncellenmiş profile yapılandırmasını hazırla
    var updatedProfile = new
    {
        attributes = attributesList,
        groups = existingProfile.TryGetProperty("groups", out var groups) ? JsonSerializer.Deserialize<object>(groups.GetRawText()) : new { }
    };

    var updateJson = JsonSerializer.Serialize(updatedProfile);
    var updateRequest = new StringContent(updateJson, Encoding.UTF8, MediaTypeNames.Application.Json);

    // 5. User Profile'ı güncelle
    var updateResponse = await httpClient.PutAsync($"/admin/realms/{realm}/users/profile", updateRequest);

    if (updateResponse.IsSuccessStatusCode)
        return Results.Ok(new { message = $"Realm seviyesinde {request.NewAttributes.Count} adet yeni attribute başarıyla tanımlandı!" });

    var errorContent = await updateResponse.Content.ReadAsStringAsync();
    return Results.BadRequest(new { error = errorContent });
});

app.MapPut("/user/{userId}/add-new-attribute/{realm}", async (string userId, string realm, TokenRequestService tokenRequestService, IHttpClientFactory httpClientFactory, AddAttributeRequest request) =>
{
    var token = await tokenRequestService.GetAccessTokenAsync();
    using var httpClient = httpClientFactory.CreateClient("keycloak");

    httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token.AccessToken);

    // 1. Mevcut kullanıcıyı GET ile çek
    var getUserResponse = await httpClient.GetAsync($"/admin/realms/{realm}/users/{userId}");

    if (!getUserResponse.IsSuccessStatusCode)
        return Results.NotFound(new { error = "Kullanıcı bulunamadı!" });

    var userContent = await getUserResponse.Content.ReadAsStringAsync();
    var existingUser = JsonSerializer.Deserialize<JsonElement>(userContent);

    // 2. Mevcut attributes'ları al
    var existingAttributes = new Dictionary<string, List<string>>();

    if (existingUser.TryGetProperty("attributes", out var attributesElement))
        attributesElement
            .EnumerateObject()
            .Select(attribute => existingAttributes[attribute.Name] = attribute.Value
                                        .EnumerateArray()
                                        .Select(v => v.GetString() ?? string.Empty)
                                        .ToList());

    // 3. Yeni attribute'ları mevcut olanlara ekle
    request.NewAttributes.ToList().ForEach(newAttribute => existingAttributes[newAttribute.Key] = newAttribute.Value);

    // 4. Kullanıcıyı güncelle (tüm attributes ile birlikte)
    var updateUser = new
    {
        firstName = existingUser.GetProperty("firstName").GetString(),
        lastName = existingUser.GetProperty("lastName").GetString(),
        email = existingUser.GetProperty("email").GetString(),
        enabled = existingUser.GetProperty("enabled").GetBoolean(),
        attributes = existingAttributes
    };

    var updateJson = JsonSerializer.Serialize(updateUser);
    var updateRequest = new StringContent(updateJson, Encoding.UTF8, MediaTypeNames.Application.Json);

    var updateResponse = await httpClient.PutAsync($"/admin/realms/{realm}/users/{userId}", updateRequest);

    if (updateResponse.IsSuccessStatusCode)
        return Results.Ok(new { message = "Yeni attribute'lar başarıyla eklendi, mevcut olanlar korundu!" });

    var errorContent = await updateResponse.Content.ReadAsStringAsync();
    return Results.BadRequest(new { error = errorContent });
});

app.MapPost("/user/add-attributes/{realm}", async (string realm, TokenRequestService tokenRequestService, IHttpClientFactory httpClientFactory, UserRequest request) =>
{
    var token = await tokenRequestService.GetAccessTokenAsync();
    using var httpClient = httpClientFactory.CreateClient("keycloak");

    var user = new
    {
        username = request.Username,
        email = request.Email,
        enabled = request.Enabled,
        firstName = request.FirstName,
        lastName = request.LastName,
        attributes = request.CustomAttributes
    };

    var userJson = JsonSerializer.Serialize(user);
    var userRequest = new StringContent(userJson, Encoding.UTF8, MediaTypeNames.Application.Json);

    httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token.AccessToken);

    var response = await httpClient.PostAsync($"/admin/realms/{realm}/users", userRequest);

    if (response.IsSuccessStatusCode)
        return Results.Ok(new { message = "Kullanıcı başarıyla oluşturuldu ve özel alanlar eklendi!" });

    var errorContent = await response.Content.ReadAsStringAsync();
    return Results.BadRequest(new { error = errorContent });
});
#endregion
#region Kullanıcıya Geçici(Temporary) ve Kalıcı(Permanent) Şifre Tanımlama
app.MapPut("/user/{userId}/reset-password/{realm}", async (string userId, string realm, TokenRequestService tokenRequestService, IHttpClientFactory httpClientFactory, SetPasswordRequest request) =>
{
    var token = await tokenRequestService.GetAccessTokenAsync();
    using var httpClient = httpClientFactory.CreateClient("keycloak");

    httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token.AccessToken);

    var passwordData = new
    {
        type = "password",
        value = request.Password,
        temporary = request.IsTemporary
    };

    var passwordJson = JsonSerializer.Serialize(passwordData);
    var passwordRequest = new StringContent(passwordJson, Encoding.UTF8, MediaTypeNames.Application.Json);

    var response = await httpClient.PutAsync($"/admin/realms/{realm}/users/{userId}/reset-password", passwordRequest);

    if (response.IsSuccessStatusCode)
        return Results.Ok(new { message = $"Şifre başarıyla {(request.IsTemporary ? "geçici" : "kalıcı")} olarak ayarlandı!" });

    var errorContent = await response.Content.ReadAsStringAsync();
    return Results.BadRequest(new { error = errorContent });
});
#endregion
#region Password Grant Type İle Kullanıcıyı Doğrulama
app.MapPost("/user/authenticate/{realm}", async (string realm, IHttpClientFactory httpClientFactory, PasswordGrantRequest request) =>
{
    using var httpClient = httpClientFactory.CreateClient("keycloak");

    var formData = new Dictionary<string, string>
    {
        ["grant_type"] = "password",
        ["client_id"] = request.ClientId,
        ["username"] = request.Username,
        ["password"] = request.Password
    };

    if (!string.IsNullOrEmpty(request.ClientSecret))
        formData["client_secret"] = request.ClientSecret;

    if (!string.IsNullOrEmpty(request.Scope))
        formData["scope"] = request.Scope;

    var formContent = new FormUrlEncodedContent(formData);

    var response = await httpClient.PostAsync($"/realms/{realm}/protocol/openid-connect/token", formContent);

    if (response.IsSuccessStatusCode)
    {
        var tokenContent = await response.Content.ReadAsStringAsync();
        var tokenResponse = JsonSerializer.Deserialize<JsonElement>(tokenContent);

        return Results.Ok(new
        {
            accessToken = tokenResponse.GetProperty("access_token").GetString(),
            refreshToken = tokenResponse.TryGetProperty("refresh_token", out var rt) ? rt.GetString() : null,
            expiresIn = tokenResponse.GetProperty("expires_in").GetInt32(),
            tokenType = tokenResponse.GetProperty("token_type").GetString()
        });
    }

    var errorContent = await response.Content.ReadAsStringAsync();
    return Results.Unauthorized();
});
#endregion
#region Kullanıcının Aktif Oturum (Session) Bilgilerine Erişme
app.MapGet("/user/{userId}/sessions/{realm}", async (string userId, string realm, TokenRequestService tokenRequestService, IHttpClientFactory httpClientFactory) =>
{
    var token = await tokenRequestService.GetAccessTokenAsync();
    using var httpClient = httpClientFactory.CreateClient("keycloak");

    httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token.AccessToken);

    var response = await httpClient.GetAsync($"/admin/realms/{realm}/users/{userId}/sessions");

    if (response.IsSuccessStatusCode)
    {
        var sessionsContent = await response.Content.ReadAsStringAsync();
        var sessions = JsonSerializer.Deserialize<JsonElement>(sessionsContent);

        return Results.Ok(new
        {
            userId,
            activeSessions = sessions.EnumerateArray().Select(session => new
            {
                id = session.GetProperty("id").GetString(),
                username = session.TryGetProperty("username", out var un) ? un.GetString() : null,
                ipAddress = session.TryGetProperty("ipAddress", out var ip) ? ip.GetString() : null,
                start = session.TryGetProperty("start", out var st) ? DateTimeOffset.FromUnixTimeMilliseconds(st.GetInt64()).DateTime : (DateTime?)null,
                lastAccess = session.TryGetProperty("lastAccess", out var la) ? DateTimeOffset.FromUnixTimeMilliseconds(la.GetInt64()).DateTime : (DateTime?)null,
                clients = session.TryGetProperty("clients", out var cl) ? cl.EnumerateObject().ToDictionary(c => c.Name, c => c.Value.GetString()) : null
            }).ToList()
        });
    }

    if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
        return Results.NotFound(new { error = "Kullanıcı bulunamadı!" });

    var errorContent = await response.Content.ReadAsStringAsync();
    return Results.BadRequest(new { error = errorContent });
});
#endregion
#region UserInfo Endpoint ile Kullanıcı Bilgilerine Erişim
app.MapGet("/user/userinfo/{realm}", async (string realm, IHttpClientFactory httpClientFactory, [Microsoft.AspNetCore.Mvc.FromHeader(Name = "Authorization")] string? authorization) =>
{
    using var httpClient = httpClientFactory.CreateClient("keycloak");

    if (string.IsNullOrEmpty(authorization) || !authorization.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
        return Results.Unauthorized();

    var accessToken = authorization.Substring("Bearer ".Length).Trim();

    httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

    var response = await httpClient.GetAsync($"/realms/{realm}/protocol/openid-connect/userinfo");

    if (response.IsSuccessStatusCode)
    {
        var userInfoContent = await response.Content.ReadAsStringAsync();
        var userInfo = JsonSerializer.Deserialize<JsonElement>(userInfoContent);

        return Results.Ok(userInfo);
    }

    if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
        return Results.Unauthorized();

    var errorContent = await response.Content.ReadAsStringAsync();
    return Results.BadRequest(new { error = errorContent });
});
#endregion
app.Run();