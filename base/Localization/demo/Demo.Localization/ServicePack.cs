using System;
using System.Globalization;
using Annium.Core.DependencyInjection;

namespace Demo.Localization;

internal class ServicePack : ServicePackBase
{
    public override void Configure(IServiceContainer container)
    {
        // register configurations
    }

    public override void Register(IServiceContainer container, IServiceProvider provider)
    {
        container.AddLocalization(opts => opts.UseYamlStorage().UseCulture(() => CultureInfo.CurrentCulture));
    }

    public override void Setup(IServiceProvider provider)
    {
        // setup post-configured services
    }
}