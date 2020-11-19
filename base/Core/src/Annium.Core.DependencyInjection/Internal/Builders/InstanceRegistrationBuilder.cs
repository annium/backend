using System;
using Annium.Core.DependencyInjection.Internal.Builders.Units;

namespace Annium.Core.DependencyInjection.Internal.Builders
{
    internal class InstanceRegistrationBuilder : IInstanceRegistrationBuilderBase
    {
        public IInstanceRegistrationUnit Unit => _unit;
        private readonly Action<IServiceDescriptor> _register;
        private readonly InstanceRegistrationUnit _unit;

        public InstanceRegistrationBuilder(Type type, object instance, Action<IServiceDescriptor> register)
        {
            _register = register;
            _unit = new InstanceRegistrationUnit(type, instance);
        }

        public IInstanceRegistrationBuilderTarget As(Type serviceType)
        {
            throw new NotImplementedException();
        }

        public IInstanceRegistrationBuilderTarget AsFactory(Type serviceType)
        {
            throw new NotImplementedException();
        }

        public IInstanceRegistrationBuilderConfigure Configure(Action<IInstanceRegistrationUnit> configure)
        {
            throw new NotImplementedException();
        }

        public void Scoped()
        {
            throw new NotImplementedException();
        }

        public void Singleton()
        {
            throw new NotImplementedException();
        }

        public void Transient()
        {
            throw new NotImplementedException();
        }
    }
}