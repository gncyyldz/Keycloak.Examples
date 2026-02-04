using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace Shared.Services.Authentication
{
    public class ManualJwtValidator(IHttpClientFactory _httpClientFactory)
    {
        public async Task<ClaimsPrincipal> ValidateAsync(string token, string realm = "master")
        {
            var httpClient = _httpClientFactory.CreateClient("keycloak");
            var jwtSecurityTokenHandler = new JwtSecurityTokenHandler();

            var tokenValidationParameters = new TokenValidationParameters
            {
                // Token'ı kimin oluşturduğu kontrol edilecek.
                ValidateIssuer = true,
                // Beklenen oluşturucu adresi (Keycloak realm URL'i)
                ValidIssuer = $"{httpClient.BaseAddress.AbsoluteUri}realms/{realm}",
                // Token'ın kime hitap ettiği kontrol edilecek.
                ValidateAudience = true,
                // Beklenen hedef kitle.
                ValidAudience = "account",
                // Token'ın geçerlilik süresi kontrol edilecek.
                ValidateLifetime = true,
                // Token imzasının doğruluğunu kontrol edilecek.
                ValidateIssuerSigningKey = true,
                // Token imzasını doğrulamak için public key'i alınmakta ve doğrulanmaktadır.
                IssuerSigningKeyResolver = (token, securityToken, kid, validationParameters) =>
                {
                    // Keycloak'tan JWKS endpoint'inden public key getirilmektedir.
                    var publicKey = JWKSService.GetKeysAsync(httpClient, "master").Result;
                    return publicKey;
                },
                // JWT doğrulama işlemi, claim'leri otomatik olarak 'Identity.Name' alanına eşlememektedir! Bu alanın hangi claim ile dolacağı bu property üzerinden belirlenmelidir.
                NameClaimType = JwtRegisteredClaimNames.PreferredUsername
            };

            var tokenValidationResult = await jwtSecurityTokenHandler.ValidateTokenAsync(token, tokenValidationParameters);
            return new ClaimsPrincipal(tokenValidationResult.ClaimsIdentity);
        }
    }
}