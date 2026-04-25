using Microsoft.AspNetCore.Authentication;
using System.Security.Claims;
using System.Text.Json;

namespace Keycloak8.Keycloakda_Advanced_Authorization_Services_UMA_Sistemini_İnceleyelim.Transformations
{
    public class KeycloakRolesTransformer : IClaimsTransformation
    {
        public Task<ClaimsPrincipal> TransformAsync(ClaimsPrincipal principal)
        {
            var identity = (ClaimsIdentity)principal.Identity!;
            var realmAccess = identity.FindFirst("realm_access");

            if (realmAccess is not null)
            {
                var json = JsonDocument.Parse(realmAccess.Value);
                if (json.RootElement.TryGetProperty("roles", out var roles))
                    foreach (var role in roles.EnumerateArray())
                    {
                        var r = role.GetString();
                        if (!string.IsNullOrEmpty(r))
                            identity.AddClaim(new Claim(ClaimTypes.Role, r));
                    }
            }

            return Task.FromResult(principal);
        }
    }
}
