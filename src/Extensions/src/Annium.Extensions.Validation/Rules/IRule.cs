using System;
using Annium.Data.Operations;

namespace Annium.Extensions.Validation
{
    public interface IRule<T>
    {
        IRule<T> Add(Func<T, string> validate);
    }
}