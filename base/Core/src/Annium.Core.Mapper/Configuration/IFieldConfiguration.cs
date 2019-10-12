using System;

namespace Annium.Core.Mapper
{
    public interface IFieldConfiguration<F, S, D> : IFieldTerminalConfiguration<F, S, D>
    {
        IFieldConfiguration<F, S, D> When(Func<S, F, bool> predicate);
        IFieldConfiguration<F, S, D> When(Func<F, bool> predicate);
        IMapConfiguration<S, D> With(Func<IMappingContext, S, F> map);
        IMapConfiguration<S, D> With(Func<S, F> map);
        IMapConfiguration<S, D> Ignore();
    }
}
