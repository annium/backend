using System;
using System.Threading.Tasks;

namespace Annium.Extensions.Validation
{
    internal class AsyncRule<TValue, TField> : IRule<TValue, TField>
    {
        public Func<ValidationContext<TValue>, TField, Task> Validate { get; }

        public AsyncRule(
            Func<ValidationContext<TValue>, TField, Task> validate
        )
        {
            Validate = validate;
        }
    }
}