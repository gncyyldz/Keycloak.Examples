using Keycloak8.Keycloakda_Advanced_Authorization_Services_UMA_Sistemini_İnceleyelim.Requirements;
using Keycloak8.Keycloakda_Advanced_Authorization_Services_UMA_Sistemini_İnceleyelim.Services;
using Microsoft.AspNetCore.Authorization;
using System.IdentityModel.Tokens.Jwt;

namespace Keycloak8.Keycloakda_Advanced_Authorization_Services_UMA_Sistemini_İnceleyelim.Handlers
{
    public class UmaPermissionHandler(RptValidator rptValidator) : AuthorizationHandler<UmaPermissionRequirement>
    {
        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, UmaPermissionRequirement requirement)
        {
            if (context.Resource is not HttpContext httpContext)
            {
                context.Fail();
                return Task.CompletedTask;
            }

            var authorizationHeader = httpContext.Request.Headers.Authorization.ToString();

            //Hiç token yoksa başarısız authorization'ın detaylarıyla birlikte Fail'liyoruz
            if (string.IsNullOrEmpty(authorizationHeader) || !authorizationHeader.StartsWith("Bearer "))
            {
                context.Fail(new AuthorizationFailureReason(this, "no_token"));
                return Task.CompletedTask;
            }

            var token = httpContext.Request.Headers.Authorization.ToString().Replace("Bearer ", "");

            //Eğer token varsa RPT'mi değil mi kontrol ediyoruz
            var jwtSecurityTokenHandler = new JwtSecurityTokenHandler();
            var jwt = jwtSecurityTokenHandler.ReadJwtToken(token);
            if (!jwt.Claims.Any(c => c.Type == "authorization"))
            {
                context.Fail(new AuthorizationFailureReason(this, "not_rpt"));
                return Task.CompletedTask;
            }

            //RPT ise permission'ları kontrol ediyoruz.
            if (rptValidator.HasPermission(token, requirement.ResourceName, requirement.Scope))
                context.Succeed(requirement);
            else
                context.Fail();

            return Task.CompletedTask;
        }
    }
}
