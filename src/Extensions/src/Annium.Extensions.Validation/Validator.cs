using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;
using Annium.Core.Application.Types;
using Annium.Data.Operations;

namespace Annium.Extensions.Validation
{
    public abstract class Validator<TValue>
    {
        private IDictionary<PropertyInfo, IRuleContainer<TValue>> rules = new Dictionary<PropertyInfo, IRuleContainer<TValue>>();

        protected IRuleBuilder<TValue, TField> Field<TField>(Expression<Func<TValue, TField>> accessor)
        {
            if (accessor is null)
                throw new ArgumentNullException(nameof(accessor));

            var property = TypeHelper.ResolveProperty(accessor);
            var rule = new Rule<TValue, TField>(accessor.Compile());

            rules[property] = rule;

            return rule;
        }

        public async Task<BooleanResult> Validate(TValue value, string label = null)
        {
            var hasLabel = !string.IsNullOrWhiteSpace(label);

            if (value == null)
                return hasLabel ?
                    Result.Failure().Error(label, "Value is null") :
                    Result.Failure().Error("Value is null");

            var result = Result.Success();
            foreach (var(property, rule) in rules)
            {
                var propertyLabel = hasLabel ? $"{label}.{property.Name}" : property.Name;
                var context = new ValidationContext<TValue>(value, propertyLabel, property.Name, result);
                await rule.Validate(value, context);
            }

            return result;
        }
    }
}