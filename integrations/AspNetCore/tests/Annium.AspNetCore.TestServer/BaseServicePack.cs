using System;
using System.Threading;
using System.Threading.Tasks;
using Annium.Architecture.CQRS;
using Annium.Architecture.Http;
using Annium.AspNetCore.Extensions;
using Annium.AspNetCore.TestServer.Components;
using Annium.Core.DependencyInjection;
using Annium.Core.Mediator;
using Annium.Core.Runtime;
using Annium.Core.Runtime.Types;
using Annium.Data.Operations.Serialization.Json;
using Annium.Logging.Shared;
using Annium.Logging.Xunit;
using Annium.NodaTime.Serialization.Json;
using Annium.Serialization.Abstractions;
using Annium.Serialization.Json;
using Microsoft.Extensions.DependencyInjection;

namespace Annium.AspNetCore.TestServer;

/// <summary>
/// Base service pack that provides common services for AspNetCore test server
/// </summary>
internal class BaseServicePack : ServicePackBase
{
    /// <summary>
    /// Registers common services required for the test server
    /// </summary>
    /// <param name="container">The service container to register services with</param>
    /// <param name="provider">The service provider for dependency resolution</param>
    /// <param name="ct">The cancellation token.</param>
    public override Task RegisterAsync(IServiceContainer container, IServiceProvider provider, CancellationToken ct)
    {
        // register and setup services
        container.AddRuntime(GetType().Assembly);
        container.AddSerializers().WithJson(opts => opts.ConfigureForOperations().ConfigureForNodaTime());
        container.AddLogging();
        container.AddMediatorConfiguration(ConfigureMediator);
        container.AddMediator();
        container.Add<SharedDataContainer>().AsSelf().Singleton();

        // server
        container.Collection.AddControllers();
        container.Collection.AddCors();
        container.Collection.AddMvc().AddDefaultJsonOptions();
        return Task.CompletedTask;
    }

    /// <summary>
    /// Sets up logging configuration for the test server
    /// </summary>
    /// <param name="provider">The service provider containing registered services</param>
    /// <param name="ct">The cancellation token.</param>
    public override Task SetupAsync(IServiceProvider provider, CancellationToken ct)
    {
        provider.UseLogging(route => route.UseTestOutput());
        return Task.CompletedTask;
    }

    /// <summary>
    /// Configures the mediator with HTTP status handling and command/query handlers
    /// </summary>
    /// <param name="cfg">The mediator configuration to configure</param>
    /// <param name="tm">The type manager for handler discovery</param>
    private void ConfigureMediator(MediatorConfiguration cfg, ITypeManager tm)
    {
        cfg.AddHttpStatusPipeHandler();
        cfg.AddCommandQueryHandlers(tm);
    }
}
