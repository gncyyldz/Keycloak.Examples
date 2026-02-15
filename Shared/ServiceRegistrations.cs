using Microsoft.Extensions.DependencyInjection;
using Shared.Services;

namespace Shared
{
    public static class ServiceRegistrations
    {
        public static void AddRegistrations(this IServiceCollection serviceCollection)
        {
            serviceCollection.AddSingleton<TokenRequestService>();
        }
    }
}
