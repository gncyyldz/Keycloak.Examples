using System.IdentityModel.Tokens.Jwt;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Keycloak8.Keycloakda_Advanced_Authorization_Services_UMA_Sistemini_İnceleyelim.Services
{
    public class RptValidator
    {
        /// <summary>
        /// RPT'nin payload'ını decode edip içindeki izinlerin istenen kaynağa ve scope'a sahip olup olmadığını kontrol ediyoruz.
        /// 
        /// RPT payload örneği : 
        ///  
        /// {
        ///   "authorization": {
        ///    "permissions": [
        ///      {
        ///        "scopes": [
        ///          "read"
        ///        ],
        ///        "rsid": "dbfb5c18-4166-4638-8e84-faff8f5f9126",
        ///        "rsname": "Document Resource"
        ///      }
        ///    ]
        ///  }
        /// 
        /// 
        /// </summary>
        public bool HasPermission(string rptToken, string resourceName, string scope)
        {
            try
            {
                var jwtSecurityTokenHandler = new JwtSecurityTokenHandler();
                var jwt = jwtSecurityTokenHandler.ReadJwtToken(rptToken);

                var authorizationClaim = jwt.Claims.FirstOrDefault(c => c.Type == "authorization")?.Value;

                if (authorizationClaim is null) return false;

                var authData = JsonSerializer.Deserialize<AuthorizationClaim>(authorizationClaim);

                return authData?.Permissions?.Any(p => p.ResourceName == resourceName && (p.Scopes?.Contains(scope) ?? false)) ?? false;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }


    public record AuthorizationClaim(
        [property: JsonPropertyName("permissions")] List<PermissionClaim>? Permissions
    );

    public record PermissionClaim(
        [property: JsonPropertyName("rsid")] string? ResourceId,
        [property: JsonPropertyName("rsname")] string? ResourceName,
        [property: JsonPropertyName("scopes")] List<string>? Scopes
    );
}