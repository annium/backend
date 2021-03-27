using System;
using Annium.Infrastructure.MessageBus.Node;
using Annium.Infrastructure.MessageBus.Node.Internal;

namespace Annium.Core.DependencyInjection
{
    public static class ServiceContainerExtensions
    {
        public static IServiceContainer AddMessageBus(
            this IServiceContainer container,
            Action<IServiceProvider, IConfigurationBuilder> configure
        )
        {
            container.Add(sp =>
            {
                var builder = sp.Resolve<ConfigurationBuilder>();
                configure(sp, builder);
                return builder.Build();
            }).AsSelf().Singleton();
            container.Add<IMessageBusSocket, MessageBusSocket>().Transient();
            container.Add<ConfigurationBuilder>().AsSelf().Transient();

            return container.AddMessageBusBase();
        }

        public static IServiceContainer AddTestMessageBus(
            this IServiceContainer container,
            Action<IServiceProvider, ITestConfigurationBuilder> configure
        )
        {
            container.Add(sp =>
            {
                var builder = sp.Resolve<TestConfigurationBuilder>();
                configure(sp, builder);
                return builder.Build();
            }).AsSelf().Singleton();
            container.Add<IMessageBusSocket, TestMessageBusSocket>().Transient();
            container.Add<TestConfigurationBuilder>().AsSelf().Transient();

            return container.AddMessageBusBase();
        }

        private static IServiceContainer AddMessageBusBase(this IServiceContainer container)
        {
            container.Add<IMessageBusNode, MessageBusNode>().Transient();

            container.Add<IMessageBusClient, MessageBusClient>().Transient();
            container.Add<IMessageBusServer, MessageBusServer>().Transient();

            return container;
        }
    }
}