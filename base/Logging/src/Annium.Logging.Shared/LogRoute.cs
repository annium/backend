using System;
using System.Linq;
using Annium.Core.DependencyInjection;

namespace Annium.Logging.Shared
{
    public class LogRoute
    {
        internal Func<LogMessage, bool> Filter { get; private set; } = _ => true;
        internal IServiceDescriptor? Service { get; private set; }
        private readonly Action<LogRoute> _registerRoute;

        internal LogRoute(Action<LogRoute> registerRoute)
        {
            _registerRoute = registerRoute;

            registerRoute(this);
        }

        public LogRoute For(Func<LogMessage, bool> filter) => new(_registerRoute) { Filter = filter };

        public LogRoute Use(IServiceDescriptor descriptor)
        {
            if (descriptor.ServiceType != typeof(ILogHandler) && !descriptor.ServiceType.GetInterfaces().Contains(typeof(ILogHandler)))
                throw new ArgumentException($"{descriptor.ServiceType} must implement {typeof(ILogHandler)} to be used as log handler");

            Service = descriptor;

            return this;
        }
    }
}