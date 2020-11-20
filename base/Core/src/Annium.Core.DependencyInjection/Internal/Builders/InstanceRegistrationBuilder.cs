using System;
using Annium.Core.DependencyInjection.Internal.Builders.Units;

namespace Annium.Core.DependencyInjection.Internal.Builders
{
    internal class InstanceRegistrationBuilder : IInstanceRegistrationBuilderBase
    {
        private readonly Action<IServiceDescriptor> _register;
        private readonly InstanceRegistrationUnit _unit;

        public InstanceRegistrationBuilder(Type type, object instance, Action<IServiceDescriptor> register)
        {
            _register = register;
            _unit = new InstanceRegistrationUnit(type, instance);
        }

        public IInstanceRegistrationBuilderBase AsSelf()
        {
            throw new NotImplementedException();
        }

        public IInstanceRegistrationBuilderBase As(Type serviceType)
        {
            throw new NotImplementedException();
        }

        public IInstanceRegistrationBuilderBase As<T>()
        {
            throw new NotImplementedException();
        }

        public IInstanceRegistrationBuilderBase AsSelfKeyed<TKey>(TKey key)
        {
            throw new NotImplementedException();
        }

        public IInstanceRegistrationBuilderBase AsKeyed<TKey>(Type serviceType, TKey key)
        {
            throw new NotImplementedException();
        }

        public IInstanceRegistrationBuilderBase AsKeyed<T, TKey>(TKey key)
        {
            throw new NotImplementedException();
        }

        public IInstanceRegistrationBuilderBase AsSelfFactory()
        {
            throw new NotImplementedException();
        }

        public IInstanceRegistrationBuilderBase AsFactory(Type serviceType)
        {
            throw new NotImplementedException();
        }

        public IInstanceRegistrationBuilderBase AsFactory<T>()
        {
            throw new NotImplementedException();
        }

        public IInstanceRegistrationBuilderBase AsSelfKeyedFactory<TKey>(TKey key)
        {
            throw new NotImplementedException();
        }

        public IInstanceRegistrationBuilderBase AsKeyedFactory<TKey>(Type serviceType, TKey key)
        {
            throw new NotImplementedException();
        }

        public IInstanceRegistrationBuilderBase AsKeyedFactory<T, TKey>(TKey key)
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