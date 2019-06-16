using System;
using System.Threading.Tasks;

namespace Annium.Extensions.Validation
{
    public interface IRuleBuilder<TValue, TField>
    {
        IRuleBuilder<TValue, TField> Add(Action<ValidationContext<TValue>, TField> validate);

        IRuleBuilder<TValue, TField> Add(Func<ValidationContext<TValue>, TField, Task> validate);
    }
}