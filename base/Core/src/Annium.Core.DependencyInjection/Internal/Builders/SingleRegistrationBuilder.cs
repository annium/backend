using System;
using Annium.Core.DependencyInjection.Internal.Builders.Units;

namespace Annium.Core.DependencyInjection.Internal.Builders
{
    internal class SingleRegistrationBuilder : ISingleRegistrationBuilderBase
    {
        public ISingleRegistrationUnit Unit => _unit;
        private readonly Action<IServiceDescriptor> _register;
        private readonly SingleRegistrationUnit _unit;

        public SingleRegistrationBuilder(Type type, Action<IServiceDescriptor> register)
        {
            _register = register;
            _unit = new SingleRegistrationUnit(type);
        }

        public ISingleRegistrationBuilderTarget As(Type serviceType)
        {
            throw new NotImplementedException();
        }

        public ISingleRegistrationBuilderTarget AsFactory(Type serviceType)
        {
            throw new NotImplementedException();
        }

        public ISingleRegistrationBuilderConfigure Configure(Action<ISingleRegistrationUnit> configure)
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