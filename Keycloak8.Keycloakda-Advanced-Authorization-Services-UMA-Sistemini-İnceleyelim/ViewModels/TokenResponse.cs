using System.Text.Json.Serialization;

namespace Keycloak8.Keycloakda_Advanced_Authorization_Services_UMA_Sistemini_İnceleyelim.ViewModels
{
    public record TokenResponse([property: JsonPropertyName("access_token")] string AccessToken);
}
