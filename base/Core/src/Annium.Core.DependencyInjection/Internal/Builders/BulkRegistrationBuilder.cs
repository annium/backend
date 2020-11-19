using System;
using System.Collections.Generic;
using Annium.Core.DependencyInjection.Internal.Builders.Units;

namespace Annium.Core.DependencyInjection.Internal.Builders
{
    internal class BulkRegistrationBuilder : IBulkRegistrationBuilderBase
    {
        public IReadOnlyCollection<IBulkRegistrationUnit> Units => _units;
        private readonly List<BulkRegistrationUnit> _units = new();

        public BulkRegistrationBuilder(IEnumerable<Type> types)
        {
        }

        public IBulkRegistrationBuilderBase Where(Func<Type, bool> predicate)
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

        public IBulkRegistrationBuilderConfigure Configure(Action<IBulkRegistrationUnit> configure)
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