using System;
using Annium.linq2db.Extensions;
using Annium.Logging.Abstractions;
using LinqToDB.Data;
using Microsoft.Extensions.DependencyInjection;

namespace Annium.Core.DependencyInjection
{
    public static class ServiceProviderExtensions
    {
        public static IServiceProvider UseConnectionTracing<TConnection>(
            this IServiceProvider provider
        )
            where TConnection : DataConnectionBase
        {
            var logger = provider.GetRequiredService<ILogger<TConnection>>();
            DataConnection.TurnTraceSwitchOn();
            DataConnection.WriteTraceLine = (message, displayName) => logger.Trace($"{message} {displayName}");

            return provider;
        }
    }
}