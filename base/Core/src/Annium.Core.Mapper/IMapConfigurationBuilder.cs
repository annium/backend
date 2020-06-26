using System;
using System.Linq.Expressions;

namespace Annium.Core.Mapper
{
    public interface IMapConfigurationBuilder<S, T>
    {
        void With(Expression<Func<S, T>> map);

        IMapConfigurationBuilder<S, T> For<F>(
            Expression<Func<T, object>> members,
            Expression<Func<S, F>> map
        );

        IMapConfigurationBuilder<S, T> Ignore(
            Expression<Func<T, object>> members
        );
    }
}