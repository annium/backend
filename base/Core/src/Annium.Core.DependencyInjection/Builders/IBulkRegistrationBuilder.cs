using System;
using System.Collections.Generic;

namespace Annium.Core.DependencyInjection
{
    public interface IBulkRegistrationBuilderBase : IBulkRegistrationBuilderTarget
    {
        IBulkRegistrationBuilderBase Where(Func<Type, bool> predicate);
    }

    public interface IBulkRegistrationBuilderTarget : IBulkRegistrationBuilderConfigure
    {
        IBulkRegistrationBuilderTarget As(Type serviceType);
        IBulkRegistrationBuilderTarget AsFactory(Type serviceType);
    }

    public interface IBulkRegistrationBuilderConfigure : IBulkRegistrationBuilderLifetime
    {
        IBulkRegistrationBuilderConfigure Configure(Action<IBulkRegistrationUnit> configure);
    }

    public interface IBulkRegistrationBuilderLifetime
    {
        IReadOnlyCollection<IBulkRegistrationUnit> Units { get; }
        void Scoped();
        void Singleton();
        void Transient();
    }
}