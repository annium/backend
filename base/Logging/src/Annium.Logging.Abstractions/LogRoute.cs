using System;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;

namespace Annium.Logging.Abstractions
{
    public class LogRoute
    {
        internal Func<LogMessage, bool> Filter { get; private set; } = m => true;
        internal ServiceDescriptor? Service { get; private set; }
        private readonly Action<LogRoute> _registerRoute;

        internal LogRoute(Action<LogRoute> registerRoute)
        {
            this._registerRoute = registerRoute;

            registerRoute(this);
        }

        public LogRoute For(Func<LogMessage, bool> filter) => new LogRoute(_registerRoute) { Filter = filter };

        public LogRoute Use(ServiceDescriptor descriptor)
        {
            if (!descriptor.ServiceType.GetInterfaces().Contains(typeof(ILogHandler)))
                throw new ArgumentException($"{descriptor.ServiceType} must implement {typeof(ILogHandler)} to be used as log handler");

            Service = descriptor;

            return this;
        }
    }
}