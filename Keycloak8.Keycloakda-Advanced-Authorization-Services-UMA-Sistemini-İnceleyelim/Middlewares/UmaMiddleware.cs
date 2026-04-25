using Keycloak8.Keycloakda_Advanced_Authorization_Services_UMA_Sistemini_İnceleyelim.Services;
using Microsoft.Net.Http.Headers;
using System.IdentityModel.Tokens.Jwt;
using System.Net;

namespace Keycloak8.Keycloakda_Advanced_Authorization_Services_UMA_Sistemini_İnceleyelim.Middlewares
{
    public class UmaMiddleware(UmaTokenService umaTokenService, RptValidator rptValidator) : IMiddleware
    {
        public async Task InvokeAsync(HttpContext context, RequestDelegate next)
        {
            var path = context.Request.Path.Value ?? "";

            //UMA koruması altındaki path'leri belirliyoruz. Örneğin /documents/{id} path'i için id'yi alıp o kaynağa erişim izni olup olmadığını kontrol edeceğiz.
            if (!path.StartsWith("/api/documents", StringComparison.OrdinalIgnoreCase))
            {
                await next(context);
                return;
            }

            var authorization = context.Request.Headers.Authorization.ToString();

            if (string.IsNullOrEmpty(authorization) || !authorization.StartsWith("Bearer "))
            {
                //Token yok → UMA akışını başlat
                await ReturnPermissionTicket(context, "Document Resource", "read");
                return;
            }

            var token = authorization["Bearer ".Length..];

            //Token RPT mi kontrol ediyoruz... (authorization code içeriyor mu kontrol ediyoruz)
            var jwtSecurityTokenHandler = new JwtSecurityTokenHandler();
            var jwt = jwtSecurityTokenHandler.ReadJwtToken(token);
            if (jwt.Claims.Any(c => c.Type == "authorization"))
            {
                var requiredScope = GetScopeForMethod(context.Request.Method);

                if (rptValidator.HasPermission(token, "Document Resource", requiredScope))
                    await next(context);
                else
                {
                    context.Response.StatusCode = (int)HttpStatusCode.Forbidden;
                    await context.Response.WriteAsync("Bu kaynak için yetkiniz bulunmamaktadır!");
                }
            }
            else
            {
                //Normal access token ise permission ticket üretiyor ve 401 dönüyoruz. Client bu ticket'ı alıp Keycloak'tan RPT edinir ve tekrar istekte bulunur.
                await ReturnPermissionTicket(context, "Document Resource", GetScopeForMethod(context.Request.Method));
            }

            string GetScopeForMethod(string method) => method switch
            {
                "GET" => "read",
                "POST" => "write",
                "PUT" => "write",
                "DELETE" => "delete",
                _ => "read"
            };
        }

        async Task ReturnPermissionTicket(HttpContext context, string resourceId, string scope)
        {
            var ticket = await umaTokenService.GetPermissionTicketAsync(resourceId, scope);

            if (ticket is null)
            {
                context.Response.StatusCode = 500;
                return;
            }

            context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;

            //WWW-Authenticate header'ında UMA realm, as_uri ve ticket bilgilerini client'a gönderiyoruz. Client bu bilgileri kullanarak Keycloak'tan RPT alacak.
            //Bu bir UMA standardıdır ve client'ların 401 response aldıklarında ne yapmaları gerektiğini anlamalarını sağlar.
            context.Response.Headers[HeaderNames.WWWAuthenticate] = $""" UMA realm="master", as_uri="http://localhost:8080/realms/master", ticket="{ticket}" """;

            await context.Response.WriteAsJsonAsync(new
            {
                error = "uma_redirect",
                message = "Permission ticket alındı. Bu ticket ile RPT edinin!",
                ticket = ticket,
                token_endpoint = "http://localhost:8080/realms/master/protocol/openid-connect/token"
            });
        }
    }
}
