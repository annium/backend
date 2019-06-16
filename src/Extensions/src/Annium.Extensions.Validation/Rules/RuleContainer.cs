using System;
using System.Collections.Generic;
using Annium.Data.Operations;

namespace Annium.Extensions.Validation
{
    internal class Rule<TValue, TField> : IRuleContainer<TValue>, IRuleBuilder<TField>
    {
        private readonly Func<TValue, TField> getField;

        private readonly IList<Func<TField, string>> chain = new List<Func<TField, string>>();

        public Rule(
            Func<TValue, TField> getField
        )
        {
            this.getField = getField;
        }

        public IRuleBuilder<TField> Add(Func<TField, string> validate)
        {
            chain.Add(validate);

            return this;
        }

        public BooleanResult Validate(TValue value, ValidationContext<TValue> context)
        {
            var field = getField(value);
            var result = Result.Success();

            foreach (var validate in chain)
            {
                var error = validate(field);
                if (error != null)
                    result.Error(context.Label, error);
            }

            return result;
        }
    }
}