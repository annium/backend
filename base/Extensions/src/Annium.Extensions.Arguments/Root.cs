using System;
using Annium.Extensions.Arguments.Internal;

namespace Annium.Extensions.Arguments;

internal class Root
{
    public IServiceProvider Provider { get; }

    public IConfigurationBuilder ConfigurationBuilder { get; }

    public IHelpBuilder HelpBuilder { get; }

    public Root(
        IServiceProvider provider,
        IConfigurationBuilder configurationBuilder,
        IHelpBuilder helpBuilder
    )
    {
        Provider = provider;
        ConfigurationBuilder = configurationBuilder;
        HelpBuilder = helpBuilder;
    }
}