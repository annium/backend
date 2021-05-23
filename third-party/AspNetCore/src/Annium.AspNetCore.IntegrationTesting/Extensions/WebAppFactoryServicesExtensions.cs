using Annium.Core.DependencyInjection;

namespace Annium.AspNetCore.IntegrationTesting
{
    public static class WebAppFactoryServicesExtensions
    {
        public static T Resolve<T>(
            this IWebApplicationFactory appFactory
        )
            where T : notnull
            => appFactory.ServiceProvider.Resolve<T>();
    }
}