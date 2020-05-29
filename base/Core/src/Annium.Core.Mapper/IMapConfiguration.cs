using System;
using System.Linq.Expressions;

namespace Annium.Core.Mapper
{
    public interface IMapConfiguration<S, T>
    {
        void With(Expression<Func<S, T>> map);

        IMapConfiguration<S, T> For(
            Expression<Func<T, object>> members,
            Expression<Func<S, object>> map
        );

        IMapConfiguration<S, T> Ignore(
            Expression<Func<T, object>> members
        );
    }
}