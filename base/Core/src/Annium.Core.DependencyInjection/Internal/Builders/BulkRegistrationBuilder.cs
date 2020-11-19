using System;
using System.Collections.Generic;

namespace Annium.Core.DependencyInjection.Internal.Builders
{
    internal class BulkRegistrationBuilder : IBulkRegistrationBuilderBase
    {
        public IReadOnlyCollection<IBulkRegistrationUnit> Units { get; }

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

        public IBulkRegistrationBuilderConfigure Configure(Action<IBulkRegistrationUnit> configure)
        {
            throw new NotImplementedException();
        }

        public IBulkRegistrationBuilderTarget As(Type serviceType)
        {
            throw new NotImplementedException();
        }

        public IBulkRegistrationBuilderTarget AsFactory(Type serviceType)
        {
            throw new NotImplementedException();
        }

        public IBulkRegistrationBuilderBase Where(Func<Type, bool> predicate)
        {
            throw new NotImplementedException();
        }
    }
}