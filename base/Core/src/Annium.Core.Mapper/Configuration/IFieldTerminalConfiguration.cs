using System;

namespace Annium.Core.Mapper
{
    public interface IFieldTerminalConfiguration<F, S, D>
    {
        IMapConfiguration<S, D> Throw<E>(Func<S, F, E> buildException) where E : Exception;
        IMapConfiguration<S, D> Throw<E>(Func<F, E> buildException) where E : Exception;
        IMapConfiguration<S, D> Throw<E>(Func<E> buildException) where E : Exception;
    }
}