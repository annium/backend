using Annium.Net.Mail;

namespace Annium.Core.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceContainer AddEmailService(this IServiceContainer container)
        {
            container.Add<IEmailService, EmailService>().Singleton();

            return container;
        }

        public static IServiceContainer AddTestEmailService(this IServiceContainer container, TestEmailService service)
        {
            container.Add<IEmailService>(service).Singleton();

            return container;
        }
    }
}