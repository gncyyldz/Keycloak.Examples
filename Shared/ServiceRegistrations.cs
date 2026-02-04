using Microsoft.Extensions.DependencyInjection;
using Shared.Services;
using Shared.Services.Authentication;

namespace Shared
{
    public static class ServiceRegistrations
    {
        public static void AddRegistrations(this IServiceCollection serviceCollection)
        {
            serviceCollection.AddSingleton<TokenRequestService>();
            serviceCollection.AddSingleton<ManualJwtValidator>();
            serviceCollection.AddSingleton<JwtAuthenticationMiddleware>();
        }
    }
}
