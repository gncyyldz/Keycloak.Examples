using Shared.Modals.Responses;
using System.Net.Http.Json;

namespace Shared.Services
{
    public class TokenRequestService(IHttpClientFactory httpClientFactory)
    {
        public async Task<AccessTokenResponse> GetAccessTokenAsync()
        {
            var httpClient = httpClientFactory.CreateClient("keycloak");
            var formData = new Dictionary<string, string>
            {
                ["grant_type"] = "client_credentials",
                ["client_id"] = "restapi-playground",
                ["client_secret"] = "TSQ7dhxgymeqxPunV18S3WQY6dmBY9Il"
            };
            var content = new FormUrlEncodedContent(formData);
            var response = await httpClient.PostAsync("/realms/master/protocol/openid-connect/token", content);
            var result = await response.Content.ReadFromJsonAsync<AccessTokenResponse>();
            return result;
        }
    }
}
