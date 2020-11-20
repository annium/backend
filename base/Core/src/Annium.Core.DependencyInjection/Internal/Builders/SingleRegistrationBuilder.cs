using System;
using Annium.Core.DependencyInjection.Internal.Builders.Units;

namespace Annium.Core.DependencyInjection.Internal.Builders
{
    internal class SingleRegistrationBuilder : ISingleRegistrationBuilderBase
    {
        private readonly Action<IServiceDescriptor> _register;
        private readonly SingleRegistrationUnit _unit;

        public SingleRegistrationBuilder(Type type, Action<IServiceDescriptor> register)
        {
            _register = register;
            _unit = new SingleRegistrationUnit(type);
        }


        public ISingleRegistrationBuilderBase AsSelf()
        {
            throw new NotImplementedException();
        }

        public ISingleRegistrationBuilderBase As(Type serviceType)
        {
            throw new NotImplementedException();
        }

        public ISingleRegistrationBuilderBase As<T>()
        {
            throw new NotImplementedException();
        }

        public ISingleRegistrationBuilderBase AsSelfKeyed<TKey>(TKey key)
        {
            throw new NotImplementedException();
        }

        public ISingleRegistrationBuilderBase AsKeyed<TKey>(Type serviceType, TKey key)
        {
            throw new NotImplementedException();
        }

        public ISingleRegistrationBuilderBase AsKeyed<T, TKey>(TKey key)
        {
            throw new NotImplementedException();
        }

        public ISingleRegistrationBuilderBase AsSelfFactory()
        {
            throw new NotImplementedException();
        }

        public ISingleRegistrationBuilderBase AsFactory(Type serviceType)
        {
            throw new NotImplementedException();
        }

        public ISingleRegistrationBuilderBase AsFactory<T>()
        {
            throw new NotImplementedException();
        }

        public ISingleRegistrationBuilderBase AsSelfKeyedFactory<TKey>(TKey key)
        {
            throw new NotImplementedException();
        }

        public ISingleRegistrationBuilderBase AsKeyedFactory<TKey>(Type serviceType, TKey key)
        {
            throw new NotImplementedException();
        }

        public ISingleRegistrationBuilderBase AsKeyedFactory<T, TKey>(TKey key)
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