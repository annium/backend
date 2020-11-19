using System;

namespace Annium.Core.DependencyInjection.Internal.Builders
{
    internal class SingleRegistrationBuilder : ISingleRegistrationBuilderBase
    {
        public ISingleRegistrationUnit Unit { get; }

        public SingleRegistrationBuilder(Type type)
        {
        }

        public ISingleRegistrationBuilderBase Where(Func<Type, bool> predicate)
        {
            throw new NotImplementedException();
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