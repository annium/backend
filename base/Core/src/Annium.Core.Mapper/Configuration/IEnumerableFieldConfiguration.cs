using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Annium.Core.Mapper
{
    public interface IEnumerableFieldConfiguration<F, I, S, D> : IFieldTerminalConfiguration<F, S, D>
    {
        IEnumerableFieldConfiguration<F, I, S, D> When(Func<S, F, bool> predicate);
        IEnumerableFieldConfiguration<F, I, S, D> When(Func<F, bool> predicate);
        IFieldItemConfiguration<SI, I, S, D> From<SI>(Expression<Func<S, IEnumerable<SI>>> itemExpression);
        IMapConfiguration<S, D> Ignore();
    }
}
