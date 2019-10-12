using System;

namespace Annium.Core.Mapper
{
    public interface IFieldItemConfiguration<SI, DI, S, D> : IFieldTerminalConfiguration<DI, S, D>
    {
        IFieldItemConfiguration<SI, DI, S, D> When(Func<S, SI, bool> predicate);
        IFieldItemConfiguration<SI, DI, S, D> When(Func<SI, bool> predicate);
        IMapConfiguration<S, D> With(Func<IMappingContext, SI, DI> map);
        IMapConfiguration<S, D> With(Func<SI, DI> map);
    }
}
