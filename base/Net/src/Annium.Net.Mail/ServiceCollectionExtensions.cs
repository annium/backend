using Annium.Net.Mail;
using Microsoft.Extensions.DependencyInjection;

namespace Annium.Core.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddEmailService(this IServiceCollection services)
        {
            services.AddSingleton<IEmailService, EmailService>();

            return services;
        }

        public static IServiceCollection AddTestEmailService(this IServiceCollection services, TestEmailService service)
        {
            services.AddSingleton<IEmailService>(service);

            return services;
        }
    }
}