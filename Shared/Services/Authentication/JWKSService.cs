using Microsoft.IdentityModel.Tokens;

namespace Shared.Services.Authentication
{
    public class JWKSService
    {
        public static async Task<IList<SecurityKey>> GetKeysAsync(HttpClient httpClient, string realm = "master")
        {
            // Keycloak'un JWKS endpoint'inden public key'leri alıyoruz.
            var certsResponse = await httpClient.GetStringAsync($"/realms/{realm}/protocol/openid-connect/certs");
            // JSON string'i JsonWebKeySet nesnesine dönüştürüyoruz. Bu, içerisinde token'ı doğrulamak için kullanılacak key'leri barındıran bir koleksiyondur.
            var certsJWKS = new JsonWebKeySet(certsResponse);
            // Key set'inden signed key'leri ayıklayıp, gönderiyoruz.
            var keys = certsJWKS.GetSigningKeys();
            return keys;
        }
    }
}
