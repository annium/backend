using System;
using Annium.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;

namespace Annium.Extensions.Arguments
{
    public class ServicePack : ServicePackBase
    {
        public override void Register(IServiceCollection services, IServiceProvider provider)
        {
            services.AddSingleton<IArgumentProcessor, ArgumentProcessor>();
            services.AddSingleton<IConfigurationBuilder, ConfigurationBuilder>();
            services.AddSingleton<IConfigurationProcessor, ConfigurationProcessor>();
            services.AddSingleton<IHelpBuilder, HelpBuilder>();
            services.AddSingleton<Root, Root>();
        }
    }
}