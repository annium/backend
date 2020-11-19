using System;
using System.Collections.Generic;
using System.Linq;
using Annium.Core.DependencyInjection.Internal.Builders.Units;

namespace Annium.Core.DependencyInjection.Internal.Builders
{
    internal class BulkRegistrationBuilder : IBulkRegistrationBuilderBase
    {
        public IReadOnlyCollection<IBulkRegistrationUnit> Units => _units;
        private readonly Action<IServiceDescriptor> _register;
        private readonly List<BulkRegistrationUnit> _units;

        public BulkRegistrationBuilder(IEnumerable<Type> types, Action<IServiceDescriptor> register)
        {
            _register = register;
            _units = new List<BulkRegistrationUnit>(types.Select(x => new BulkRegistrationUnit(x)));
        }

        public IBulkRegistrationBuilderBase Where(Func<Type, bool> predicate)
        {
            _units.RemoveAll(x => predicate(x.Type));

            return this;
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