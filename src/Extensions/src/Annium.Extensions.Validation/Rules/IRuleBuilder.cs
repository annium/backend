using System;

namespace Annium.Extensions.Validation
{
    public interface IRuleBuilder<T>
    {
        IRuleBuilder<T> Add(Func<T, string> validate);
    }
}