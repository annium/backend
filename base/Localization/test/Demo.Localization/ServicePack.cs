using System;
using System.Globalization;
using Annium.Core.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;

namespace Demo.Localization
{
    internal class ServicePack : ServicePackBase
    {
        public override void Configure(IServiceCollection services)
        {
            // register configurations
        }

        public override void Register(IServiceCollection services, IServiceProvider provider)
        {
            services.AddLocalization(opts => opts.UseYamlStorage().UseCulture(() => CultureInfo.CurrentCulture));
        }

        public override void Setup(System.IServiceProvider provider)
        {
            // setup post-configured services
        }
    }
}