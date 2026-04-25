using Keycloak8.Keycloakda_Advanced_Authorization_Services_UMA_Sistemini_İnceleyelim.Entities;
using Keycloak8.Keycloakda_Advanced_Authorization_Services_UMA_Sistemini_İnceleyelim.Requirements;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace Keycloak8.Keycloakda_Advanced_Authorization_Services_UMA_Sistemini_İnceleyelim.Handlers
{
    public class EditPostHandler : AuthorizationHandler<EditPostRequirement, Post>
    {
        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, EditPostRequirement requirement, Post resource)
        {
            //ClaimTypes.NameIdentifier : sub
            if (resource.OwnerId.ToString() == context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value)
                context.Succeed(requirement);

            return Task.CompletedTask;
        }
    }
}
