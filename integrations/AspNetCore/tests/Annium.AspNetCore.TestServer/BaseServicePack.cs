using System;
using Annium.Architecture.CQRS;
using Annium.Architecture.Http;
using Annium.AspNetCore.Extensions.Extensions;
using Annium.AspNetCore.TestServer.Components;
using Annium.Core.DependencyInjection.Container;
using Annium.Core.DependencyInjection.Extensions;
using Annium.Core.DependencyInjection.Packs;
using Annium.Core.Mediator;
using Annium.Core.Runtime;
using Annium.Core.Runtime.Types;
using Annium.Data.Operations.Serialization.Json;
using Annium.Logging.Shared;
using Annium.Logging.Xunit;
using Annium.Net.Http;
using Annium.NodaTime.Serialization.Json;
using Annium.Serialization.Abstractions;
using Annium.Serialization.Json;
using Microsoft.Extensions.DependencyInjection;

namespace Annium.AspNetCore.TestServer;

internal class BaseServicePack : ServicePackBase
{
    public override void Register(IServiceContainer container, IServiceProvider provider)
    {
        // register and setup services
        container.AddRuntime(GetType().Assembly);
        container.AddSerializers().WithJson(opts => opts.ConfigureForOperations().ConfigureForNodaTime());
        container.AddLogging();
        container.AddHttpRequestFactory(true);
        container.AddMediatorConfiguration(ConfigureMediator);
        container.AddMediator();
        container.Add<SharedDataContainer>().AsSelf().Singleton();

        // server
        container.Collection.AddControllers();
        container.Collection.AddCors();
        container.Collection.AddMvc().AddDefaultJsonOptions();
    }

    public override void Setup(IServiceProvider provider)
    {
        provider.UseLogging(route => route.UseTestOutput());
    }

    private void ConfigureMediator(MediatorConfiguration cfg, ITypeManager tm)
    {
        cfg.AddHttpStatusPipeHandler();
        cfg.AddCommandQueryHandlers(tm);
    }
}
