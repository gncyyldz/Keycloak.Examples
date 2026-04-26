using Keycloak8.Keycloakda_Advanced_Authorization_Services_UMA_Sistemini_İnceleyelim.Requirements;
using Keycloak8.Keycloakda_Advanced_Authorization_Services_UMA_Sistemini_İnceleyelim.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Policy;
using Microsoft.Net.Http.Headers;
using System.Net;

namespace Keycloak8.Keycloakda_Advanced_Authorization_Services_UMA_Sistemini_İnceleyelim.Middlewares
{
    public class UmaAuthorizationMiddlewareResultHandler(UmaTokenService umaTokenService) : IAuthorizationMiddlewareResultHandler
    {
        private readonly AuthorizationMiddlewareResultHandler _default = new();
        public async Task HandleAsync(RequestDelegate next, HttpContext context, AuthorizationPolicy policy, PolicyAuthorizationResult authorizeResult)
        {
            //Authorization başarılıysa direkt geçiriyoruz
            if (authorizeResult.Succeeded)
            {
                await next(context);
                return;
            }

            //Hatalıysa nedenine bakıyoruz
            var reason = authorizeResult.AuthorizationFailure?.FailureReasons.FirstOrDefault()?.Message;

            //RPT varsa ama yetersiz permission'sa → 403
            if (reason == "permission_denied")
            {
                context.Response.StatusCode = (int)HttpStatusCode.Forbidden;
                await context.Response.WriteAsJsonAsync(new
                {
                    error = "forbidden",
                    message = "Bu resource için yetkiniz bulunmamaktadır."
                });
                return;
            }

            //Token yoksa ya da RPT değilse → 401
            if (reason is "no_token" or "not_rpt")
            {
                //Hangi endpoint için ticket isteniyorsa bunu requirement'tan almak için policy'e bakıyoruz
                var requirement = policy.Requirements.OfType<UmaPermissionRequirement>().FirstOrDefault();

                if (requirement == null)
                {
                    context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                    return;
                }

                var ticket = await umaTokenService.GetPermissionTicketAsync(requirement.ResourceName, requirement.Scope);

                if (ticket == null)
                {
                    context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                    return;
                }

                context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                context.Response.Headers[HeaderNames.WWWAuthenticate] = $""" UMA realm="master", as_uri="http://localhost:8080/realms/master", ticket="{ticket}" """;

                await context.Response.WriteAsJsonAsync(new
                {
                    error = "uma_redirect",
                    message = "Permission ticket alındı. Bu ticket ile RPT edinin!",
                    ticket = ticket,
                    token_endpoint = "http://localhost:8080/realms/master/protocol/openid-connect/token"
                });
            }

            //Token süresi dolması vs. gibi diğer durumlar için default davranışı devreye alıyoruz
            await _default.HandleAsync(next, context, policy, authorizeResult);
        }
    }
}
