using System;

namespace Annium.Core.DependencyInjection.Internal.Builders
{
    internal class InstanceRegistrationBuilder : IInstanceRegistrationBuilderBase
    {
        public IInstanceRegistrationUnit Unit { get; }

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

        public IInstanceRegistrationBuilderConfigure Configure(Action<IInstanceRegistrationUnit> configure)
        {
            throw new NotImplementedException();
        }

        public IInstanceRegistrationBuilderTarget As(Type serviceType)
        {
            throw new NotImplementedException();
        }

        public IInstanceRegistrationBuilderTarget AsFactory(Type serviceType)
        {
            throw new NotImplementedException();
        }

        public IInstanceRegistrationBuilderBase Where(Func<Type, bool> predicate)
        {
            throw new NotImplementedException();
        }
    }
}