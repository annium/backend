using System;
using Annium.Core.DependencyInjection.Internal.Builders.Units;

namespace Annium.Core.DependencyInjection.Internal.Builders
{
    internal class FactoryRegistrationBuilder : IFactoryRegistrationBuilderBase
    {
        private readonly Action<IServiceDescriptor> _register;
        private readonly FactoryRegistrationUnit _unit;

        public FactoryRegistrationBuilder(Type type, Func<IServiceProvider, object> factory, Action<IServiceDescriptor> register)
        {
            _register = register;
            _unit = new FactoryRegistrationUnit(type);
        }

        public IFactoryRegistrationBuilderBase AsSelf()
        {
            throw new NotImplementedException();
        }

        public IFactoryRegistrationBuilderBase As(Type serviceType)
        {
            throw new NotImplementedException();
        }

        public IFactoryRegistrationBuilderBase As<T>()
        {
            throw new NotImplementedException();
        }

        public IFactoryRegistrationBuilderBase AsSelfKeyed<TKey>(TKey key)
        {
            throw new NotImplementedException();
        }

        public IFactoryRegistrationBuilderBase AsKeyed<TKey>(Type serviceType, TKey key)
        {
            throw new NotImplementedException();
        }

        public IFactoryRegistrationBuilderBase AsKeyed<T, TKey>(TKey key)
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