using Keycloak8.Keycloakda_Advanced_Authorization_Services_UMA_Sistemini_İnceleyelim.ViewModels;

namespace Keycloak8.Keycloakda_Advanced_Authorization_Services_UMA_Sistemini_İnceleyelim.Services
{
    public class UmaTokenService(IHttpClientFactory httpClientFactory, IConfiguration configuration)
    {
        //Keycloak'a -bu kaynak için izin bileti ver- diyen metottur.
        //Dönen ticket, 401 response içinde client'a gönderilir.
        //Client bu ticket'ı alıp Keycloak'a götürerek RPT edinir.
        public async Task<string?> GetPermissionTicketAsync(string resourceId, string scope)
        {
            var httpClient = httpClientFactory.CreateClient();

            //Önce resource server'ın kendi access token'ını alıyoruz.
            var resourceServerToken = await GetResourceServerAccessTokenAsync(httpClient);

            httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", resourceServerToken);

            //UMA Permission Endpoint'ine istek atarak permission ticket alıyoruz.
            var permissionEndpoint = $"{configuration["Keycloak:Authority"]}/authz/protection/permission";
            var body = new[] {
                new {
                    resource_id = resourceId,
                    resource_scopes = new[] { scope }
                }
            };
            var response = await httpClient.PostAsJsonAsync(permissionEndpoint, body);
            if (!response.IsSuccessStatusCode) return null;

            var result = await response.Content.ReadFromJsonAsync<PermissionTicketResponse>();
            return result.Ticket;

            async Task<string?> GetResourceServerAccessTokenAsync(HttpClient httpClient)
            {
                var formData = new Dictionary<string, string>
                {
                    ["grant_type"] = "client_credentials",
                    ["client_id"] = configuration["Keycloak:ClientId"]!,
                    ["client_secret"] = configuration["Keycloak:ClientSecret"]!
                };

                var response = await httpClient.PostAsync(configuration["Keycloak:TokenEndpoint"]!, new FormUrlEncodedContent(formData));
                var result = await response.Content.ReadFromJsonAsync<TokenResponse>();

                return result?.AccessToken ?? throw new Exception("Resource server token alınamadı!");
            }
        }

        public async Task<string?> ExchangeForRptAsync(string userAccessToken, string permissionTicket)
        {
            var httpClient = httpClientFactory.CreateClient();
            var formData = new Dictionary<string, string>
            {
                ["grant_type"] = "urn:ietf:params:oauth:grant-type:uma-ticket",
                ["client_id"] = configuration["Keycloak:ClientId"]!,
                ["client_secret"] = configuration["Keycloak:ClientSecret"]!,
                ["ticket"] = permissionTicket
            };
            httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", userAccessToken);
            var response = await httpClient.PostAsync(configuration["Keycloak:TokenEndpoint"]!, new FormUrlEncodedContent(formData));

            if (!response.IsSuccessStatusCode) return null;

            var result = await response.Content.ReadFromJsonAsync<TokenResponse>();
            return result?.AccessToken;
        }
    }
}
